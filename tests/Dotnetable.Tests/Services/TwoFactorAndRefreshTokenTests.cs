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
        // Services open a context per call now; the factory points at the same database so the
        // fixture can still seed and assert through its own _context.
        var factory = new TestDbContextFactory(_db.Options);

        _members = new MemberService(factory, new PlainTextHasher());
        _refreshTokens = new RefreshTokenService(
            factory, NullLogger<RefreshTokenService>.Instance);
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
        // The service wrote through its own short-lived context, so this fixture's context must
        // re-read rather than answer from entities it is still tracking.
        _context.ChangeTracker.Clear();

        (await _context.Members.FindAsync(member.MemberID))!.TwoFactorEnabled.Should().BeFalse();

        var recoveryCodes = await _members.ConfirmTwoFactorAsync(
            member.MemberID, enrolment.Secret, CodeFor(enrolment.Secret, DateTime.UtcNow));

        recoveryCodes.Should().NotBeNull().And.HaveCount(10);

        // The first assertion above loaded the member into this context; clear again so the second
        // one sees the enrolment the service just wrote rather than that cached copy.
        _context.ChangeTracker.Clear();
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
    /// The code an authenticator app would show at <paramref name="when"/>, computed straight from
    /// RFC 6238 here rather than by searching through <see cref="Totp.Verify"/>.
    ///
    /// <para>Two reasons. It is an independent implementation, so a test that passes is evidence the
    /// production code follows the spec rather than merely agreeing with itself. And it is instant:
    /// searching a million candidates took long enough that the 30-second step rolled over between
    /// generating a code and using it, which made the enrolment tests fail at random.</para>
    /// </summary>
    private static string CodeFor(string secret, DateTime when)
    {
        var key = FromBase32(secret);
        var step = (long)(when.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds / 30;

        var counter = BitConverter.GetBytes(step);
        if (BitConverter.IsLittleEndian) Array.Reverse(counter);

        var hash = System.Security.Cryptography.HMACSHA1.HashData(key, counter);
        var offset = hash[^1] & 0x0F;
        var binary = ((hash[offset] & 0x7F) << 24)
                     | ((hash[offset + 1] & 0xFF) << 16)
                     | ((hash[offset + 2] & 0xFF) << 8)
                     | (hash[offset + 3] & 0xFF);

        return (binary % 1_000_000).ToString("D6");
    }

    private static byte[] FromBase32(string value)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var normalized = value.Replace(" ", "").Replace("-", "").TrimEnd('=').ToUpperInvariant();

        var bytes = new List<byte>(normalized.Length * 5 / 8);
        int buffer = 0, bitsLeft = 0;

        foreach (var c in normalized)
        {
            buffer = (buffer << 5) | alphabet.IndexOf(c);
            bitsLeft += 5;
            if (bitsLeft < 8) continue;

            bytes.Add((byte)((buffer >> (bitsLeft - 8)) & 0xFF));
            bitsLeft -= 8;
        }

        return bytes.ToArray();
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
