using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class PolicyServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PolicyService _service;
    private readonly Website _website;

    public PolicyServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        _service = new PolicyService(_context);

        _website = new Website
        {
            TradeName = "Test", WebsiteAddress = "test.com", AuthCode = Guid.NewGuid(),
            Active = true, Manager = "Mgr", Mobile = "123", Email = "admin@test.com",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today), DefaultLanguageCode = "en", BrandName = "Test"
        };
        _context.Websites.Add(_website);
        _context.SaveChanges();
    }

    private Policy NewPolicy(string title = "Policy", bool active = true) => new()
    {
        Title = title,
        Active = active,
        WebsiteID = _website.WebsiteID
    };

    private Role NewRole(string key, bool active = true) => new()
    {
        RoleKey = key,
        Description = key,
        Active = active
    };

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsPolicyWithPolicyRoles()
    {
        var role = NewRole("admin.read");
        _context.Roles.Add(role);
        var policy = NewPolicy(); _context.Policies.Add(policy);
        await _context.SaveChangesAsync();

        _context.PolicyRoles.Add(new PolicyRole
            { PolicyID = policy.PolicyID, RoleID = role.RoleID, Active = true });
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(policy.PolicyID);

        result.Should().NotBeNull();
        result!.PolicyRoles.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_Missing_ReturnsNull()
    {
        (await _service.GetByIdAsync(999)).Should().BeNull();
    }

    [Fact]
    public async Task GetByWebsiteAsync_ReturnsOnlyActivePoliciesForWebsite()
    {
        var other = new Website
        {
            TradeName = "Other", WebsiteAddress = "other.com", AuthCode = Guid.NewGuid(),
            Active = true, Manager = "Mgr", Mobile = "123", Email = "x@x.com",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today), DefaultLanguageCode = "en", BrandName = "Other"
        };
        _context.Websites.Add(other);
        await _context.SaveChangesAsync();

        _context.Policies.AddRange(
            new Policy { Title = "Active", Active = true, WebsiteID = _website.WebsiteID },
            new Policy { Title = "Inactive", Active = false, WebsiteID = _website.WebsiteID },
            new Policy { Title = "OtherSite", Active = true, WebsiteID = other.WebsiteID });
        await _context.SaveChangesAsync();

        var result = await _service.GetByWebsiteAsync(_website.WebsiteID);

        result.Should().ContainSingle(p => p.Title == "Active");
    }

    [Fact]
    public async Task GetPagedAsync_TitleFilter_FiltersCorrectly()
    {
        _context.Policies.AddRange(
            NewPolicy("Admin Policy"),
            NewPolicy("User Policy"),
            NewPolicy("Guest Policy"));
        await _context.SaveChangesAsync();

        var q = new GridQuery(); q.Search["Title"] = "Admin";
        var result = await _service.GetPagedAsync(null, q);

        result.Items.Should().ContainSingle(p => p.Title == "Admin Policy");
    }

    [Fact]
    public async Task GetPagedAsync_ActiveFilter_FiltersCorrectly()
    {
        _context.Policies.AddRange(NewPolicy("Active", active: true), NewPolicy("Inactive", active: false));
        await _context.SaveChangesAsync();

        var q = new GridQuery(); q.Search["Active"] = "false";
        var result = await _service.GetPagedAsync(null, q);

        result.Items.Should().ContainSingle(p => p.Title == "Inactive");
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

        _context.Policies.AddRange(
            new Policy { Title = "SitePolicy", Active = true, WebsiteID = _website.WebsiteID },
            new Policy { Title = "OtherPolicy", Active = true, WebsiteID = other.WebsiteID });
        await _context.SaveChangesAsync();

        var result = await _service.GetPagedAsync(_website.WebsiteID, new GridQuery());

        result.Items.Should().ContainSingle(p => p.Title == "SitePolicy");
    }

    [Fact]
    public async Task CreateAsync_CreatesPolicyWithAssignedRoles()
    {
        var r1 = NewRole("r1"); var r2 = NewRole("r2");
        _context.Roles.AddRange(r1, r2);
        await _context.SaveChangesAsync();

        var policy = NewPolicy("New Policy");
        await _service.CreateAsync(policy, [r1.RoleID, r2.RoleID]);

        var saved = await _context.Policies.Include(p => p.PolicyRoles)
            .FirstAsync(p => p.PolicyID == policy.PolicyID);
        saved.PolicyRoles.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAsync_DeduplicatesRoleIds()
    {
        var role = NewRole("dup.role");
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        var policy = NewPolicy();
        await _service.CreateAsync(policy, [role.RoleID, role.RoleID, role.RoleID]);

        var policyRoles = _context.PolicyRoles.Where(pr => pr.PolicyID == policy.PolicyID).ToList();
        policyRoles.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAsync_AddsNewRolesAndRemovesOldRoles()
    {
        var r1 = NewRole("keep"); var r2 = NewRole("remove"); var r3 = NewRole("add");
        _context.Roles.AddRange(r1, r2, r3);
        var policy = NewPolicy(); _context.Policies.Add(policy);
        await _context.SaveChangesAsync();

        _context.PolicyRoles.AddRange(
            new PolicyRole { PolicyID = policy.PolicyID, RoleID = r1.RoleID, Active = true },
            new PolicyRole { PolicyID = policy.PolicyID, RoleID = r2.RoleID, Active = true });
        await _context.SaveChangesAsync();

        // Update: keep r1, remove r2, add r3
        await _service.UpdateAsync(policy, [r1.RoleID, r3.RoleID]);

        var remainingRoleIds = _context.PolicyRoles
            .Where(pr => pr.PolicyID == policy.PolicyID)
            .Select(pr => pr.RoleID)
            .ToList();

        remainingRoleIds.Should().BeEquivalentTo(new[] { r1.RoleID, r3.RoleID });
    }

    [Fact]
    public async Task DeleteAsync_RemovesPolicy()
    {
        var policy = NewPolicy(); _context.Policies.Add(policy); await _context.SaveChangesAsync();

        await _service.DeleteAsync(policy.PolicyID);

        (await _context.Policies.FindAsync(policy.PolicyID)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_MissingId_DoesNotThrow()
    {
        await _service.Invoking(s => s.DeleteAsync(999)).Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetAssignableRolesAsync_MasterMember_ReturnsAllActiveRoles()
    {
        _context.Roles.AddRange(
            NewRole("a.role", active: true),
            NewRole("b.role", active: true),
            NewRole("c.role", active: false));
        await _context.SaveChangesAsync();

        var result = await _service.GetAssignableRolesAsync(memberId: 1, isMaster: true);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.Active);
    }

    [Fact]
    public async Task GetAssignableRolesAsync_NonMasterMember_ReturnsOnlyHeldRoles()
    {
        var r1 = NewRole("held.role"); var r2 = NewRole("other.role");
        _context.Roles.AddRange(r1, r2);
        var policy = NewPolicy(); _context.Policies.Add(policy);
        var website = _website;
        await _context.SaveChangesAsync();

        _context.PolicyRoles.Add(new PolicyRole
            { PolicyID = policy.PolicyID, RoleID = r1.RoleID, Active = true });
        await _context.SaveChangesAsync();

        var member = new Member
        {
            Username = "m", Active = true, Password = "pw", Email = "m@x.com",
            CellphoneNumber = "123", CountryCode = "US",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today),
            Givenname = "M", Surname = "N", HashKey = Guid.NewGuid(),
            PolicyID = policy.PolicyID, WebsiteID = website.WebsiteID
        };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var result = await _service.GetAssignableRolesAsync(member.MemberID, isMaster: false);

        result.Should().ContainSingle(r => r.RoleKey == "held.role");
    }

    public void Dispose() => _context.Dispose();
}
