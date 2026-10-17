using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace Dotnetable.Tests.Services;

public class SlowSellerServiceTests : IDisposable
{
    private const int SiteId = 1;
    private const int OtherSiteId = 2;

    private readonly RelationalTestDb _db;
    private readonly AppDbContext _context;
    private readonly SlowSellerService _service;

    public SlowSellerServiceTests()
    {
        _db = new RelationalTestDb();
        _context = _db.NewContext();
        _service = new SlowSellerService(new TestDbContextFactory(_db.Options));
    }

    public void Dispose()
    {
        _context.Dispose();
        _db.Dispose();
    }

    [Fact]
    public async Task List_Keeps_Products_At_Or_Under_The_Cutoff_For_The_Last_30_Days()
    {
        _context.Websites.Add(new Website
        {
            WebsiteID = SiteId, TradeName = "Shop", WebsiteAddress = "shop.example", AuthCode = Guid.NewGuid(),
            Active = true, Manager = "Mgr", Mobile = "1", Email = "a@shop.example",
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow), DefaultLanguageCode = "fa",
            DefaultCurrencyCode = "IRR", BrandName = "Shop",
        });
        AddProduct(1, "Quiet mug", compareAt: 120, price: 100, stock: 4);
        AddProduct(2, "Steady mug");
        AddProduct(3, "Busy mug");
        AddProduct(4, "Old hit");
        AddProduct(5, "Draft", status: 0);
        AddProduct(6, "Catalog only", catalogOnly: true);
        _context.SaveChanges();

        AddOrder(10, productId: 1, variantId: 1, quantity: 1, status: OrderStatus.Paid, paidAt: DateTime.UtcNow.AddDays(-2));
        AddOrder(11, productId: 2, variantId: 2, quantity: 2, status: OrderStatus.Completed, paidAt: DateTime.UtcNow.AddDays(-1));
        AddOrder(12, productId: 3, variantId: 3, quantity: 5, status: OrderStatus.Shipped, paidAt: DateTime.UtcNow.AddDays(-3));
        AddOrder(13, productId: 4, variantId: 4, quantity: 20, status: OrderStatus.Paid, paidAt: DateTime.UtcNow.AddDays(-40));
        AddOrder(14, productId: 1, variantId: 1, quantity: 9, status: OrderStatus.Cancelled, paidAt: DateTime.UtcNow);
        AddOrder(15, productId: 1, variantId: 1, quantity: 8, status: OrderStatus.PendingPayment, paidAt: null);
        AddOrder(16, productId: 2, variantId: 2, quantity: 7, status: OrderStatus.Paid, paidAt: DateTime.UtcNow, websiteId: OtherSiteId);
        _context.SaveChanges();

        var report = await _service.GetAsync(SiteId, maxUnitsSold: 2, search: null, new GridQuery { PageIndex = 1, PageSize = 20 });

        report.CurrencyCode.Should().Be("IRR");
        report.FromUtc.Should().BeCloseTo(DateTime.UtcNow.AddDays(-30), TimeSpan.FromMinutes(1));
        report.Items.Select(r => r.Title).Should().Equal("Old hit", "Quiet mug", "Steady mug");
        report.Items.Single(r => r.Title == "Quiet mug").UnitsSold.Should().Be(1);
        report.Items.Single(r => r.Title == "Quiet mug").HasDiscount.Should().BeTrue();
        report.Items.Single(r => r.Title == "Quiet mug").StockOnHand.Should().Be(4);
        report.Items.Single(r => r.Title == "Quiet mug").Price.Should().Be(100);
        report.Items.Single(r => r.Title == "Steady mug").UnitsSold.Should().Be(2);
        report.Items.Single(r => r.Title == "Steady mug").OrderCount.Should().Be(1);
        report.Items.Single(r => r.Title == "Old hit").UnitsSold.Should().Be(0);
        report.TotalCount.Should().Be(3);

        var searched = await _service.GetAsync(SiteId, 2, "Quiet", new GridQuery { PageIndex = 1, PageSize = 20 });
        searched.Items.Should().ContainSingle().Which.ProductId.Should().Be(1);
    }

    private void AddProduct(int id, string title, decimal price = 50, decimal? compareAt = null, int stock = 0, byte status = ProductService.PublishedStatus, bool catalogOnly = false)
    {
        var now = DateTime.UtcNow;
        _context.Products.Add(new Product
        {
            ProductID = id, WebsiteID = SiteId, Slug = $"p{id}", Title = title,
            Status = status, IsActive = true, IsCatalogOnly = catalogOnly,
            CreatedAt = now, UpdatedAt = now,
        });
        _context.ProductVariants.Add(new ProductVariant
        {
            ProductVariantID = id, WebsiteID = SiteId, ProductID = id, Sku = $"SKU-{id}", Title = title,
            IsDefault = true, IsActive = true, ReferencePrice = price, ReferencePriceUsd = price,
            CompareAtPrice = compareAt, CreatedAt = now,
        });
        if (stock > 0)
        {
            _context.InventoryItems.Add(new InventoryItem
            {
                WebsiteID = SiteId, ProductVariantID = id, QuantityOnHand = stock, RowVersion = new byte[] { 1 },
            });
        }
    }

    private void AddOrder(int id, int productId, int variantId, int quantity, OrderStatus status, DateTime? paidAt, int websiteId = SiteId)
    {
        var created = paidAt ?? DateTime.UtcNow;
        _context.Orders.Add(new Order
        {
            OrderID = id, WebsiteID = websiteId, WebsiteClientID = 1, OrderNumber = $"O-{id}",
            Status = (byte)status, CurrencyCode = "IRR", ExchangeRateToUsd = 1,
            CreatedAt = created, PaidAt = paidAt,
        });
        _context.OrderItems.Add(new OrderItem
        {
            OrderItemID = id, OrderID = id, WebsiteID = websiteId, SourceWebsiteID = SiteId,
            ProductVariantID = variantId, Quantity = quantity,
            TitleSnapshot = $"P{productId}", SkuSnapshot = $"SKU-{variantId}",
            UnitPrice = 50, UnitPriceUsd = 1, CatalogUnitPrice = 50, TotalPrice = 50 * quantity,
        });
    }
}
