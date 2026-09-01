using Dotnetable.Application.Authorization;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Dotnetable.Tests.Services;

public class SetupServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly SetupService _service;

    public SetupServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        // Services open a context per call now, so they get a factory over the same options;
        // the fixture keeps its own _context for seeding and asserting.
        var factory = new TestDbContextFactory(opts);

        var config = new Mock<IDatabaseConfigStore>();
        config.SetupGet(c => c.IsConfigured).Returns(true);

        _service = new SetupService(
            new TestDbContextFactory(opts),
            config.Object,
            Mock.Of<IAppSettingsStore>(),
            Mock.Of<IDatabaseProvisionerRegistry>(),
            Mock.Of<IInitialDataSeeder>());
    }

    [Fact]
    public async Task SyncRoleCatalogAsync_InsertsMissingRoles()
    {
        await SeedAdminPolicyWithRoles(RoleCatalog.All.Take(3).ToList());

        await _service.SyncRoleCatalogAsync();

        var keys = await _context.Roles.Select(r => r.RoleKey).ToListAsync();
        keys.Should().BeEquivalentTo(RoleCatalog.All.Select(r => r.Key));
    }

    [Fact]
    public async Task SyncRoleCatalogAsync_GrantsMissingRolesToAdministrators()
    {
        var subset = RoleCatalog.All.Take(5).ToList();
        await SeedAdminPolicyWithRoles(subset);

        await _service.SyncRoleCatalogAsync();

        var admin = await _context.Policies.SingleAsync(p => p.Title == DefaultPolicies.Administrators);
        var granted = await _context.PolicyRoles
            .Where(pr => pr.PolicyID == admin.PolicyID && pr.Active)
            .Join(_context.Roles, pr => pr.RoleID, r => r.RoleID, (_, r) => r.RoleKey)
            .ToListAsync();

        granted.Should().BeEquivalentTo(RoleCatalog.All.Select(r => r.Key));
    }

    [Fact]
    public async Task SyncRoleCatalogAsync_DoesNotDuplicateExistingAdminGrants()
    {
        await SeedAdminPolicyWithRoles(RoleCatalog.All.ToList());

        await _service.SyncRoleCatalogAsync();
        await _service.SyncRoleCatalogAsync();

        var admin = await _context.Policies.SingleAsync(p => p.Title == DefaultPolicies.Administrators);
        var count = await _context.PolicyRoles.CountAsync(pr => pr.PolicyID == admin.PolicyID);
        count.Should().Be(RoleCatalog.All.Count);
    }

    [Fact]
    public async Task SyncRoleCatalogAsync_GrantsMissingStaffTemplateRoles()
    {
        await SeedAdminPolicyWithRoles(RoleCatalog.All.ToList());
        var view = await _context.Roles.SingleAsync(r => r.RoleKey == RoleKeys.TasksView);
        var warehouse = new Policy { Title = DefaultPolicies.WarehouseStaff, Active = true, WebsiteID = 1 };
        _context.Policies.Add(warehouse);
        await _context.SaveChangesAsync();

        await _service.SyncRoleCatalogAsync();

        var granted = await _context.PolicyRoles
            .AnyAsync(pr => pr.PolicyID == warehouse.PolicyID && pr.RoleID == view.RoleID && pr.Active);
        granted.Should().BeTrue();
    }

    private async Task SeedAdminPolicyWithRoles(IReadOnlyList<RoleDefinition> defs)
    {
        _context.Roles.AddRange(defs.Select(def => new Role
        {
            RoleKey = def.Key,
            Description = def.Description,
            Category = (byte)def.Category,
            Active = true,
        }));
        await _context.SaveChangesAsync();

        var policy = new Policy { Title = DefaultPolicies.Administrators, Active = true, WebsiteID = 1 };
        _context.Policies.Add(policy);
        await _context.SaveChangesAsync();

        _context.PolicyRoles.AddRange(_context.Roles.Select(r => new PolicyRole
        {
            PolicyID = policy.PolicyID,
            RoleID = r.RoleID,
            Active = true,
        }));
        await _context.SaveChangesAsync();
    }

    public void Dispose() => _context.Dispose();
}
