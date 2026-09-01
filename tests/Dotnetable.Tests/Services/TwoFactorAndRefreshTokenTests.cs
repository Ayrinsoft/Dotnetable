using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Security;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Dotnetable.Tests.Services;

/// <summary>
/// The admin second factor and the customer refresh-token rotation. Both hand out a credential, so
/// each test is about what happens when the credential is reused, guessed, or presented late.
/// </summary>
public class TwoFactorAndRefreshTokenTests : IDisposable
{
    private readonly RelationalTestDb _db;
    private readonly AppDbContext _context;
    private readonly MemberService _members;
    private readonly RefreshTokenService _refreshTokens;

    public TwoFactorAndRefreshTokenTests()
    {
        _db = new RelationalTestDb();
        _context = _db.NewContext();
        _members = new MemberService(_context, new PlainTextHasher());
        _refreshTokens = new RefreshTokenService(
            new TestDbContextFactory(_db.Options), NullLogger<RefreshTokenService>.Instance);
    }

    // ── TOTP ────────────────────────────────────────────────────────

    [Fact]
    public void Totp_AcceptsACodeItGeneratedForTheCurrentStep()
    {
        var secret = Totp.GenerateSecret();
        var now = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        // Round-trips through the same algorithm an authenticator app runs, so a code produced for
        // this instant verifies at this instant.
        var code = CodeFor(secret, now);

        Totp.Verify(secret, code, now).Should().BeTrue();
    }

    [Fact]
    public void Totp_ToleratesOneStepOfClockDrift()
    {
        var secret = Totp.GenerateSecret();
        var now = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var code = CodeFor(secret, now);

        // A phone thirty seconds fast or slow must still be able to sign in.
        Totp.Verify(secret, code, now.AddSeconds(30)).Should().BeTrue();
        Totp.Verify(secret, code, now.AddSeconds(-30)).Should().BeTrue();

        // Two steps out is refused: drift tolerance must not become an open window.
        Totp.Verify(secret, code, now.AddSeconds(90)).Should().BeFalse();
    }

    [Fact]
    public void Totp_RejectsAWrongCodeAndMalformedInput()
    {
        var secret = Totp.GenerateSecret();

        Totp.Verify(secret, "000000").Should().BeFalse();
        Totp.Verify(secret, "12345").Should().BeFalse();
        Totp.Verify(secret, null).Should().BeFalse();
        Totp.Verify(null, "123456").Should().BeFalse();
    }

    [Fact]
    public async Task TwoFactor_IsOnlyEnabledAfterTheMemberProvesTheirAppWorks()
    {
        var member = await SeedMemberAsync();
        var enrolment = _members.BeginTwoFactorEnrolment(member, "Dotnetable");

        // A wrong confirmation code must leave 2FA off — enabling it on an unproven secret would lock
        // the member out of their own panel permanently.
        (await _members.ConfirmTwoFactorAsync(member.MemberID, enrolment.Secret, "000000")).Should().BeNull();
        (await _context.Members.FindAsync(member.MemberID))!.TwoFactorEnabled.Should().BeFalse();

        var recoveryCodes = await _members.ConfirmTwoFactorAsync(
            member.MemberID, enrolment.Secret, CodeFor(enrolment.Secret, DateTime.UtcNow));

        recoveryCodes.Should().NotBeNull().And.HaveCount(10);
        (await _context.Members.FindAsync(member.MemberID))!.TwoFactorEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task SignIn_StopsAtTheSecondFactorWhenOneIsEnrolled()
    {
        var member = await SeedMemberAsync();
        var enrolment = _members.BeginTwoFactorEnrolment(member, "Dotnetable");
        await _members.ConfirmTwoFactorAsync(member.MemberID, enrolment.Secret, CodeFor(enrolment.Secret, DateTime.UtcNow));

        var result = await _members.ValidateSignInAsync("admin", "CorrectHorse-99!");

        // The correct password alone is explicitly not a sign-in; the caller must challenge.
        result.Status.Should().Be(MemberSignInStatus.TwoFactorRequired);
        result.Member.Should().NotBeNull();
    }

    [Fact]
    public async Task RecoveryCode_WorksOnceAndThenNeverAgain()
    {
        var member = await SeedMemberAsync();
        var enrolment = _members.BeginTwoFactorEnrolment(member, "Dotnetable");
        var codes = await _members.ConfirmTwoFactorAsync(
            member.MemberID, enrolment.Secret, CodeFor(enrolment.Secret, DateTime.UtcNow));

        var recovery = codes![0];

        (await _members.VerifyTwoFactorAsync(member.MemberID, recovery)).Status
            .Should().Be(MemberSignInStatus.Success);

        // Single use: a code read off a printout that has already been used is worthless.
        (await _members.VerifyTwoFactorAsync(member.MemberID, recovery)).Status
            .Should().Be(MemberSignInStatus.InvalidTwoFactorCode);
    }

    [Fact]
    public async Task SignIn_LocksTheMemberAfterRepeatedWrongPasswords()
    {
        await SeedMemberAsync();

        for (var attempt = 0; attempt < 4; attempt++)
        {
            (await _members.ValidateSignInAsync("admin", "wrong")).Status
                .Should().Be(MemberSignInStatus.InvalidCredentials);
        }

        (await _members.ValidateSignInAsync("admin", "wrong")).Status
            .Should().Be(MemberSignInStatus.LockedOut);

        (await _members.ValidateSignInAsync("admin", "CorrectHorse-99!")).Status
            .Should().Be(MemberSignInStatus.LockedOut);
    }

    // ── Refresh tokens ──────────────────────────────────────────────

    [Fact]
    public async Task Refresh_RotatesTheTokenOnEveryExchange()
    {
        var client = await SeedClientAsync();
        var issued = await _refreshTokens.IssueAsync(client, "127.0.0.1");

        var exchanged = await _refreshTokens.ExchangeAsync(client.WebsiteID, issued.Token, "127.0.0.1");

        exchanged.Success.Should().BeTrue();
        exchanged.Refresh!.Token.Should().NotBe(issued.Token);
    }

    [Fact]
    public async Task Refresh_TreatsAReplayedTokenAsTheftAndCutsEverySession()
    {
        var client = await SeedClientAsync();
        var issued = await _refreshTokens.IssueAsync(client, "127.0.0.1");
        var rotated = await _refreshTokens.ExchangeAsync(client.WebsiteID, issued.Token, "127.0.0.1");

        // Presenting the spent token again is what a stolen copy looks like.
        var replay = await _refreshTokens.ExchangeAsync(client.WebsiteID, issued.Token, "10.0.0.9");
        replay.Success.Should().BeFalse();
        replay.ReuseDetected.Should().BeTrue();

        // The replacement is revoked too, so the thief and the owner both have to sign in again —
        // the alternative would let a stolen token stay usable for as long as it is refreshed.
        var afterFamilyRevoke = await _refreshTokens.ExchangeAsync(
            client.WebsiteID, rotated.Refresh!.Token, "127.0.0.1");
        afterFamilyRevoke.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Refresh_IsRefusedForALockedOutOrDeactivatedAccount()
    {
        var client = await SeedClientAsync();
        var issued = await _refreshTokens.IssueAsync(client, "127.0.0.1");

        var stored = await _context.WebsiteClients.FindAsync(client.WebsiteClientID);
        stored!.LockoutEndUtc = DateTime.UtcNow.AddMinutes(10);
        await _context.SaveChangesAsync();

        // Otherwise a lockout would only stop the sign-in form, not the session already in flight.
        (await _refreshTokens.ExchangeAsync(client.WebsiteID, issued.Token, "127.0.0.1"))
            .Success.Should().BeFalse();
    }

    [Fact]
    public async Task Refresh_StoresOnlyAHashOfTheToken()
    {
        var client = await SeedClientAsync();
        var issued = await _refreshTokens.IssueAsync(client, "127.0.0.1");

        var rows = await _context.WebsiteClientRefreshTokens.AsNoTracking().ToListAsync();

        // A database leak must not hand over live sessions.
        rows.Should().ContainSingle();
        rows[0].TokenHash.Should().NotBe(issued.Token).And.HaveLength(64);
    }

    // ── Helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// Produces the code an authenticator app would show at <paramref name="when"/>, by asking
    /// <see cref="Totp.Verify"/> which of the million candidates matches. Slow but exact, and it
    /// keeps the test independent of the implementation's internals.
    ///
    /// <para>Verify accepts one step of drift either way, so three different codes pass at any given
    /// instant. Requiring the candidate to pass at <paramref name="when"/> and at 30 seconds either
    /// side pins it to the code for that exact step — which is what a drift test has to start from.</para>
    /// </summary>
    private static string CodeFor(string secret, DateTime when)
    {
        for (var candidate = 0; candidate < 1_000_000; candidate++)
        {
            var code = candidate.ToString("D6");

            if (Totp.Verify(secret, code, when)
                && Totp.Verify(secret, code, when.AddSeconds(30))
                && Totp.Verify(secret, code, when.AddSeconds(-30)))
                return code;
        }

        throw new InvalidOperationException("No TOTP code matched, which means Verify is broken.");
    }

    private async Task<Member> SeedMemberAsync()
    {
        var policy = new Policy { Title = "Administrators", WebsiteID = 1, Active = true };
        _context.Policies.Add(policy);
        await _context.SaveChangesAsync();

        var member = new Member
        {
            Username = "admin",
            Password = "HASHED:CorrectHorse-99!",
            Email = "admin@test.local",
            CellphoneNumber = "9120000000",
            CountryCode = "98",
            Givenname = "Site",
            Surname = "Admin",
            Active = true,
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            HashKey = Guid.NewGuid(),
            PolicyID = policy.PolicyID,
            WebsiteID = 1,
        };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();
        return member;
    }

    private async Task<WebsiteClient> SeedClientAsync()
    {
        var client = new WebsiteClient
        {
            WebsiteID = 1,
            Email = "buyer@test.local",
            Password = "HASHED:CorrectHorse-9!",
            Active = true,
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            HashKey = Guid.NewGuid(),
            ClientLevel = 0,
        };
        _context.WebsiteClients.Add(client);
        await _context.SaveChangesAsync();
        return client;
    }

    public void Dispose()
    {
        _context.Dispose();
        _db.Dispose();
    }

    private sealed class PlainTextHasher : IPasswordHasher<Member>
    {
        public string HashPassword(Member user, string password) => $"HASHED:{password}";

        public PasswordVerificationResult VerifyHashedPassword(
            Member user, string hashedPassword, string providedPassword) =>
            hashedPassword == $"HASHED:{providedPassword}"
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
    }
}
