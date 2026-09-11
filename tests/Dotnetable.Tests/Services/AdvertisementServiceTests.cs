using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class AdvertisementServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AdvertisementService _service;
    private readonly Website _website;

    public AdvertisementServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        var factory = new TestDbContextFactory(opts);
        _service = new AdvertisementService(factory);

        _website = new Website
        {
            TradeName = "Test", BrandName = "Test", WebsiteAddress = "test.com",
            AuthCode = Guid.NewGuid(), Active = true, Manager = "Mgr", Mobile = "1",
            Email = "a@test.com", RegisterDate = DateOnly.FromDateTime(DateTime.Today),
            DefaultLanguageCode = "en", DefaultCurrencyCode = "USD",
        };
        _context.Websites.Add(_website);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateAsync_PersistsKeywordUrlAndLocation()
    {
        var created = await _service.CreateAsync(new Advertisement
        {
            WebsiteID = _website.WebsiteID,
            Location = (byte)AdvertisementLocation.Header,
            Keyword = " Buy steel ",
            Url = "https://example.com/offer",
            OpenInNewTab = true,
            IsActive = true,
        });

        created.AdvertisementID.Should().BeGreaterThan(0);
        created.Keyword.Should().Be("Buy steel");
        created.Url.Should().Be("https://example.com/offer");

        _context.ChangeTracker.Clear();
        var stored = await _context.Advertisements.FindAsync(created.AdvertisementID);
        stored!.Keyword.Should().Be("Buy steel");
        stored.Location.Should().Be((byte)AdvertisementLocation.Header);
    }

    [Fact]
    public async Task CreateAsync_AcceptsSiteRelativeUrl()
    {
        var created = await _service.CreateAsync(new Advertisement
        {
            WebsiteID = _website.WebsiteID,
            Location = (byte)AdvertisementLocation.Footer,
            Keyword = "Shop",
            Url = "/shop",
            IsActive = true,
        });

        created.Url.Should().Be("/shop");
    }

    [Fact]
    public async Task CreateAsync_RejectsJavascriptUrl()
    {
        var act = () => _service.CreateAsync(new Advertisement
        {
            WebsiteID = _website.WebsiteID,
            Keyword = "x",
            Url = "javascript:alert(1)",
        });

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetByLocationAsync_ReturnsLocalizedKeywordAndFallsBack()
    {
        var ad = await _service.CreateAsync(new Advertisement
        {
            WebsiteID = _website.WebsiteID,
            Location = (byte)AdvertisementLocation.Home,
            Keyword = "Buy now",
            Url = "https://example.com/en",
            IsActive = true,
            SortOrder = 1,
        });
        await _service.SetTranslationsAsync(ad.AdvertisementID, new Dictionary<string, (string Keyword, string? Url)>
        {
            ["fa"] = ("همین حالا بخر", "https://example.com/fa"),
        });

        var fa = await _service.GetByLocationAsync(_website.WebsiteID, AdvertisementLocation.Home, "fa");
        fa.Should().ContainSingle();
        fa[0].Keyword.Should().Be("همین حالا بخر");
        fa[0].Url.Should().Be("https://example.com/fa");

        var en = await _service.GetByLocationAsync(_website.WebsiteID, AdvertisementLocation.Home, "en");
        en[0].Keyword.Should().Be("Buy now");
        en[0].Url.Should().Be("https://example.com/en");
    }

    [Fact]
    public async Task GetByLocationAsync_SkipsInactive()
    {
        await _service.CreateAsync(new Advertisement
        {
            WebsiteID = _website.WebsiteID,
            Location = (byte)AdvertisementLocation.Sidebar,
            Keyword = "Hidden",
            Url = "https://example.com",
            IsActive = false,
        });

        var ads = await _service.GetByLocationAsync(_website.WebsiteID, AdvertisementLocation.Sidebar);
        ads.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_RemovesTranslations()
    {
        var ad = await _service.CreateAsync(new Advertisement
        {
            WebsiteID = _website.WebsiteID,
            Location = (byte)AdvertisementLocation.Blog,
            Keyword = "Read",
            Url = "https://example.com",
            IsActive = true,
        });
        await _service.SetTranslationsAsync(ad.AdvertisementID, new Dictionary<string, (string Keyword, string? Url)>
        {
            ["fa"] = ("بخوانید", null),
        });

        await _service.DeleteAsync(ad.AdvertisementID);

        _context.ChangeTracker.Clear();
        (await _context.Advertisements.FindAsync(ad.AdvertisementID)).Should().BeNull();
        (await _context.AdvertisementTranslations.CountAsync(t => t.AdvertisementID == ad.AdvertisementID)).Should().Be(0);
    }

    public void Dispose() => _context.Dispose();
}
