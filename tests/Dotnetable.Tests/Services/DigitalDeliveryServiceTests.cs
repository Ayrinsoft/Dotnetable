using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class DigitalDeliveryServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly DigitalDeliveryService _digital;
    private readonly Website _website;
    private readonly WebsiteClient _client;

    public DigitalDeliveryServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        _digital = new DigitalDeliveryService(_context);

        _website = new Website
        {
            TradeName = "Host", WebsiteAddress = "host.test", AuthCode = Guid.NewGuid(),
            Active = true, Manager = "M", Mobile = "1", Email = "h@t.com",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today),
            DefaultLanguageCode = "en", DefaultCurrencyCode = "USD", BrandName = "Host",
        };
        _context.Websites.Add(_website);
        _context.SaveChanges();

        _client = new WebsiteClient
        {
            WebsiteID = _website.WebsiteID,
            Email = "c@t.com",
            Password = "x",
            Active = true,
            RegisterDate = DateOnly.FromDateTime(DateTime.Today),
            HashKey = Guid.NewGuid(),
            Givenname = "C",
            Surname = "Ust",
        };
        // WebsiteClient may require more fields — fill after save if needed via entity check
        _context.WebsiteClients.Add(_client);
        _context.SaveChanges();
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task Grant_CreatesEntitlement_OnPaidDigitalOrder()
    {
        var product = new Product
        {
            WebsiteID = _website.WebsiteID, Slug = "pdf", Title = "PDF Book",
            ProductType = (byte)ProductType.DigitalDownload, RequiresShipping = false,
            DigitalDownloadUrl = "https://cdn.example.com/book.pdf",
            DigitalDeliveryNote = "CODE-123",
            IsActive = true, Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var variant = new ProductVariant
        {
            WebsiteID = _website.WebsiteID, ProductID = product.ProductID,
            Sku = "PDF-1", Title = "Default", IsDefault = true, IsActive = true,
            ReferencePrice = 10, ReferencePriceUsd = 10, CreatedAt = DateTime.UtcNow,
        };
        _context.ProductVariants.Add(variant);
        await _context.SaveChangesAsync();

        var order = new Order
        {
            WebsiteID = _website.WebsiteID,
            WebsiteClientID = _client.WebsiteClientID,
            OrderNumber = "T-1",
            Status = (byte)OrderStatus.Paid,
            CurrencyCode = "USD",
            ExchangeRateToUsd = 1,
            SubTotal = 10, DiscountTotal = 0, ShippingTotal = 0, TaxTotal = 0,
            GrandTotal = 10, GrandTotalUsd = 10,
            CreatedAt = DateTime.UtcNow,
            PaidAt = DateTime.UtcNow,
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var line = new OrderItem
        {
            OrderID = order.OrderID,
            WebsiteID = _website.WebsiteID,
            SourceWebsiteID = _website.WebsiteID,
            ProductVariantID = variant.ProductVariantID,
            TitleSnapshot = "PDF Book",
            SkuSnapshot = "PDF-1",
            Quantity = 1,
            UnitPrice = 10, UnitPriceUsd = 10, UnitCostUsd = 0,
            DiscountAmount = 0, TotalPrice = 10,
        };
        _context.OrderItems.Add(line);
        await _context.SaveChangesAsync();

        await _digital.GrantForOrderAsync(order.OrderID);
        await _digital.GrantForOrderAsync(order.OrderID); // idempotent

        var assets = await _context.OrderDigitalAssets.Where(a => a.OrderID == order.OrderID).ToListAsync();
        assets.Should().ContainSingle();
        assets[0].DigitalDeliveryNote.Should().Be("CODE-123");
        assets[0].DigitalDownloadUrl.Should().Be("https://cdn.example.com/book.pdf");
        assets[0].WebsiteClientID.Should().Be(_client.WebsiteClientID);

        var dl = await _digital.DownloadAsync(
            _website.WebsiteID, _client.WebsiteClientID, assets[0].OrderDigitalAssetID, "1.1.1.1", "ua");
        dl.Success.Should().BeTrue();
        dl.DownloadUrl.Should().Be("https://cdn.example.com/book.pdf");

        var (ok, err) = await _digital.LogAccessAsync(
            _website.WebsiteID, _client.WebsiteClientID, assets[0].OrderDigitalAssetID,
            DigitalAccessType.ViewCode, "127.0.0.1", "test-agent");
        ok.Should().BeTrue(err);

        var logs = await _context.DigitalAccessLogs
            .Where(l => l.OrderDigitalAssetID == assets[0].OrderDigitalAssetID)
            .OrderBy(l => l.DigitalAccessLogID)
            .ToListAsync();
        logs.Should().HaveCount(2); // Download + ViewCode
        logs.Should().Contain(l => l.AccessType == (byte)DigitalAccessType.Download);
        logs.Should().Contain(l => l.AccessType == (byte)DigitalAccessType.ViewCode && l.IpAddress == "127.0.0.1");

        var library = await _digital.GetClientLibraryAsync(
            _website.WebsiteID, _client.WebsiteClientID, new Application.DTOs.GridQuery { PageIndex = 1, PageSize = 10 });
        library.TotalCount.Should().Be(1);
        library.Items[0].HasDeliveryNote.Should().BeTrue();
        library.Items[0].HasDownloadUrl.Should().BeTrue();
    }

    [Fact]
    public async Task Grant_SkipsPhysicalProducts()
    {
        var product = new Product
        {
            WebsiteID = _website.WebsiteID, Slug = "mug", Title = "Mug",
            ProductType = (byte)ProductType.Physical, RequiresShipping = true,
            IsActive = true, Status = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        var variant = new ProductVariant
        {
            WebsiteID = _website.WebsiteID, ProductID = product.ProductID,
            Sku = "MUG", Title = "Default", IsDefault = true, IsActive = true,
            ReferencePrice = 5, ReferencePriceUsd = 5, CreatedAt = DateTime.UtcNow,
        };
        _context.ProductVariants.Add(variant);

        var order = new Order
        {
            WebsiteID = _website.WebsiteID, WebsiteClientID = _client.WebsiteClientID,
            OrderNumber = "T-2", Status = (byte)OrderStatus.Paid, CurrencyCode = "USD",
            ExchangeRateToUsd = 1, GrandTotal = 5, GrandTotalUsd = 5, CreatedAt = DateTime.UtcNow,
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        _context.OrderItems.Add(new OrderItem
        {
            OrderID = order.OrderID, WebsiteID = _website.WebsiteID, SourceWebsiteID = _website.WebsiteID,
            ProductVariantID = variant.ProductVariantID, TitleSnapshot = "Mug", SkuSnapshot = "MUG",
            Quantity = 1, UnitPrice = 5, UnitPriceUsd = 5, TotalPrice = 5,
        });
        await _context.SaveChangesAsync();

        await _digital.GrantForOrderAsync(order.OrderID);
        (await _context.OrderDigitalAssets.CountAsync()).Should().Be(0);
    }
}
