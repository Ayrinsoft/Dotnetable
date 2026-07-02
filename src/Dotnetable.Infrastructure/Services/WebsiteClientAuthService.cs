using System.Security.Cryptography;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Customer authentication for the public website. Activation and password-reset codes are stored in
/// <c>WebsiteClientForgetPassword</c> (one live code per customer) and delivered by email or SMS.
/// </summary>
public class WebsiteClientAuthService : IWebsiteClientAuthService
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(30);

    private readonly AppDbContext _context;
    private readonly IEmailService _email;
    private readonly ISmsSender _sms;
    private readonly IPasswordHasher<WebsiteClient> _hasher;

    public WebsiteClientAuthService(
        AppDbContext context,
        IEmailService email,
        ISmsSender sms,
        IPasswordHasher<WebsiteClient> hasher)
    {
        _context = context;
        _email = email;
        _sms = sms;
        _hasher = hasher;
    }

    public async Task<ClientRegisterResponse> RegisterAsync(ClientRegistration registration, CancellationToken ct = default)
    {
        var email = Normalize(registration.Email);
        var cellphone = Normalize(registration.Cellphone);
        var countryCode = Normalize(registration.CountryCode);

        // Choose the channel: email takes precedence when both are supplied.
        OtpChannel channel;
        string identifier;
        if (email is not null && LooksLikeEmail(email))
        {
            channel = OtpChannel.Email;
            identifier = email;
        }
        else if (cellphone is not null)
        {
            channel = OtpChannel.Sms;
            identifier = cellphone;
        }
        else
        {
            return new ClientRegisterResponse(ClientRegisterResult.InvalidInput, OtpChannel.Email, string.Empty);
        }

        if (string.IsNullOrWhiteSpace(registration.Password))
            return new ClientRegisterResponse(ClientRegisterResult.InvalidInput, channel, identifier);

        // Email must be truly deliverable; the SMS stub always "delivers" (it logs the code).
        if (channel == OtpChannel.Email && !await _email.IsConfiguredAsync(ct))
            return new ClientRegisterResponse(ClientRegisterResult.DeliveryNotConfigured, channel, identifier);

        var existing = await FindAsync(registration.WebsiteId, identifier, ct);
        if (existing is { Active: true })
            return new ClientRegisterResponse(ClientRegisterResult.AlreadyRegistered, channel, identifier);

        WebsiteClient client;
        if (existing is not null)
        {
            // Re-registering an inactive account: refresh details, don't create a new record.
            client = existing;
            client.Givenname = Normalize(registration.GivenName);
            client.Surname = Normalize(registration.Surname);
            client.Password = _hasher.HashPassword(client, registration.Password);
        }
        else
        {
            client = new WebsiteClient
            {
                WebsiteID = registration.WebsiteId,
                Email = channel == OtpChannel.Email ? email : null,
                Cellphone = channel == OtpChannel.Sms ? cellphone : null,
                CountryCode = channel == OtpChannel.Sms ? countryCode : null,
                Givenname = Normalize(registration.GivenName),
                Surname = Normalize(registration.Surname),
                Active = false,
                RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
                HashKey = Guid.NewGuid(),
                ClientLevel = ClientLevel.Normal,
            };
            client.Password = _hasher.HashPassword(client, registration.Password);
            _context.WebsiteClients.Add(client);
        }

        await _context.SaveChangesAsync(ct);

        var code = await IssueCodeAsync(client.WebsiteClientID, ct);
        await SendCodeAsync(channel, identifier, countryCode, code, isActivation: true, ct);

        return new ClientRegisterResponse(ClientRegisterResult.OtpSent, channel, identifier);
    }

    public async Task<(ClientVerifyResult Result, WebsiteClient? Client)> VerifyOtpAsync(
        int websiteId, string identifier, string code, CancellationToken ct = default)
    {
        var client = await FindAsync(websiteId, Normalize(identifier) ?? string.Empty, ct);
        if (client is null) return (ClientVerifyResult.NotFound, null);
        if (client.Active) return (ClientVerifyResult.AlreadyActive, client);

        if (!await IsCodeValidAsync(client.WebsiteClientID, code, ct))
            return (ClientVerifyResult.InvalidCode, null);

        client.Active = true;
        await ClearCodesAsync(client.WebsiteClientID, ct);
        await _context.SaveChangesAsync(ct);
        return (ClientVerifyResult.Success, client);
    }

    public async Task<ClientResendResult> ResendOtpAsync(int websiteId, string identifier, CancellationToken ct = default)
    {
        var client = await FindAsync(websiteId, Normalize(identifier) ?? string.Empty, ct);
        if (client is null) return ClientResendResult.NotFound;
        if (client.Active) return ClientResendResult.AlreadyActive;

        var (channel, target) = ChannelFor(client);
        if (channel == OtpChannel.Email && !await _email.IsConfiguredAsync(ct))
            return ClientResendResult.DeliveryNotConfigured;

        var code = await IssueCodeAsync(client.WebsiteClientID, ct);
        await SendCodeAsync(channel, target, client.CountryCode, code, isActivation: true, ct);
        return ClientResendResult.OtpSent;
    }

    public async Task<(ClientLoginStatus Status, WebsiteClient? Client)> ValidateCredentialsAsync(
        int websiteId, string identifier, string password, CancellationToken ct = default)
    {
        var client = await FindAsync(websiteId, Normalize(identifier) ?? string.Empty, ct);
        if (client is null || string.IsNullOrEmpty(client.Password))
            return (ClientLoginStatus.InvalidCredentials, null);

        var result = _hasher.VerifyHashedPassword(client, client.Password, password);
        if (result == PasswordVerificationResult.Failed)
            return (ClientLoginStatus.InvalidCredentials, null);

        return client.Active
            ? (ClientLoginStatus.Success, client)
            : (ClientLoginStatus.NotActivated, client);
    }

    public async Task<ClientResetRequestResult> RequestPasswordResetAsync(int websiteId, string identifier, CancellationToken ct = default)
    {
        var client = await FindAsync(websiteId, Normalize(identifier) ?? string.Empty, ct);
        if (client is null) return ClientResetRequestResult.NotFound;

        var (channel, target) = ChannelFor(client);
        if (channel == OtpChannel.Email && !await _email.IsConfiguredAsync(ct))
            return ClientResetRequestResult.DeliveryNotConfigured;

        var code = await IssueCodeAsync(client.WebsiteClientID, ct);
        await SendCodeAsync(channel, target, client.CountryCode, code, isActivation: false, ct);
        return ClientResetRequestResult.OtpSent;
    }

    public async Task<ClientResetResult> ResetPasswordAsync(
        int websiteId, string identifier, string code, string newPassword, CancellationToken ct = default)
    {
        var client = await FindAsync(websiteId, Normalize(identifier) ?? string.Empty, ct);
        if (client is null) return ClientResetResult.NotFound;

        if (!await IsCodeValidAsync(client.WebsiteClientID, code, ct))
            return ClientResetResult.InvalidCode;

        client.HashKey = Guid.NewGuid();
        client.Password = _hasher.HashPassword(client, newPassword);
        // Activate the account too — proving control of the email/mobile is enough.
        client.Active = true;
        await ClearCodesAsync(client.WebsiteClientID, ct);
        await _context.SaveChangesAsync(ct);
        return ClientResetResult.Success;
    }

    // ── Helpers ─────────────────────────────────────────────────────

    private Task<WebsiteClient?> FindAsync(int websiteId, string needle, CancellationToken ct) =>
        _context.WebsiteClients.FirstOrDefaultAsync(
            c => c.WebsiteID == websiteId && (c.Email == needle || c.Cellphone == needle), ct);

    private static (OtpChannel Channel, string Target) ChannelFor(WebsiteClient client) =>
        !string.IsNullOrWhiteSpace(client.Email)
            ? (OtpChannel.Email, client.Email!)
            : (OtpChannel.Sms, client.Cellphone ?? string.Empty);

    /// <summary>Replaces any outstanding code for the customer with a fresh 6-digit one.</summary>
    private async Task<string> IssueCodeAsync(int clientId, CancellationToken ct)
    {
        await ClearCodesAsync(clientId, ct);
        var code = GenerateCode();
        _context.WebsiteClientForgetPasswords.Add(new WebsiteClientForgetPassword
        {
            WebsiteClientID = clientId,
            ForgetKey = code,
            LogTime = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync(ct);
        return code;
    }

    private async Task<bool> IsCodeValidAsync(int clientId, string code, CancellationToken ct)
    {
        var normalized = Normalize(code);
        if (normalized is null) return false;
        var cutoff = DateTime.UtcNow - CodeLifetime;
        return await _context.WebsiteClientForgetPasswords.AnyAsync(
            f => f.WebsiteClientID == clientId && f.ForgetKey == normalized && f.LogTime >= cutoff, ct);
    }

    private async Task ClearCodesAsync(int clientId, CancellationToken ct)
    {
        var stale = _context.WebsiteClientForgetPasswords.Where(f => f.WebsiteClientID == clientId);
        _context.WebsiteClientForgetPasswords.RemoveRange(stale);
        await _context.SaveChangesAsync(ct);
    }

    private async Task SendCodeAsync(
        OtpChannel channel, string target, string? countryCode, string code, bool isActivation, CancellationToken ct)
    {
        if (channel == OtpChannel.Email)
        {
            var subject = isActivation ? "Your activation code" : "Your password reset code";
            var intro = isActivation
                ? "Use the code below to activate your account."
                : "Use the code below to reset your password.";
            var body =
                $"<p>{intro}</p>" +
                $"<p style=\"font-size:24px;font-weight:bold;letter-spacing:3px\">{code}</p>" +
                "<p>This code expires in 30 minutes. If you didn't request it, you can ignore this message.</p>";
            await _email.SendAsync(target, subject, body, ct);
        }
        else
        {
            var verb = isActivation ? "activation" : "password reset";
            await _sms.SendAsync(countryCode ?? string.Empty, target, $"Your {verb} code is {code}", ct);
        }
    }

    private static string GenerateCode() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool LooksLikeEmail(string value) => value.Contains('@');
}
