using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class WarehouseServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly WarehouseService _service;
    private readonly Website _website;

    public WarehouseServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        _service = new WarehouseService(_context);

        _website = NewWebsite("Test", "test.com");
        _context.Websites.Add(_website);
        _context.SaveChanges();
    }

    public void Dispose() => _context.Dispose();

    private static Website NewWebsite(string trade, string host) => new()
    {
        TradeName = trade,
        WebsiteAddress = host,
        AuthCode = Guid.NewGuid(),
        Active = true,
        Manager = "Mgr",
        Mobile = "123",
        Email = "admin@test.com",
        RegisterDate = DateOnly.FromDateTime(DateTime.Today),
        DefaultLanguageCode = "en",
        DefaultCurrencyCode = "USD",
        BrandName = trade
    };

    private Warehouse NewWarehouse(string code, string name, bool isDefault = false, bool isActive = true, int? websiteId = null) => new()
    {
        WebsiteID = websiteId ?? _website.WebsiteID,
        Code = code,
        Name = name,
        IsDefault = isDefault,
        IsActive = isActive,
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task GetPagedAsync_CodeFilter_FiltersCorrectly()
    {
        _context.Warehouses.AddRange(
            NewWarehouse("MAIN", "Main warehouse", isDefault: true),
            NewWarehouse("EAST", "East depot"),
            NewWarehouse("WEST", "West depot"));
        await _context.SaveChangesAsync();

        var q = new GridQuery();
        q.Search[nameof(Warehouse.Code)] = "EAS";
        var result = await _service.GetPagedAsync(_website.WebsiteID, q);

        result.Items.Should().ContainSingle(w => w.Code == "EAST");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetPagedAsync_NameFilter_FiltersCorrectly()
    {
        _context.Warehouses.AddRange(
            NewWarehouse("MAIN", "Main warehouse", isDefault: true),
            NewWarehouse("EAST", "East depot"));
        await _context.SaveChangesAsync();

        var q = new GridQuery();
        q.Search[nameof(Warehouse.Name)] = "depot";
        var result = await _service.GetPagedAsync(_website.WebsiteID, q);

        result.Items.Should().ContainSingle(w => w.Code == "EAST");
    }

    [Fact]
    public async Task GetPagedAsync_ActiveFilter_FiltersCorrectly()
    {
        _context.Warehouses.AddRange(
            NewWarehouse("MAIN", "Main warehouse", isDefault: true, isActive: true),
            NewWarehouse("OLD", "Old warehouse", isActive: false));
        await _context.SaveChangesAsync();

        var q = new GridQuery();
        q.Search[nameof(Warehouse.IsActive)] = "false";
        var result = await _service.GetPagedAsync(_website.WebsiteID, q);

        result.Items.Should().ContainSingle(w => w.Code == "OLD");
    }

    [Fact]
    public async Task GetPagedAsync_DefaultFilter_FiltersCorrectly()
    {
        _context.Warehouses.AddRange(
            NewWarehouse("MAIN", "Main warehouse", isDefault: true),
            NewWarehouse("EAST", "East depot"));
        await _context.SaveChangesAsync();

        var q = new GridQuery();
        q.Search[nameof(Warehouse.IsDefault)] = "true";
        var result = await _service.GetPagedAsync(_website.WebsiteID, q);

        result.Items.Should().ContainSingle(w => w.IsDefault);
    }

    [Fact]
    public async Task GetPagedAsync_WebsiteFilter_ReturnsOnlyMatchingSite()
    {
        var other = NewWebsite("Other", "other.com");
        _context.Websites.Add(other);
        await _context.SaveChangesAsync();

        _context.Warehouses.AddRange(
            NewWarehouse("MAIN", "Main warehouse", isDefault: true),
            NewWarehouse("OTH", "Other warehouse", isDefault: true, websiteId: other.WebsiteID));
        await _context.SaveChangesAsync();

        var result = await _service.GetPagedAsync(_website.WebsiteID, new GridQuery());

        result.Items.Should().OnlyContain(w => w.WebsiteID == _website.WebsiteID);
        result.Items.Should().NotContain(w => w.Code == "OTH");
    }

    [Fact]
    public async Task GetPagedAsync_SortsByName()
    {
        _context.Warehouses.AddRange(
            NewWarehouse("Z", "Zebra"),
            NewWarehouse("A", "Alpha"));
        await _context.SaveChangesAsync();

        var q = new GridQuery { OrderBy = $"{nameof(Warehouse.Name)} ASC" };
        var result = await _service.GetPagedAsync(_website.WebsiteID, q);

        result.Items.Select(w => w.Name).Should().Equal("Alpha", "Zebra");
    }

    [Fact]
    public async Task GetPagedAsync_PaginatesAndReturnsCorrectTotalCount()
    {
        for (var i = 0; i < 12; i++)
            _context.Warehouses.Add(NewWarehouse($"W{i:00}", $"Warehouse {i:00}"));
        await _context.SaveChangesAsync();

        var q = new GridQuery { PageIndex = 2, PageSize = 5 };
        var result = await _service.GetPagedAsync(_website.WebsiteID, q);

        result.TotalCount.Should().Be(12);
        result.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task UpsertAsync_Update_ChangesNameAddressAndDefault()
    {
        var main = NewWarehouse("MAIN", "Main warehouse", isDefault: true);
        var east = NewWarehouse("EAST", "East depot");
        _context.Warehouses.AddRange(main, east);
        await _context.SaveChangesAsync();

        east.Name = "East hub";
        east.Address = "12 Dock Rd";
        east.IsDefault = true;
        await _service.UpsertAsync(east);

        var saved = await _context.Warehouses.AsNoTracking().FirstAsync(w => w.WarehouseID == east.WarehouseID);
        saved.Name.Should().Be("East hub");
        saved.Address.Should().Be("12 Dock Rd");
        saved.IsDefault.Should().BeTrue();

        (await _context.Warehouses.AsNoTracking().FirstAsync(w => w.WarehouseID == main.WarehouseID))
            .IsDefault.Should().BeFalse();
    }

    [Fact]
    public async Task UpsertAsync_DuplicateCode_Throws()
    {
        _context.Warehouses.Add(NewWarehouse("MAIN", "Main warehouse", isDefault: true));
        await _context.SaveChangesAsync();

        var act = () => _service.UpsertAsync(NewWarehouse("MAIN", "Another main"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*code already exists*");
    }
}
