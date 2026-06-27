using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Dotnetable.Tests.Services;

public class MemberServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IPasswordHasher<Member>> _hasher;
    private readonly MemberService _service;
    private readonly Website _website;
    private readonly Policy _policy;

    public MemberServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);

        _hasher = new Mock<IPasswordHasher<Member>>();
        _hasher.Setup(h => h.HashPassword(It.IsAny<Member>(), It.IsAny<string>()))
            .Returns<Member, string>((_, pw) => $"HASHED:{pw}");
        _hasher.Setup(h => h.VerifyHashedPassword(It.IsAny<Member>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<Member, string, string>((_, hash, pw) =>
                hash == $"HASHED:{pw}" ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed);

        _service = new MemberService(_context, _hasher.Object);

        _website = new Website
        {
            TradeName = "Test", WebsiteAddress = "test.com", AuthCode = Guid.NewGuid(),
            Active = true, Manager = "Mgr", Mobile = "123", Email = "admin@test.com",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today), DefaultLanguageCode = "en", BrandName = "Test"
        };
        _context.Websites.Add(_website);
        _context.SaveChanges();

        _policy = new Policy { Title = "Policy", Active = true, WebsiteID = _website.WebsiteID };
        _context.Policies.Add(_policy);
        _context.SaveChanges();
    }

    private Member NewMember(string username = "user", bool active = true, string pw = "HASHED:secret") => new()
    {
        Username = username, Active = active, Password = pw,
        Email = $"{username}@test.com", CellphoneNumber = "123", CountryCode = "US",
        RegisterDate = DateOnly.FromDateTime(DateTime.Today),
        Givenname = "A", Surname = "B", HashKey = Guid.NewGuid(),
        PolicyID = _policy.PolicyID, WebsiteID = _website.WebsiteID
    };

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsMember()
    {
        var m = NewMember(); _context.Members.Add(m); await _context.SaveChangesAsync();

        (await _service.GetByIdAsync(m.MemberID)).Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_Missing_ReturnsNull()
    {
        (await _service.GetByIdAsync(999)).Should().BeNull();
    }

    [Fact]
    public async Task ValidateCredentialsAsync_CorrectPassword_ReturnsMember()
    {
        _context.Members.Add(NewMember("alice", pw: "HASHED:pw123"));
        await _context.SaveChangesAsync();

        var result = await _service.ValidateCredentialsAsync("alice", "pw123");

        result.Should().NotBeNull();
        result!.Username.Should().Be("alice");
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WrongPassword_ReturnsNull()
    {
        _context.Members.Add(NewMember("alice", pw: "HASHED:pw123"));
        await _context.SaveChangesAsync();

        (await _service.ValidateCredentialsAsync("alice", "wrong")).Should().BeNull();
    }

    [Fact]
    public async Task ValidateCredentialsAsync_InactiveMember_ReturnsNull()
    {
        _context.Members.Add(NewMember("alice", active: false, pw: "HASHED:pw123"));
        await _context.SaveChangesAsync();

        (await _service.ValidateCredentialsAsync("alice", "pw123")).Should().BeNull();
    }

    [Fact]
    public async Task ValidateCredentialsAsync_UnknownUsername_ReturnsNull()
    {
        (await _service.ValidateCredentialsAsync("nobody", "pw123")).Should().BeNull();
    }

    [Fact]
    public async Task GetWebsiteIdByUsernameAsync_KnownUser_ReturnsWebsiteId()
    {
        var m = NewMember(); _context.Members.Add(m); await _context.SaveChangesAsync();

        (await _service.GetWebsiteIdByUsernameAsync(m.Username)).Should().Be(_website.WebsiteID);
    }

    [Fact]
    public async Task GetWebsiteIdByUsernameAsync_UnknownUser_ReturnsNull()
    {
        (await _service.GetWebsiteIdByUsernameAsync("nobody")).Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_MatchingUsername_ReturnsTrue()
    {
        var m = NewMember("taken"); _context.Members.Add(m); await _context.SaveChangesAsync();

        (await _service.ExistsAsync("taken", "fresh@test.com")).Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_MatchingEmail_ReturnsTrue()
    {
        var m = NewMember("someone"); _context.Members.Add(m); await _context.SaveChangesAsync();

        (await _service.ExistsAsync("freshname", "someone@test.com")).Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NoMatch_ReturnsFalse()
    {
        var m = NewMember("someone"); _context.Members.Add(m); await _context.SaveChangesAsync();

        (await _service.ExistsAsync("nobody", "nobody@test.com")).Should().BeFalse();
    }

    [Fact]
    public async Task GetByWebsiteAsync_ReturnsOnlyMembersForWebsite()
    {
        var other = new Website
        {
            TradeName = "Other", WebsiteAddress = "other.com", AuthCode = Guid.NewGuid(),
            Active = true, Manager = "Mgr", Mobile = "123", Email = "x@x.com",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today), DefaultLanguageCode = "en", BrandName = "Other"
        };
        _context.Websites.Add(other);
        await _context.SaveChangesAsync();

        var m1 = NewMember("m1");
        var m2 = NewMember("m2"); m2.WebsiteID = other.WebsiteID;
        _context.Members.AddRange(m1, m2);
        await _context.SaveChangesAsync();

        var result = await _service.GetByWebsiteAsync(_website.WebsiteID);

        result.Should().ContainSingle(m => m.Username == "m1");
    }

    [Fact]
    public async Task GetPagedAsync_UsernameFilter_FiltersCorrectly()
    {
        _context.Members.AddRange(NewMember("alice"), NewMember("bob"));
        await _context.SaveChangesAsync();

        var q = new GridQuery(); q.Search["Username"] = "ali";
        var result = await _service.GetPagedAsync(null, q);

        result.Items.Should().ContainSingle(m => m.Username == "alice");
    }

    [Fact]
    public async Task GetPagedAsync_EmailFilter_FiltersCorrectly()
    {
        _context.Members.AddRange(NewMember("alice"), NewMember("bob"));
        await _context.SaveChangesAsync();

        var q = new GridQuery(); q.Search["Email"] = "bob@";
        var result = await _service.GetPagedAsync(null, q);

        result.Items.Should().ContainSingle(m => m.Username == "bob");
    }

    [Fact]
    public async Task GetPagedAsync_ActiveFilter_FiltersCorrectly()
    {
        _context.Members.AddRange(
            NewMember("active1", active: true),
            NewMember("inactive1", active: false));
        await _context.SaveChangesAsync();

        var q = new GridQuery(); q.Search["Active"] = "false";
        var result = await _service.GetPagedAsync(null, q);

        result.Items.Should().ContainSingle(m => m.Username == "inactive1");
    }

    [Fact]
    public async Task GetPagedAsync_WebsiteFilter_ReturnsOnlyMatchingSite()
    {
        var other = new Website
        {
            TradeName = "Other", WebsiteAddress = "other.com", AuthCode = Guid.NewGuid(),
            Active = true, Manager = "Mgr", Mobile = "123", Email = "x@x.com",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today), DefaultLanguageCode = "en", BrandName = "Other"
        };
        _context.Websites.Add(other);
        await _context.SaveChangesAsync();

        var m1 = NewMember("site1");
        var m2 = NewMember("other1"); m2.WebsiteID = other.WebsiteID;
        _context.Members.AddRange(m1, m2);
        await _context.SaveChangesAsync();

        var result = await _service.GetPagedAsync(_website.WebsiteID, new GridQuery());

        result.Items.Should().OnlyContain(m => m.WebsiteID == _website.WebsiteID);
    }

    [Fact]
    public async Task GetPagedAsync_PaginatesAndReturnsCorrectTotalCount()
    {
        for (int i = 0; i < 15; i++)
            _context.Members.Add(NewMember($"user{i}"));
        await _context.SaveChangesAsync();

        var q = new GridQuery { PageSize = 5 };
        var result = await _service.GetPagedAsync(null, q);

        result.TotalCount.Should().Be(15);
        result.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task CreateAsync_HashesPasswordAndAssignsId()
    {
        var m = NewMember(); m.Password = string.Empty;

        var created = await _service.CreateAsync(m, "secret");

        created.MemberID.Should().BeGreaterThan(0);
        created.Password.Should().Be("HASHED:secret");
    }

    [Fact]
    public async Task CreateAsync_GeneratesHashKeyWhenEmpty()
    {
        var m = NewMember(); m.HashKey = Guid.Empty;

        await _service.CreateAsync(m, "secret");

        m.HashKey.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateAsync_PreservesExistingHashKey()
    {
        var m = NewMember(); var existingKey = m.HashKey;

        await _service.CreateAsync(m, "secret");

        m.HashKey.Should().Be(existingKey);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var m = NewMember(); _context.Members.Add(m); await _context.SaveChangesAsync();
        m.Givenname = "Updated";

        await _service.UpdateAsync(m);

        (await _context.Members.FindAsync(m.MemberID))!.Givenname.Should().Be("Updated");
    }

    [Fact]
    public async Task ChangePasswordAsync_UpdatesPasswordAndRotatesHashKey()
    {
        var m = NewMember(pw: "HASHED:old"); _context.Members.Add(m); await _context.SaveChangesAsync();
        var oldKey = m.HashKey;

        await _service.ChangePasswordAsync(m.MemberID, "new");

        var updated = await _context.Members.FindAsync(m.MemberID);
        updated!.Password.Should().Be("HASHED:new");
        updated.HashKey.Should().NotBe(oldKey);
    }

    [Fact]
    public async Task ChangePasswordAsync_MissingMember_ThrowsKeyNotFoundException()
    {
        await _service.Invoking(s => s.ChangePasswordAsync(999, "new"))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_RemovesMember()
    {
        var m = NewMember(); _context.Members.Add(m); await _context.SaveChangesAsync();

        await _service.DeleteAsync(m.MemberID);

        (await _context.Members.FindAsync(m.MemberID)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_MissingId_DoesNotThrow()
    {
        await _service.Invoking(s => s.DeleteAsync(999)).Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetRoleKeysAsync_ReturnsDistinctActiveRoleKeys()
    {
        var r1 = new Role { RoleKey = "admin.read", Description = "Read", Active = true };
        var r2 = new Role { RoleKey = "admin.write", Description = "Write", Active = true };
        var r3 = new Role { RoleKey = "hidden", Description = "Hidden", Active = false };
        _context.Roles.AddRange(r1, r2, r3);
        await _context.SaveChangesAsync();

        _context.PolicyRoles.AddRange(
            new PolicyRole { PolicyID = _policy.PolicyID, RoleID = r1.RoleID, Active = true },
            new PolicyRole { PolicyID = _policy.PolicyID, RoleID = r2.RoleID, Active = true },
            new PolicyRole { PolicyID = _policy.PolicyID, RoleID = r3.RoleID, Active = false });
        await _context.SaveChangesAsync();

        var member = NewMember(); _context.Members.Add(member); await _context.SaveChangesAsync();

        var keys = await _service.GetRoleKeysAsync(member.MemberID);

        keys.Should().BeEquivalentTo(new[] { "admin.read", "admin.write" });
    }

    [Fact]
    public async Task GetRoleKeysAsync_InactivePolicyRole_ExcludedFromResult()
    {
        var role = new Role { RoleKey = "admin.delete", Description = "Del", Active = true };
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        _context.PolicyRoles.Add(new PolicyRole
            { PolicyID = _policy.PolicyID, RoleID = role.RoleID, Active = false });
        await _context.SaveChangesAsync();

        var member = NewMember(); _context.Members.Add(member); await _context.SaveChangesAsync();

        var keys = await _service.GetRoleKeysAsync(member.MemberID);

        keys.Should().BeEmpty();
    }

    public void Dispose() => _context.Dispose();
}
