using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class WebsiteServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly WebsiteService _service;

    public WebsiteServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        _service = new WebsiteService(_context);
    }

    private static Website NewWebsite(
        string trade = "Test", string address = "test.com",
        string manager = "Mgr", bool active = true) => new()
    {
        TradeName = trade,
        BrandName = trade,
        WebsiteAddress = address,
        AuthCode = Guid.NewGuid(),
        Active = active,
        Manager = manager,
        Mobile = "123",
        Email = $"admin@{address}",
        RegisterDate = DateOnly.FromDateTime(DateTime.Today),
        DefaultLanguageCode = "en"
    };

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsWebsite()
    {
        var w = NewWebsite(); _context.Websites.Add(w); await _context.SaveChangesAsync();

        (await _service.GetByIdAsync(w.WebsiteID)).Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_Missing_ReturnsNull()
    {
        (await _service.GetByIdAsync(999)).Should().BeNull();
    }

    [Fact]
    public async Task GetByAddressAsync_MatchingAddress_ReturnsWebsite()
    {
        var w = NewWebsite(address: "mysite.com");
        _context.Websites.Add(w); await _context.SaveChangesAsync();

        var result = await _service.GetByAddressAsync("mysite.com");

        result.Should().NotBeNull();
        result!.WebsiteAddress.Should().Be("mysite.com");
    }

    [Fact]
    public async Task GetByAddressAsync_NoMatch_ReturnsNull()
    {
        (await _service.GetByAddressAsync("notexist.com")).Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllWebsitesOrderedById()
    {
        _context.Websites.AddRange(NewWebsite("A"), NewWebsite("B"), NewWebsite("C"));
        await _context.SaveChangesAsync();

        var result = (await _service.GetAllAsync()).ToList();

        result.Should().HaveCount(3);
        result.Select(w => w.WebsiteID).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetPagedAsync_TradeNameFilter_FiltersCorrectly()
    {
        _context.Websites.AddRange(
            NewWebsite("Alpha Corp"),
            NewWebsite("Beta Inc"),
            NewWebsite("Alpha Ltd"));
        await _context.SaveChangesAsync();

        var q = new GridQuery(); q.Search["TradeName"] = "Alpha";
        var result = await _service.GetPagedAsync(q);

        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(w => w.TradeName.Contains("Alpha"));
    }

    [Fact]
    public async Task GetPagedAsync_BrandNameFilter_FiltersCorrectly()
    {
        _context.Websites.AddRange(
            new Website
            {
                TradeName = "X", BrandName = "BrandX", WebsiteAddress = "x.com", AuthCode = Guid.NewGuid(),
                Active = true, Manager = "Mgr", Mobile = "1", Email = "x@x.com",
                RegisterDate = DateOnly.FromDateTime(DateTime.Today), DefaultLanguageCode = "en"
            },
            new Website
            {
                TradeName = "Y", BrandName = "BrandY", WebsiteAddress = "y.com", AuthCode = Guid.NewGuid(),
                Active = true, Manager = "Mgr", Mobile = "1", Email = "y@y.com",
                RegisterDate = DateOnly.FromDateTime(DateTime.Today), DefaultLanguageCode = "en"
            });
        await _context.SaveChangesAsync();

        var q = new GridQuery(); q.Search["BrandName"] = "BrandX";
        var result = await _service.GetPagedAsync(q);

        result.Items.Should().ContainSingle(w => w.BrandName == "BrandX");
    }

    [Fact]
    public async Task GetPagedAsync_AddressFilter_FiltersCorrectly()
    {
        _context.Websites.AddRange(NewWebsite(address: "shop.com"), NewWebsite(address: "blog.com"));
        await _context.SaveChangesAsync();

        var q = new GridQuery(); q.Search["WebsiteAddress"] = "shop";
        var result = await _service.GetPagedAsync(q);

        result.Items.Should().ContainSingle(w => w.WebsiteAddress == "shop.com");
    }

    [Fact]
    public async Task GetPagedAsync_ManagerFilter_FiltersCorrectly()
    {
        _context.Websites.AddRange(
            NewWebsite(manager: "Alice"),
            NewWebsite(manager: "Bob"),
            NewWebsite(address: "other.com", manager: "Alice"));
        await _context.SaveChangesAsync();

        var q = new GridQuery(); q.Search["Manager"] = "Alice";
        var result = await _service.GetPagedAsync(q);

        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPagedAsync_ActiveFilter_FiltersCorrectly()
    {
        _context.Websites.AddRange(NewWebsite(active: true), NewWebsite("Inactive", "inactive.com", active: false));
        await _context.SaveChangesAsync();

        var q = new GridQuery(); q.Search["Active"] = "false";
        var result = await _service.GetPagedAsync(q);

        result.Items.Should().ContainSingle(w => w.TradeName == "Inactive");
    }

    [Fact]
    public async Task GetPagedAsync_PaginatesAndReturnsCorrectTotalCount()
    {
        for (int i = 0; i < 12; i++)
            _context.Websites.Add(NewWebsite($"Site{i}", $"site{i}.com"));
        await _context.SaveChangesAsync();

        var q = new GridQuery { PageSize = 4 };
        var result = await _service.GetPagedAsync(q);

        result.TotalCount.Should().Be(12);
        result.Items.Should().HaveCount(4);
    }

    [Fact]
    public async Task CreateAsync_PersistsWebsiteAndAssignsId()
    {
        var w = NewWebsite("New Site");

        var created = await _service.CreateAsync(w);

        created.WebsiteID.Should().BeGreaterThan(0);
        (await _context.Websites.FindAsync(created.WebsiteID))!.TradeName.Should().Be("New Site");
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var w = NewWebsite(); _context.Websites.Add(w); await _context.SaveChangesAsync();
        w.TradeName = "Updated";

        await _service.UpdateAsync(w);

        (await _context.Websites.FindAsync(w.WebsiteID))!.TradeName.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteAsync_RemovesWebsite()
    {
        var w = NewWebsite(); _context.Websites.Add(w); await _context.SaveChangesAsync();

        await _service.DeleteAsync(w.WebsiteID);

        (await _context.Websites.FindAsync(w.WebsiteID)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_MissingId_DoesNotThrow()
    {
        await _service.Invoking(s => s.DeleteAsync(999)).Should().NotThrowAsync();
    }

    public void Dispose() => _context.Dispose();
}
