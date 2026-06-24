using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Dotnetable.Tests.Services;

public class PasswordResetServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IEmailService> _emailMock;
    private readonly Mock<IPasswordHasher<Member>> _hasherMock;
    private readonly PasswordResetService _service;

    public PasswordResetServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);

        _emailMock = new Mock<IEmailService>();
        _emailMock.Setup(e => e.IsConfiguredAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _emailMock.Setup(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _hasherMock = new Mock<IPasswordHasher<Member>>();
        _hasherMock.Setup(h => h.HashPassword(It.IsAny<Member>(), It.IsAny<string>()))
            .Returns<Member, string>((_, pw) => $"HASHED:{pw}");

        _service = new PasswordResetService(_context, _emailMock.Object, _hasherMock.Object);
    }

    private async Task<Member> SeedMemberAsync(string username = "alice", string email = "alice@test.com")
    {
        // Website and Policy needed as FKs
        var website = new Website
        {
            TradeName = "T", WebsiteAddress = "t.com", AuthCode = Guid.NewGuid(), Active = true,
            Manager = "M", Mobile = "1", Email = "a@t.com",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today), DefaultLanguageCode = "en", BrandName = "T"
        };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync();

        var policy = new Policy { Title = "P", Active = true, WebsiteID = website.WebsiteID };
        _context.Policies.Add(policy);
        await _context.SaveChangesAsync();

        var member = new Member
        {
            Username = username, Active = true, Password = "HASHED:secret",
            Email = email, CellphoneNumber = "123", CountryCode = "US",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today),
            Givenname = "Alice", Surname = "Smith", HashKey = Guid.NewGuid(),
            PolicyID = policy.PolicyID, WebsiteID = website.WebsiteID
        };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();
        return member;
    }

    private async Task<string> SeedValidKeyAsync(int memberId)
    {
        const string key = "ABCD1234";
        _context.MemberForgetPasswords.Add(new MemberForgetPassword
        {
            MemberID = memberId,
            ForgetKey = key,
            LogTime = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        return key;
    }

    // ── IsKeyValidAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task IsKeyValidAsync_ValidRecentKey_ReturnsTrue()
    {
        var member = await SeedMemberAsync();
        var key = await SeedValidKeyAsync(member.MemberID);

        (await _service.IsKeyValidAsync(key)).Should().BeTrue();
    }

    [Fact]
    public async Task IsKeyValidAsync_ExpiredKey_ReturnsFalse()
    {
        var member = await SeedMemberAsync();
        _context.MemberForgetPasswords.Add(new MemberForgetPassword
        {
            MemberID = member.MemberID,
            ForgetKey = "OLDKEY12",
            LogTime = DateTime.UtcNow.AddMinutes(-31) // beyond the 30-min window
        });
        await _context.SaveChangesAsync();

        (await _service.IsKeyValidAsync("OLDKEY12")).Should().BeFalse();
    }

    [Fact]
    public async Task IsKeyValidAsync_UnknownKey_ReturnsFalse()
    {
        (await _service.IsKeyValidAsync("XXXXXXXX")).Should().BeFalse();
    }

    [Fact]
    public async Task IsKeyValidAsync_NullOrWhitespaceKey_ReturnsFalse()
    {
        (await _service.IsKeyValidAsync("")).Should().BeFalse();
        (await _service.IsKeyValidAsync("   ")).Should().BeFalse();
    }

    [Fact]
    public async Task IsKeyValidAsync_KeyIsCaseInsensitive()
    {
        var member = await SeedMemberAsync();
        // Key is stored uppercase by the service; test that lowercase lookup also works
        _context.MemberForgetPasswords.Add(new MemberForgetPassword
        {
            MemberID = member.MemberID,
            ForgetKey = "ABCDEFGH",
            LogTime = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        (await _service.IsKeyValidAsync("abcdefgh")).Should().BeTrue();
    }

    // ── ResetPasswordAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task ResetPasswordAsync_ValidKey_UpdatesPasswordAndHashKey()
    {
        var member = await SeedMemberAsync();
        var key = await SeedValidKeyAsync(member.MemberID);
        var oldHashKey = member.HashKey;

        var result = await _service.ResetPasswordAsync(key, "newpassword");

        result.Should().BeTrue();
        var updated = await _context.Members.FindAsync(member.MemberID);
        updated!.Password.Should().Be("HASHED:newpassword");
        updated.HashKey.Should().NotBe(oldHashKey);
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidKey_ClearsAllForgetPasswordEntries()
    {
        var member = await SeedMemberAsync();
        // Seed two keys for the same member
        _context.MemberForgetPasswords.AddRange(
            new MemberForgetPassword { MemberID = member.MemberID, ForgetKey = "FIRST111", LogTime = DateTime.UtcNow },
            new MemberForgetPassword { MemberID = member.MemberID, ForgetKey = "SECOND22", LogTime = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        await _service.ResetPasswordAsync("FIRST111", "newpw");

        _context.MemberForgetPasswords
            .Where(f => f.MemberID == member.MemberID)
            .Should().BeEmpty();
    }

    [Fact]
    public async Task ResetPasswordAsync_InvalidKey_ReturnsFalse()
    {
        (await _service.ResetPasswordAsync("BADKEY00", "newpw")).Should().BeFalse();
    }

    [Fact]
    public async Task ResetPasswordAsync_ExpiredKey_ReturnsFalse()
    {
        var member = await SeedMemberAsync();
        _context.MemberForgetPasswords.Add(new MemberForgetPassword
        {
            MemberID = member.MemberID,
            ForgetKey = "EXPIRED1",
            LogTime = DateTime.UtcNow.AddMinutes(-35)
        });
        await _context.SaveChangesAsync();

        (await _service.ResetPasswordAsync("EXPIRED1", "newpw")).Should().BeFalse();
    }

    // ── RequestResetAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task RequestResetAsync_MemberNotFound_ReturnsMemberNotFound()
    {
        var result = await _service.RequestResetAsync("nobody@test.com", k => $"https://app/reset?k={k}");

        result.Should().Be(PasswordResetRequestResult.MemberNotFound);
    }

    [Fact]
    public async Task RequestResetAsync_InactiveMember_ReturnsMemberNotFound()
    {
        var member = await SeedMemberAsync("alice", "alice@test.com");
        member.Active = false;
        await _context.SaveChangesAsync();

        var result = await _service.RequestResetAsync("alice@test.com", k => $"https://app/reset?k={k}");

        result.Should().Be(PasswordResetRequestResult.MemberNotFound);
    }

    [Fact]
    public async Task RequestResetAsync_EmailNotConfigured_ReturnsEmailNotConfigured()
    {
        _emailMock.Setup(e => e.IsConfiguredAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        await SeedMemberAsync("alice", "alice@test.com");

        var result = await _service.RequestResetAsync("alice", k => $"https://app/reset?k={k}");

        result.Should().Be(PasswordResetRequestResult.EmailNotConfigured);
    }

    [Fact]
    public async Task RequestResetAsync_ValidMember_ReturnsSentAndCreatesKey()
    {
        await SeedMemberAsync("alice", "alice@test.com");

        var result = await _service.RequestResetAsync("alice@test.com", k => $"https://app/reset?k={k}");

        result.Should().Be(PasswordResetRequestResult.Sent);
        _context.MemberForgetPasswords.Should().HaveCount(1);
        _emailMock.Verify(e => e.SendAsync(
            "alice@test.com",
            It.IsAny<string>(),
            It.Is<string>(b => b.Contains("https://app/reset?k=")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestResetAsync_MatchesByUsername()
    {
        await SeedMemberAsync("alice", "alice@test.com");

        var result = await _service.RequestResetAsync("alice", k => $"https://app/reset?k={k}");

        result.Should().Be(PasswordResetRequestResult.Sent);
    }

    public void Dispose() => _context.Dispose();
}
