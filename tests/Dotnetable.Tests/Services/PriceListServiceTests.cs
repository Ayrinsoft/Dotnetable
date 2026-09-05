using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class PriceListServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PriceListService _service;
    private readonly Website _website;
    private readonly CurrencyRate _rate;

    public PriceListServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        var factory = new TestDbContextFactory(opts);
        _service = new PriceListService(factory, new CurrencyRateService(factory));

        _context.Currencies.Add(new Currency
        {
            CurrencyCode = "IRR",
            Name = "Iranian Rial",
            Symbol = "﷼",
            DecimalDigits = 0,
            IsActive = true,
        });
        _website = new Website
        {
            TradeName = "Test", BrandName = "Test", WebsiteAddress = "test.com",
            AuthCode = Guid.NewGuid(), Active = true, Manager = "Mgr", Mobile = "1",
            Email = "a@test.com", RegisterDate = DateOnly.FromDateTime(DateTime.Today),
            DefaultLanguageCode = "fa", DefaultCurrencyCode = "IRR",
        };
        _context.Websites.Add(_website);
        _context.SaveChanges();

        _rate = new CurrencyRate
        {
            WebsiteID = _website.WebsiteID,
            CurrencyCode = "IRR",
            USDToCurrency = 1_000_000m,
            IsDefault = true,
            LastUpdate = DateTime.UtcNow.AddHours(-2),
        };
        _context.CurrencyRates.Add(_rate);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Published_Default_UsesLocalPrice_WithoutUsd()
    {
        var list = await _service.CreateAsync(new PriceList
        {
            WebsiteID = _website.WebsiteID,
            Title = "Local",
            IsActive = true,
        });
        await _service.CreateItemAsync(new PriceListItem
        {
            PriceListID = list.PriceListID,
            Title = "Sheet",
            FixedPrice = 750_000m,
            IsActive = true,
        });

        var published = await _service.GetPublishedBySlugAsync(_website.WebsiteID, "local");
        published.Should().NotBeNull();
        published!.FollowsUsd.Should().BeFalse();
        published.ExchangeRateLastUpdate.Should().BeNull();
        published.Groups[0].Items[0].Price.Amount.Should().Be(750_000m);
        published.Groups[0].Items[0].LinkToUsd.Should().BeFalse();
    }

    [Fact]
    public async Task Published_LinkedItem_UsesUsdTimesCurrentRate()
    {
        var list = await _service.CreateAsync(new PriceList
        {
            WebsiteID = _website.WebsiteID,
            Title = "Steel",
            Slug = "steel",
            IsActive = true,
            Pricing = (byte)PriceListPricing.LinkedToUsd,
        });
        await _service.CreateItemAsync(new PriceListItem
        {
            PriceListID = list.PriceListID,
            Title = "Profile 40x40",
            GroupName = "Profiles",
            Unit = "kg",
            BasePriceUsd = 85m,
            LinkToUsd = true,
            IsActive = true,
        });

        var published = await _service.GetPublishedBySlugAsync(_website.WebsiteID, "steel");
        published.Should().NotBeNull();
        published!.CurrencyCode.Should().Be("IRR");
        published.Groups.Should().ContainSingle();
        var item = published.Groups[0].Items.Should().ContainSingle().Subject;
        item.Price.AmountUsd.Should().Be(85m);
        item.Price.Amount.Should().Be(85_000_000m);
    }

    [Fact]
    public async Task RateChange_UpdatesDisplayPrice_WithoutEditingItem()
    {
        var list = await _service.CreateAsync(new PriceList
        {
            WebsiteID = _website.WebsiteID,
            Title = "Steel",
            IsActive = true,
            Pricing = (byte)PriceListPricing.LinkedToUsd,
        });
        await _service.CreateItemAsync(new PriceListItem
        {
            PriceListID = list.PriceListID,
            Title = "Sheet",
            BasePriceUsd = 10m,
            LinkToUsd = true,
            IsActive = true,
        });

        var before = await _service.GetPublishedBySlugAsync(_website.WebsiteID, "steel");
        before!.Groups[0].Items[0].Price.Amount.Should().Be(10_000_000m);

        var updated = await _service.UpdateUsdRateAsync(_website.WebsiteID, 1_200_000m);
        updated.Should().NotBeNull();
        updated!.UsdToCurrency.Should().Be(1_200_000m);

        _context.ChangeTracker.Clear();
        var after = await _service.GetPublishedBySlugAsync(_website.WebsiteID, "steel");
        after!.Groups[0].Items[0].Price.Amount.Should().Be(12_000_000m);
        after.LastUpdatedAt.Should().BeOnOrAfter(updated.LastUpdate!.Value);
    }

    [Fact]
    public async Task FixedItem_DoesNotMove_WhenRateChanges()
    {
        var list = await _service.CreateAsync(new PriceList
        {
            WebsiteID = _website.WebsiteID,
            Title = "Fixed",
            IsActive = true,
        });
        await _service.CreateItemAsync(new PriceListItem
        {
            PriceListID = list.PriceListID,
            Title = "Special",
            LinkToUsd = false,
            FixedPrice = 500_000m,
            IsActive = true,
        });

        await _service.UpdateUsdRateAsync(_website.WebsiteID, 2_000_000m);
        _context.ChangeTracker.Clear();

        var published = await _service.GetPublishedBySlugAsync(_website.WebsiteID, "fixed");
        published!.Groups[0].Items[0].Price.Amount.Should().Be(500_000m);
        published.Groups[0].Items[0].LinkToUsd.Should().BeFalse();
    }

    [Fact]
    public async Task CatalogSource_ReadsLiveProductPrice()
    {
        _context.Products.Add(new Product
        {
            WebsiteID = _website.WebsiteID,
            Title = "Pipe",
            Slug = "pipe",
            IsActive = true,
            Status = ProductService.PublishedStatus,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();
        var product = await _context.Products.SingleAsync(p => p.Slug == "pipe");
        _context.ProductVariants.Add(new ProductVariant
        {
            WebsiteID = _website.WebsiteID,
            ProductID = product.ProductID,
            Sku = "PIPE-1",
            Title = "Default",
            IsDefault = true,
            IsActive = true,
            ReferencePrice = 1_250_000m,
            ReferencePriceUsd = 1.25m,
            CreatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();

        await _service.CreateAsync(new PriceList
        {
            WebsiteID = _website.WebsiteID,
            Title = "Shop",
            Slug = "shop",
            IsActive = true,
            Source = (byte)PriceListSource.CatalogProducts,
        });

        var published = await _service.GetPublishedBySlugAsync(_website.WebsiteID, "shop");
        published.Should().NotBeNull();
        published!.Source.Should().Be(PriceListSource.CatalogProducts);
        published.FollowsUsd.Should().BeFalse();
        var row = published.Groups.SelectMany(g => g.Items).Should().ContainSingle().Subject;
        row.Title.Should().Be("Pipe");
        row.Price.Amount.Should().Be(1_250_000m);
        row.ProductSlug.Should().Be("pipe");
    }

    [Fact]
    public async Task InactiveList_IsNotPublished()
    {
        await _service.CreateAsync(new PriceList
        {
            WebsiteID = _website.WebsiteID,
            Title = "Hidden",
            Slug = "hidden",
            IsActive = false,
        });

        var lists = await _service.GetPublishedAsync(_website.WebsiteID);
        lists.Should().BeEmpty();
        (await _service.GetPublishedBySlugAsync(_website.WebsiteID, "hidden")).Should().BeNull();
    }

    public void Dispose() => _context.Dispose();
}
