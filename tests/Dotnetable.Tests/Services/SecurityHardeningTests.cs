using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Security;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Dotnetable.Tests.Services;

/// <summary>
/// Covers the launch-blocking auth gaps: an unlimited OTP guessing budget, unlimited password
/// guessing, and passwords short enough to be worth guessing. Each test asserts the limit exists,
/// because each of these was previously absent rather than merely loose.
/// </summary>
public class SecurityHardeningTests : IDisposable
{
    private readonly RelationalTestDb _db;
    private readonly AppDbContext _context;
    private readonly WebsiteClientAuthService _auth;
    private readonly Mock<ISmsSender> _sms = new();

    public SecurityHardeningTests()
    {
        _db = new RelationalTestDb();
        _context = _db.NewContext();

        var email = new Mock<IEmailService>();
        email.Setup(e => e.IsConfiguredAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _sms.Setup(s => s.IsConfiguredAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _auth = new WebsiteClientAuthService(
            _context, email.Object, _sms.Object,
            new PlainTextHasher(), new Mock<IAdminNotificationService>().Object);
    }

    // ── OTP brute force ─────────────────────────────────────────────

    [Fact]
    public async Task VerifyOtp_DiesAfterTooManyWrongCodes()
    {
        var client = await SeedInactiveClientAsync();
        var code = await IssueCodeAsync(client.WebsiteClientID);

        // Five misses is the budget: the first four are ordinary failures and the fifth spends it.
        // A six-digit code has a million values, so without a budget the 30-minute window is enough
        // to walk the whole space.
        for (var attempt = 0; attempt < 4; attempt++)
        {
            var (result, _) = await _auth.VerifyOtpAsync(client.WebsiteID, "buyer@test.local", "000000");
            result.Should().Be(ClientVerifyResult.InvalidCode);
        }

        var (afterBudget, _) = await _auth.VerifyOtpAsync(client.WebsiteID, "buyer@test.local", "000000");
        afterBudget.Should().Be(ClientVerifyResult.TooManyAttempts);

        // The correct code is dead too — the row was burnt, not merely locked, so an attacker who
        // exhausts the budget and then guesses right still gets nothing.
        var (withRealCode, _) = await _auth.VerifyOtpAsync(client.WebsiteID, "buyer@test.local", code);
        withRealCode.Should().Be(ClientVerifyResult.TooManyAttempts);
    }

    [Fact]
    public async Task VerifyOtp_AcceptsTheCorrectCodeWithinBudget()
    {
        var client = await SeedInactiveClientAsync();
        var code = await IssueCodeAsync(client.WebsiteClientID);

        await _auth.VerifyOtpAsync(client.WebsiteID, "buyer@test.local", "000000");

        var (result, verified) = await _auth.VerifyOtpAsync(client.WebsiteID, "buyer@test.local", code);

        result.Should().Be(ClientVerifyResult.Success);
        verified!.Active.Should().BeTrue();
    }

    [Fact]
    public async Task ResendOtp_IsRefusedInsideTheCooldown()
    {
        var client = await SeedInactiveClientAsync();
        await IssueCodeAsync(client.WebsiteClientID);

        // Each resend costs the site an email or an SMS, so the endpoint cannot be a free megaphone.
        var result = await _auth.ResendOtpAsync(client.WebsiteID, "buyer@test.local");

        result.Should().Be(ClientResendResult.TooSoon);
    }

    // ── Sign-in lockout ─────────────────────────────────────────────

    [Fact]
    public async Task Login_LocksTheAccountAfterRepeatedFailures()
    {
        var client = await SeedActiveClientAsync();

        for (var attempt = 0; attempt < 7; attempt++)
        {
            var (status, _) = await _auth.ValidateCredentialsAsync(client.WebsiteID, "buyer@test.local", "wrong");
            status.Should().Be(ClientLoginStatus.InvalidCredentials);
        }

        var (locked, _) = await _auth.ValidateCredentialsAsync(client.WebsiteID, "buyer@test.local", "wrong");
        locked.Should().Be(ClientLoginStatus.LockedOut);

        // The correct password is refused too while the lockout stands — otherwise the limit only
        // slows an attacker who never gets it right.
        var (evenWhenCorrect, _) = await _auth.ValidateCredentialsAsync(
            client.WebsiteID, "buyer@test.local", "CorrectHorse-9!");
        evenWhenCorrect.Should().Be(ClientLoginStatus.LockedOut);
    }

    [Fact]
    public async Task Login_ResetsTheFailureCountOnSuccess()
    {
        var client = await SeedActiveClientAsync();

        await _auth.ValidateCredentialsAsync(client.WebsiteID, "buyer@test.local", "wrong");
        await _auth.ValidateCredentialsAsync(client.WebsiteID, "buyer@test.local", "wrong");

        var (status, _) = await _auth.ValidateCredentialsAsync(
            client.WebsiteID, "buyer@test.local", "CorrectHorse-9!");

        status.Should().Be(ClientLoginStatus.Success);
        (await _context.WebsiteClients.FindAsync(client.WebsiteClientID))!.FailedLoginCount.Should().Be(0);
    }

    // ── Registration delivery + password strength ───────────────────

    [Fact]
    public async Task Register_RefusesMobileOnlyWhenNoSmsGatewayIsConfigured()
    {
        await SeedWebsiteAsync();

        // Previously this reported success while the "sent" code only ever reached the log file, so
        // the customer had an account they could never activate.
        var response = await _auth.RegisterAsync(new ClientRegistration(
            1, "Test", "Buyer", Email: null, CountryCode: "98", Cellphone: "9121234567",
            Password: "CorrectHorse-9!"));

        response.Result.Should().Be(ClientRegisterResult.DeliveryNotConfigured);
        response.Channel.Should().Be(OtpChannel.Sms);
        (await _context.WebsiteClients.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Register_RefusesAWeakPassword()
    {
        await SeedWebsiteAsync();

        var response = await _auth.RegisterAsync(new ClientRegistration(
            1, "Test", "Buyer", "buyer@test.local", null, null, Password: "password123"));

        response.Result.Should().Be(ClientRegisterResult.WeakPassword);
        response.Error.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData("short1!", false)]                  // below the minimum length
    [InlineData("alllowercaseletters", false)]      // one character class only
    [InlineData("password123", false)]              // on the forbidden list
    [InlineData("aaaaaaaaaaA1", false)]             // too few distinct characters
    [InlineData("buyer@test.localX9!", false)]      // contains the account's own identifier
    [InlineData("CorrectHorse-9!", true)]
    public void PasswordPolicy_AppliesTheDocumentedRules(string password, bool expected) =>
        PasswordPolicy.Validate(password, "buyer@test.local").Ok.Should().Be(expected);

    [Fact]
    public void PasswordPolicy_HoldsAdminsToALongerMinimum() =>
        // Ten characters passes for a customer and fails for an admin, who can read every order.
        PasswordPolicy.ValidateAdmin("Nx7!qWer-T", "admin").Ok.Should().BeFalse();

    // ── Helpers ─────────────────────────────────────────────────────

    private async Task SeedWebsiteAsync()
    {
        if (await _context.Websites.AnyAsync()) return;

        _context.Websites.Add(new Website
        {
            WebsiteID = 1,
            TradeName = "Test",
            BrandName = "Test",
            WebsiteAddress = "test.local",
            AuthCode = Guid.NewGuid(),
            Active = true,
            Manager = "Manager",
            Mobile = "9120000000",
            Email = "shop@test.local",
            DefaultCurrencyCode = "USD",
            DefaultLanguageCode = "en",
        });
        await _context.SaveChangesAsync();
    }

    private async Task<WebsiteClient> SeedInactiveClientAsync() => await SeedClientAsync(active: false);

    private async Task<WebsiteClient> SeedActiveClientAsync() => await SeedClientAsync(active: true);

    private async Task<WebsiteClient> SeedClientAsync(bool active)
    {
        await SeedWebsiteAsync();

        var client = new WebsiteClient
        {
            WebsiteID = 1,
            Email = "buyer@test.local",
            Password = "HASHED:CorrectHorse-9!",
            Active = active,
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            HashKey = Guid.NewGuid(),
            ClientLevel = 0,
        };
        _context.WebsiteClients.Add(client);
        await _context.SaveChangesAsync();
        return client;
    }

    /// <summary>Writes a known live code straight to the table, standing in for a delivered one.</summary>
    private async Task<string> IssueCodeAsync(int clientId)
    {
        const string code = "424242";
        _context.WebsiteClientForgetPasswords.Add(new WebsiteClientForgetPassword
        {
            WebsiteClientID = clientId,
            ForgetKey = code,
            LogTime = DateTime.UtcNow,
            FailedAttempts = 0,
        });
        await _context.SaveChangesAsync();
        return code;
    }

    public void Dispose()
    {
        _context.Dispose();
        _db.Dispose();
    }

    /// <summary>
    /// Deterministic stand-in for the Identity hasher, so tests assert on lockout and policy rather
    /// than on PBKDF2 output.
    /// </summary>
    private sealed class PlainTextHasher : IPasswordHasher<WebsiteClient>
    {
        public string HashPassword(WebsiteClient user, string password) => $"HASHED:{password}";

        public PasswordVerificationResult VerifyHashedPassword(
            WebsiteClient user, string hashedPassword, string providedPassword) =>
            hashedPassword == $"HASHED:{providedPassword}"
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
    }
}
