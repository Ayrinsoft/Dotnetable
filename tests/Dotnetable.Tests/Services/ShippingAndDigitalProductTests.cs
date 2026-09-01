using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Dotnetable.Tests.Services;

public class ShippingAndDigitalProductTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TestDbContextFactory _factory;
    private readonly ShippingService _shipping;
    private readonly VendorProductService _listings;
    private readonly Website _website;

    public ShippingAndDigitalProductTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        // Services open a context per call now; the _factory points at the same database so the
        // fixture can still seed and assert through its own _context.
        _factory = new TestDbContextFactory(options);
        _website = new Website
        {
            TradeName = "Test",
            WebsiteAddress = "test.local",
            AuthCode = Guid.NewGuid(),
            Active = true,
            Manager = "m",
            Mobile = "0",
            Email = "t@t.com",
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            AllowAllIP = true,
            DefaultLanguageCode = "en",
            DefaultCurrencyCode = "USD",
            BrandName = "Test",
            WebsiteType = 0,
        };
        _context.Websites.Add(_website);
        _context.SaveChanges();

        var currency = new Mock<ICurrencyConversionService>();
        currency.Setup(c => c.ToUsdAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int _, decimal amount, string? _, CancellationToken _) => amount);
        currency.Setup(c => c.ToDisplayAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int _, decimal usd, string? _, CancellationToken _) =>
                new MoneyDto { Amount = usd, AmountUsd = usd, CurrencyCode = "USD" });

        _shipping = new ShippingService(_factory, currency.Object);
        _listings = new VendorProductService(_factory);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Shipping_Quote_AppliesPrepaidAndCodFloors()
    {
        var method = await _shipping.CreateAsync(new ShippingMethod
        {
            WebsiteID = _website.WebsiteID,
            Title = "Tipax",
            CarrierName = "Tipax",
            SupportsPrepaid = true,
            SupportsCod = true,
            PrepaidMinPrice = 100,
            PrepaidMinPriceUsd = 100,
            CodMinPrice = 0,
            CodMinPriceUsd = 0,
            IsActive = true,
            SortOrder = 1,
        });

        await _shipping.CreateRateAsync(new ShippingRate
        {
            ShippingMethodID = method.ShippingMethodID,
            Price = 50,
            PriceUsd = 50,
            IsActive = true,
        });

        var quotes = await _shipping.GetAvailableWithPricesAsync(_website.WebsiteID, null, null, null, 1m);
        quotes.Should().ContainSingle();
        var q = quotes[0];
        q.PrepaidPriceUsd.Should().Be(100); // floor wins over zone 50
        q.CodPriceUsd.Should().Be(50);      // zone 50 > cod floor 0
        q.PriceUsd.Should().Be(100);        // default prefers prepaid
    }

    [Fact]
    public async Task Shipping_Quote_WorksWithoutZoneRates_UsingMinPrices()
    {
        await _shipping.CreateAsync(new ShippingMethod
        {
            WebsiteID = _website.WebsiteID,
            Title = "Post",
            CarrierName = "Post",
            SupportsPrepaid = true,
            SupportsCod = true,
            PrepaidMinPrice = 80,
            PrepaidMinPriceUsd = 80,
            CodMinPrice = 0,
            CodMinPriceUsd = 0,
            IsActive = true,
        });

        var quotes = await _shipping.GetAvailableWithPricesAsync(_website.WebsiteID, null, null, null, 0);
        quotes.Should().ContainSingle();
        quotes[0].PrepaidPriceUsd.Should().Be(80);
        quotes[0].CodPriceUsd.Should().Be(0);
    }

    [Fact]
    public async Task VendorProduct_UnlimitedStock_OnlyForDigital()
    {
        var physical = new Product
        {
            WebsiteID = _website.WebsiteID, Slug = "phys", Title = "Physical",
            ProductType = (byte)ProductType.Physical, RequiresShipping = true,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        var digital = new Product
        {
            WebsiteID = _website.WebsiteID, Slug = "pdf", Title = "PDF Book",
            ProductType = (byte)ProductType.DigitalDownload, RequiresShipping = false,
            IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        _context.Products.AddRange(physical, digital);
        await _context.SaveChangesAsync();

        var pv = new ProductVariant
        {
            WebsiteID = _website.WebsiteID, ProductID = physical.ProductID, Sku = "P1", Title = "Default",
            IsDefault = true, IsActive = true, ReferencePrice = 10, ReferencePriceUsd = 10, CreatedAt = DateTime.UtcNow,
        };
        var dv = new ProductVariant
        {
            WebsiteID = _website.WebsiteID, ProductID = digital.ProductID, Sku = "D1", Title = "Default",
            IsDefault = true, IsActive = true, ReferencePrice = 5, ReferencePriceUsd = 5, CreatedAt = DateTime.UtcNow,
        };
        _context.ProductVariants.AddRange(pv, dv);

        var vendor = new Vendor
        {
            WebsiteID = _website.WebsiteID, Name = "Store", Slug = "store", VendorType = (byte)VendorType.Display,
            IsActive = true,
        };
        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();

        var (okPhys, errPhys, _) = await _listings.UpsertAsync(new VendorProduct
        {
            VendorID = vendor.VendorID, ProductVariantID = pv.ProductVariantID,
            ReferencePrice = 10, ReferencePriceUsd = 10, StockQuantity = -1, IsActive = true,
        });
        okPhys.Should().BeFalse();
        errPhys.Should().Contain("digital");

        var (okDig, errDig, listing) = await _listings.UpsertAsync(new VendorProduct
        {
            VendorID = vendor.VendorID, ProductVariantID = dv.ProductVariantID,
            ReferencePrice = 5, ReferencePriceUsd = 5, StockQuantity = -1, IsActive = true,
        });
        okDig.Should().BeTrue(errDig);
        listing!.StockQuantity.Should().Be(-1);
        IVendorProductService.IsUnlimited(listing).Should().BeTrue();
        IVendorProductService.Available(listing).Should().Be(int.MaxValue);

        (await _listings.ReserveAsync(listing.VendorProductID, 100)).Should().BeTrue();
        await _listings.CommitSaleAsync(listing.VendorProductID, 100);
        var after = await _context.VendorProducts.SingleAsync(x => x.VendorProductID == listing.VendorProductID);
        after.StockQuantity.Should().Be(-1); // never depletes
    }

    [Fact]
    public void StockDisplay_UnlimitedAggregation()
    {
        StockDisplay.IsUnlimited(-1).Should().BeTrue();
        StockDisplay.IsInStock(-1).Should().BeTrue();
        StockDisplay.ExactCountOrNull(-1).Should().BeNull();
        StockDisplay.Aggregate(new[] { 3, -1, 5 }).Should().Be(-1);
        StockDisplay.Aggregate(new[] { 3, 2 }).Should().Be(5);
        StockDisplay.Max(-1, 4).Should().Be(-1);
    }
}
