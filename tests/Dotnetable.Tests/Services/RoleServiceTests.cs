using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class RoleServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly RoleService _service;

    public RoleServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        _service = new RoleService(_context);
    }

    private static Role NewRole(string key = "test.role", string desc = "Test Role", bool active = true) => new()
    {
        RoleKey = key,
        Description = desc,
        Active = active
    };

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsRole()
    {
        var role = NewRole(); _context.Roles.Add(role); await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(role.RoleID);

        result.Should().NotBeNull();
        result!.RoleKey.Should().Be("test.role");
    }

    [Fact]
    public async Task GetByIdAsync_Missing_ReturnsNull()
    {
        (await _service.GetByIdAsync(999)).Should().BeNull();
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsOnlyActiveRoles()
    {
        _context.Roles.AddRange(
            NewRole("active.one", active: true),
            NewRole("active.two", active: true),
            NewRole("inactive.one", active: false));
        await _context.SaveChangesAsync();

        var result = await _service.GetAllActiveAsync();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.Active);
    }

    [Fact]
    public async Task GetAllActiveAsync_SortedByRoleKey()
    {
        _context.Roles.AddRange(
            NewRole("beta.role"),
            NewRole("alpha.role"),
            NewRole("gamma.role"));
        await _context.SaveChangesAsync();

        var result = await _service.GetAllActiveAsync();

        result.Select(r => r.RoleKey).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetPagedAsync_RoleKeyFilter_FiltersCorrectly()
    {
        _context.Roles.AddRange(NewRole("admin.read"), NewRole("admin.write"), NewRole("user.read"));
        await _context.SaveChangesAsync();

        var q = new GridQuery(); q.Search["RoleKey"] = "admin";
        var result = await _service.GetPagedAsync(q);

        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(r => r.RoleKey.Contains("admin"));
    }

    [Fact]
    public async Task GetPagedAsync_DescriptionFilter_FiltersCorrectly()
    {
        _context.Roles.AddRange(
            NewRole("r1", "Can read files"),
            NewRole("r2", "Can write files"),
            NewRole("r3", "Can delete rows"));
        await _context.SaveChangesAsync();

        var q = new GridQuery(); q.Search["Description"] = "files";
        var result = await _service.GetPagedAsync(q);

        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPagedAsync_ActiveFilter_FiltersCorrectly()
    {
        _context.Roles.AddRange(
            NewRole("active.one", active: true),
            NewRole("inactive.one", active: false));
        await _context.SaveChangesAsync();

        var q = new GridQuery(); q.Search["Active"] = "false";
        var result = await _service.GetPagedAsync(q);

        result.Items.Should().ContainSingle(r => r.RoleKey == "inactive.one");
    }

    [Fact]
    public async Task GetPagedAsync_PaginatesAndReturnsCorrectTotalCount()
    {
        for (int i = 0; i < 12; i++)
            _context.Roles.Add(NewRole($"role.{i:D2}"));
        await _context.SaveChangesAsync();

        var q = new GridQuery { PageSize = 5 };
        var result = await _service.GetPagedAsync(q);

        result.TotalCount.Should().Be(12);
        result.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task CreateAsync_PersistsRoleAndAssignsId()
    {
        var role = NewRole("new.role");

        var created = await _service.CreateAsync(role);

        created.RoleID.Should().BeGreaterThan(0);
        (await _context.Roles.FindAsync(created.RoleID))!.RoleKey.Should().Be("new.role");
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var role = NewRole(); _context.Roles.Add(role); await _context.SaveChangesAsync();
        role.Description = "Updated Description";

        await _service.UpdateAsync(role);

        (await _context.Roles.FindAsync(role.RoleID))!.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task DeleteAsync_RemovesRole()
    {
        var role = NewRole(); _context.Roles.Add(role); await _context.SaveChangesAsync();

        await _service.DeleteAsync(role.RoleID);

        (await _context.Roles.FindAsync(role.RoleID)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_MissingId_DoesNotThrow()
    {
        await _service.Invoking(s => s.DeleteAsync(999)).Should().NotThrowAsync();
    }

    public void Dispose() => _context.Dispose();
}
