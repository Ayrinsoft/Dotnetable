using System.Security.Cryptography;
using System.Text;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Email;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Security;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Customer authentication for the public website. Activation and password-reset codes are stored in
/// <c>WebsiteClientForgetPassword</c> (one live code per customer) and delivered by email or SMS.
///
/// <para>A 6-digit code has only a million values, so the code row itself counts wrong guesses and
/// dies at <see cref="MaxCodeAttempts"/>; without that, the 30-minute window is enough to walk the
/// whole space and take over any account whose address an attacker knows. Repeated wrong passwords
/// lock the account for <see cref="LockoutWindow"/> on the same principle, and a new code cannot be
/// requested more often than <see cref="ResendCooldown"/> so the endpoint cannot be used to bill the
/// site for SMS. These are the per-account limits; the per-IP limits live in the API rate limiter,
/// and both are needed — one attacker against many accounts, many attackers against one.</para>
/// </summary>
public class WebsiteClientAuthService : IWebsiteClientAuthService
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(30);

    /// <summary>Wrong codes tolerated before the live code is destroyed.</summary>
    private const int MaxCodeAttempts = 5;

    /// <summary>How long a burnt code keeps refusing verification, so retrying costs real time.</summary>
    private static readonly TimeSpan CodeLockout = TimeSpan.FromMinutes(15);

    /// <summary>Minimum gap between two code deliveries to the same account.</summary>
    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);

    /// <summary>Wrong passwords tolerated before the account is locked.</summary>
    private const int MaxLoginAttempts = 8;

    /// <summary>How long an account stays locked after <see cref="MaxLoginAttempts"/> failures.</summary>
    private static readonly TimeSpan LockoutWindow = TimeSpan.FromMinutes(15);

    private readonly AppDbContext _context;
    private readonly IEmailService _email;
    private readonly ISmsSender _sms;
    private readonly IPasswordHasher<WebsiteClient> _hasher;
    private readonly IAdminNotificationService _notifications;

    public WebsiteClientAuthService(
        AppDbContext context,
        IEmailService email,
        ISmsSender sms,
        IPasswordHasher<WebsiteClient> hasher,
        IAdminNotificationService notifications)
    {
        _context = context;
        _email = email;
        _sms = sms;
        _hasher = hasher;
        _notifications = notifications;
    }

    public async Task<ClientRegisterResponse> RegisterAsync(ClientRegistration registration, CancellationToken ct = default)
    {
        var email = Normalize(registration.Email);
        var cellphone = Normalize(registration.Cellphone);
        var countryCode = Normalize(registration.CountryCode);

        // Ignore a malformed email; keep only real identifiers.
        if (email is not null && !LooksLikeEmail(email)) email = null;

        // At least one identifier and a password are required.
        if ((email is null && cellphone is null) || string.IsNullOrWhiteSpace(registration.Password))
        {
            var badChannel = email is not null ? OtpChannel.Email : OtpChannel.Sms;
            return new ClientRegisterResponse(
                ClientRegisterResult.InvalidInput, badChannel, email ?? cellphone ?? string.Empty);
        }

        // The activation code goes to the email when present, otherwise the mobile — but BOTH
        // identifiers are stored and reserved so no one else can take either of them.
        var channel = email is not null ? OtpChannel.Email : OtpChannel.Sms;
        var identifier = email ?? cellphone!;

        var policy = PasswordPolicy.Validate(registration.Password, email, cellphone);
        if (!policy.Ok)
            return new ClientRegisterResponse(ClientRegisterResult.WeakPassword, channel, identifier, policy.Error);

        // The code has to be deliverable before an account is created for it — otherwise the customer
        // is left with an inactive account and no way to activate it. This is also what stops
        // mobile-only sign-up on a site with no SMS gateway configured.
        if (!await CanDeliverAsync(registration.WebsiteId, channel, ct))
            return new ClientRegisterResponse(ClientRegisterResult.DeliveryNotConfigured, channel, identifier);

        // Every existing customer in this website that already owns the email or the mobile.
        var matches = await _context.WebsiteClients
            .Where(c => c.WebsiteID == registration.WebsiteId &&
                        ((email != null && c.Email == email) || (cellphone != null && c.Cellphone == cellphone)))
            .ToListAsync(ct);

        // Taken if any owner is active, or the two identifiers already belong to two different accounts.
        if (matches.Any(c => c.Active) ||
            matches.Select(c => c.WebsiteClientID).Distinct().Count() > 1)
            return new ClientRegisterResponse(ClientRegisterResult.AlreadyRegistered, channel, identifier);

        WebsiteClient client;
        if (matches.Count == 1)
        {
            // Re-registering a single inactive account: refresh its details, do not create a new record.
            client = matches[0];
            client.Email = email ?? client.Email;
            client.Cellphone = cellphone ?? client.Cellphone;
            if (cellphone is not null) client.CountryCode = countryCode;
            client.Givenname = Normalize(registration.GivenName);
            client.Surname = Normalize(registration.Surname);
            client.Password = _hasher.HashPassword(client, registration.Password);
        }
        else
        {
            client = new WebsiteClient
            {
                WebsiteID = registration.WebsiteId,
                Email = email,
                Cellphone = cellphone,
                CountryCode = cellphone is not null ? countryCode : null,
                Givenname = Normalize(registration.GivenName),
                Surname = Normalize(registration.Surname),
                Active = false,
                RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
                HashKey = Guid.NewGuid(),
                ClientLevel = (byte)ClientLevel.Normal,
            };
            client.Password = _hasher.HashPassword(client, registration.Password);
            _context.WebsiteClients.Add(client);
        }

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Lost a race against a concurrent registration for the same email/mobile
            // (the per-website unique index rejected the insert).
            return new ClientRegisterResponse(ClientRegisterResult.AlreadyRegistered, channel, identifier);
        }

        var code = await IssueCodeAsync(client.WebsiteClientID, ct);
        await SendCodeAsync(client.WebsiteID, channel, identifier, countryCode, code, isActivation: true, ct);

        var clientLabel = string.Join(" ", new[] { client.Givenname, client.Surname }.Where(s => !string.IsNullOrWhiteSpace(s)));
        if (string.IsNullOrWhiteSpace(clientLabel)) clientLabel = identifier;
        await _notifications.NotifySiteAdminsAsync(
            client.WebsiteID,
            AdminNotificationType.ClientRegistered,
            "New customer registration",
            $"{clientLabel} registered ({identifier}).",
            $"/clients/{client.WebsiteClientID}",
            client.WebsiteClientID,
            ct);

        return new ClientRegisterResponse(ClientRegisterResult.OtpSent, channel, identifier);
    }

    public async Task<(ClientVerifyResult Result, WebsiteClient? Client)> VerifyOtpAsync(
        int websiteId, string identifier, string code, CancellationToken ct = default)
    {
        var client = await FindAsync(websiteId, Normalize(identifier) ?? string.Empty, ct);
        if (client is null) return (ClientVerifyResult.NotFound, null);
        if (client.Active) return (ClientVerifyResult.AlreadyActive, client);

        var check = await CheckCodeAsync(client.WebsiteClientID, code, ct);
        if (check == CodeCheck.TooManyAttempts) return (ClientVerifyResult.TooManyAttempts, null);
        if (check == CodeCheck.Invalid) return (ClientVerifyResult.InvalidCode, null);

        client.Active = true;
        client.FailedLoginCount = 0;
        client.LockoutEndUtc = null;
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
        if (!await CanDeliverAsync(client.WebsiteID, channel, ct))
            return ClientResendResult.DeliveryNotConfigured;

        if (await IsWithinResendCooldownAsync(client.WebsiteClientID, ct))
            return ClientResendResult.TooSoon;

        var code = await IssueCodeAsync(client.WebsiteClientID, ct);
        await SendCodeAsync(client.WebsiteID, channel, target, client.CountryCode, code, isActivation: true, ct);
        return ClientResendResult.OtpSent;
    }

    public async Task<(ClientLoginStatus Status, WebsiteClient? Client)> ValidateCredentialsAsync(
        int websiteId, string identifier, string password, CancellationToken ct = default)
    {
        var client = await FindAsync(websiteId, Normalize(identifier) ?? string.Empty, ct);
        if (client is null || string.IsNullOrEmpty(client.Password))
            return (ClientLoginStatus.InvalidCredentials, null);

        if (client.LockoutEndUtc is DateTime until && until > DateTime.UtcNow)
            return (ClientLoginStatus.LockedOut, null);

        var result = _hasher.VerifyHashedPassword(client, client.Password, password);
        if (result == PasswordVerificationResult.Failed)
        {
            client.FailedLoginCount++;
            if (client.FailedLoginCount >= MaxLoginAttempts)
            {
                client.LockoutEndUtc = DateTime.UtcNow.Add(LockoutWindow);
                client.FailedLoginCount = 0;
            }
            await _context.SaveChangesAsync(ct);
            return (client.LockoutEndUtc is not null
                ? ClientLoginStatus.LockedOut
                : ClientLoginStatus.InvalidCredentials, null);
        }

        if (client.FailedLoginCount != 0 || client.LockoutEndUtc is not null)
        {
            client.FailedLoginCount = 0;
            client.LockoutEndUtc = null;
            await _context.SaveChangesAsync(ct);
        }

        // A hash written under older Identity parameters is upgraded on the next successful sign-in,
        // which is the only moment the plaintext is available to rehash from.
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            client.Password = _hasher.HashPassword(client, password);
            await _context.SaveChangesAsync(ct);
        }

        return client.Active
            ? (ClientLoginStatus.Success, client)
            : (ClientLoginStatus.NotActivated, client);
    }

    public async Task<ClientResetRequestResult> RequestPasswordResetAsync(int websiteId, string identifier, CancellationToken ct = default)
    {
        var client = await FindAsync(websiteId, Normalize(identifier) ?? string.Empty, ct);
        if (client is null) return ClientResetRequestResult.NotFound;

        var (channel, target) = ChannelFor(client);
        if (!await CanDeliverAsync(client.WebsiteID, channel, ct))
            return ClientResetRequestResult.DeliveryNotConfigured;

        // Silently succeed inside the cooldown: the caller reports a generic "if the account exists"
        // message either way, so this neither leaks existence nor re-sends.
        if (await IsWithinResendCooldownAsync(client.WebsiteClientID, ct))
            return ClientResetRequestResult.OtpSent;

        var code = await IssueCodeAsync(client.WebsiteClientID, ct);
        await SendCodeAsync(client.WebsiteID, channel, target, client.CountryCode, code, isActivation: false, ct);
        return ClientResetRequestResult.OtpSent;
    }

    public async Task<ClientResetResult> ResetPasswordAsync(
        int websiteId, string identifier, string code, string newPassword, CancellationToken ct = default)
    {
        var client = await FindAsync(websiteId, Normalize(identifier) ?? string.Empty, ct);
        if (client is null) return ClientResetResult.NotFound;

        var check = await CheckCodeAsync(client.WebsiteClientID, code, ct);
        if (check == CodeCheck.TooManyAttempts) return ClientResetResult.TooManyAttempts;
        if (check == CodeCheck.Invalid) return ClientResetResult.InvalidCode;

        // Only validated after the code proves control of the address, so a rejected password never
        // reveals whether the code was right.
        if (!PasswordPolicy.Validate(newPassword, client.Email, client.Cellphone).Ok)
            return ClientResetResult.WeakPassword;

        // Rotating HashKey invalidates anything derived from it; the refresh tokens are revoked so a
        // session stolen before the reset cannot outlive it.
        client.HashKey = Guid.NewGuid();
        client.Password = _hasher.HashPassword(client, newPassword);
        client.FailedLoginCount = 0;
        client.LockoutEndUtc = null;
        // Activate the account too — proving control of the email/mobile is enough.
        client.Active = true;
        await ClearCodesAsync(client.WebsiteClientID, ct);
        await RevokeAllRefreshTokensAsync(client.WebsiteClientID, ct);
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

    /// <summary>True when a code sent over <paramref name="channel"/> would actually reach the customer.</summary>
    private async Task<bool> CanDeliverAsync(int websiteId, OtpChannel channel, CancellationToken ct) =>
        channel == OtpChannel.Email
            ? await _email.IsConfiguredAsync(websiteId, ct)
            : await _sms.IsConfiguredAsync(websiteId, ct);

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
            FailedAttempts = 0,
            LockedUntil = null,
        });
        await _context.SaveChangesAsync(ct);
        return code;
    }

    private async Task<bool> IsWithinResendCooldownAsync(int clientId, CancellationToken ct)
    {
        var since = DateTime.UtcNow - ResendCooldown;
        return await _context.WebsiteClientForgetPasswords
            .AnyAsync(f => f.WebsiteClientID == clientId && f.LogTime >= since, ct);
    }

    private enum CodeCheck { Valid, Invalid, TooManyAttempts }

    /// <summary>
    /// Compares the presented code against the live one, charging a failed attempt for every miss.
    /// The comparison is length-constant so timing cannot be used to learn a prefix, and the code is
    /// destroyed once the attempt budget is spent.
    /// </summary>
    private async Task<CodeCheck> CheckCodeAsync(int clientId, string code, CancellationToken ct)
    {
        var normalized = Normalize(code);
        var cutoff = DateTime.UtcNow - CodeLifetime;

        var row = await _context.WebsiteClientForgetPasswords
            .Where(f => f.WebsiteClientID == clientId && f.LogTime >= cutoff)
            .OrderByDescending(f => f.WebsiteClientForgetPasswordID)
            .FirstOrDefaultAsync(ct);

        if (row is null) return CodeCheck.Invalid;

        if (row.LockedUntil is DateTime until && until > DateTime.UtcNow)
            return CodeCheck.TooManyAttempts;

        if (normalized is not null && FixedTimeEquals(row.ForgetKey, normalized))
            return CodeCheck.Valid;

        row.FailedAttempts++;
        if (row.FailedAttempts >= MaxCodeAttempts)
        {
            row.LockedUntil = DateTime.UtcNow.Add(CodeLockout);
            // Burn the code itself so even the correct value stops working; a new one must be sent.
            row.ForgetKey = GenerateCode();
        }
        await _context.SaveChangesAsync(ct);

        return row.LockedUntil is not null ? CodeCheck.TooManyAttempts : CodeCheck.Invalid;
    }

    private async Task ClearCodesAsync(int clientId, CancellationToken ct)
    {
        var stale = _context.WebsiteClientForgetPasswords.Where(f => f.WebsiteClientID == clientId);
        _context.WebsiteClientForgetPasswords.RemoveRange(stale);
        await _context.SaveChangesAsync(ct);
    }

    private Task RevokeAllRefreshTokensAsync(int clientId, CancellationToken ct) =>
        _context.WebsiteClientRefreshTokens
            .Where(t => t.WebsiteClientID == clientId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, (DateTime?)DateTime.UtcNow), ct);

    private async Task SendCodeAsync(
        int websiteId, OtpChannel channel, string target, string? countryCode, string code, bool isActivation, CancellationToken ct)
    {
        if (channel == OtpChannel.Email)
        {
            var key = isActivation ? EmailTemplateKeys.ClientOtpActivation : EmailTemplateKeys.ClientOtpPasswordReset;
            await _email.SendTemplateAsync(websiteId, key, target, new Dictionary<string, string> { ["Code"] = code },
                languageCode: null, ct);
        }
        else
        {
            var verb = isActivation ? "activation" : "password reset";
            await _sms.SendAsync(websiteId, countryCode ?? string.Empty, target, $"Your {verb} code is {code}", ct);
        }
    }

    private static string GenerateCode() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    /// <summary>Length-constant string compare, so a wrong code leaks no timing signal about its prefix.</summary>
    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool LooksLikeEmail(string value) => value.Contains('@');
}
