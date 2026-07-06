using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class ThemeServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ThemeService _service;
    private readonly Website _website;

    public ThemeServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        _service = new ThemeService(_context);

        _website = NewWebsite("Test", "test.com");
        _context.Websites.Add(_website);
        _context.SaveChanges();
    }

    private static Website NewWebsite(string trade, string address) => new()
    {
        TradeName = trade, BrandName = trade, WebsiteAddress = address,
        AuthCode = Guid.NewGuid(), Active = true, Manager = "Mgr", Mobile = "123",
        Email = $"admin@{address}", RegisterDate = DateOnly.FromDateTime(DateTime.Today),
        DefaultLanguageCode = "en", DefaultCurrencyCode = "USD",
    };

    private WebsiteTheme NewTheme(string name, int? websiteId = null, bool active = false) => new()
    {
        WebsiteID = websiteId ?? _website.WebsiteID,
        Name = name,
        SettingsJson = """{"colors":{"primary":"#4f46e5"}}""",
        IsActive = active,
    };

    [Fact]
    public async Task CreateThemeAsync_PersistsAndAssignsId()
    {
        var theme = await _service.CreateThemeAsync(NewTheme("Ocean"));

        theme.WebsiteThemeID.Should().BeGreaterThan(0);
        (await _context.WebsiteThemes.FindAsync(theme.WebsiteThemeID))!.Name.Should().Be("Ocean");
    }

    [Fact]
    public async Task CreateThemeAsync_CreatedActive_DeactivatesExistingActiveTheme()
    {
        var old = await _service.CreateThemeAsync(NewTheme("Old", active: true));

        await _service.CreateThemeAsync(NewTheme("New", active: true));

        (await _context.WebsiteThemes.FindAsync(old.WebsiteThemeID))!.IsActive.Should().BeFalse();
        _context.WebsiteThemes.Count(t => t.IsActive).Should().Be(1);
    }

    [Fact]
    public async Task GetThemesAsync_WebsiteFilter_ReturnsOnlyMatchingSite()
    {
        var other = NewWebsite("Other", "other.com");
        _context.Websites.Add(other);
        await _context.SaveChangesAsync();

        await _service.CreateThemeAsync(NewTheme("Mine"));
        await _service.CreateThemeAsync(NewTheme("Theirs", other.WebsiteID));

        (await _service.GetThemesAsync(_website.WebsiteID)).Should().ContainSingle(t => t.Name == "Mine");
        (await _service.GetThemesAsync(null)).Should().HaveCount(2);
    }

    [Fact]
    public async Task ActivateThemeAsync_DeactivatesSiblingsOfSameWebsiteOnly()
    {
        var other = NewWebsite("Other", "other.com");
        _context.Websites.Add(other);
        await _context.SaveChangesAsync();

        var a = await _service.CreateThemeAsync(NewTheme("A", active: true));
        var b = await _service.CreateThemeAsync(NewTheme("B"));
        var foreign = await _service.CreateThemeAsync(NewTheme("Foreign", other.WebsiteID, active: true));

        await _service.ActivateThemeAsync(b.WebsiteThemeID);

        (await _context.WebsiteThemes.FindAsync(a.WebsiteThemeID))!.IsActive.Should().BeFalse();
        (await _context.WebsiteThemes.FindAsync(b.WebsiteThemeID))!.IsActive.Should().BeTrue();
        // The other website's active theme is untouched.
        (await _context.WebsiteThemes.FindAsync(foreign.WebsiteThemeID))!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ActivateThemeAsync_MissingId_DoesNotThrow()
    {
        await _service.Invoking(s => s.ActivateThemeAsync(999)).Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetActiveThemeAsync_ReturnsActiveThemeForWebsite()
    {
        await _service.CreateThemeAsync(NewTheme("Inactive"));
        var active = await _service.CreateThemeAsync(NewTheme("Active", active: true));

        var result = await _service.GetActiveThemeAsync(_website.WebsiteID);

        result.Should().NotBeNull();
        result!.WebsiteThemeID.Should().Be(active.WebsiteThemeID);
    }

    [Fact]
    public async Task GetActiveThemeAsync_NoActiveTheme_ReturnsNull()
    {
        await _service.CreateThemeAsync(NewTheme("Inactive"));

        (await _service.GetActiveThemeAsync(_website.WebsiteID)).Should().BeNull();
    }

    [Fact]
    public async Task UpdateThemeAsync_PersistsChanges()
    {
        var theme = await _service.CreateThemeAsync(NewTheme("Base"));
        theme.Name = "Renamed";
        theme.SettingsJson = """{"colors":{"primary":"#000000"}}""";

        await _service.UpdateThemeAsync(theme);

        var stored = await _context.WebsiteThemes.FindAsync(theme.WebsiteThemeID);
        stored!.Name.Should().Be("Renamed");
        stored.SettingsJson.Should().Contain("#000000");
    }

    [Fact]
    public async Task DeleteThemeAsync_RemovesTheme()
    {
        var theme = await _service.CreateThemeAsync(NewTheme("Doomed"));

        await _service.DeleteThemeAsync(theme.WebsiteThemeID);

        _context.WebsiteThemes.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteThemeAsync_MissingId_DoesNotThrow()
    {
        await _service.Invoking(s => s.DeleteThemeAsync(999)).Should().NotThrowAsync();
    }

    public void Dispose() => _context.Dispose();
}
