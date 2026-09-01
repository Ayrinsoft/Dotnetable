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

public class StockDocumentReturnWorkflowTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly StockDocumentService _docs;

    public StockDocumentReturnWorkflowTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        // Services open a context per call now, so they get a factory over the same options;
        // the fixture keeps its own _context for seeding and asserting.
        var factory = new TestDbContextFactory(opts);

        var warehouses = new Mock<IWarehouseService>();
        warehouses.Setup(w => w.EnsureDefaultAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var inventory = new Mock<IInventoryService>();
        var vendors = new Mock<IVendorProductService>();
        var notes = new Mock<IAdminNotificationService>();
        notes.Setup(n => n.NotifyRoleAsync(
                It.IsAny<int>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<AdminNotificationType>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var ledger = new Mock<IFinancialLedgerService>();

        _docs = new StockDocumentService(factory, warehouses.Object, inventory.Object, vendors.Object, notes.Object, ledger.Object);
        Seed();
    }

    public void Dispose() => _context.Dispose();

    private void Seed()
    {
        _context.Websites.Add(new Website
        {
            WebsiteID = 1, TradeName = "Shop", BrandName = "Shop", WebsiteAddress = "shop.local",
            AuthCode = Guid.NewGuid(), Active = true, Manager = "M", Mobile = "1", Email = "a@b.c",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today), DefaultLanguageCode = "en", DefaultCurrencyCode = "USD",
        });
        _context.Warehouses.AddRange(
            new Warehouse { WarehouseID = 1, WebsiteID = 1, Code = "MAIN", Name = "Main", IsDefault = true, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Warehouse { WarehouseID = 2, WebsiteID = 1, Code = "QC", Name = "QC", IsDefault = false, IsActive = true, CreatedAt = DateTime.UtcNow });
        _context.WebsiteClients.Add(new WebsiteClient
        {
            WebsiteClientID = 10, WebsiteID = 1, Cellphone = "09", CountryCode = "98", Email = "c@t.local",
            Givenname = "C", Surname = "U", Active = true, RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            HashKey = Guid.NewGuid(), ClientLevel = 1, Password = "x",
        });
        _context.Products.Add(new Product
        {
            ProductID = 1, WebsiteID = 1, Slug = "widget", Title = "Widget",
            ProductType = (byte)ProductType.Physical, RequiresShipping = true,
            IsActive = true, CreatedAt = DateTime.UtcNow,
        });
        _context.ProductVariants.Add(new ProductVariant
        {
            ProductVariantID = 1, ProductID = 1, Sku = "SKU-1", Title = "Widget default",
            IsActive = true, CreatedAt = DateTime.UtcNow,
        });
        var order = new Order
        {
            OrderID = 50, WebsiteID = 1, WebsiteClientID = 10, OrderNumber = "ORD-50",
            Status = (byte)OrderStatus.Shipped, CurrencyCode = "USD", ExchangeRateToUsd = 1,
            SubTotal = 100, GrandTotal = 100, GrandTotalUsd = 100, PaidAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow,
        };
        order.OrderItems.Add(new OrderItem
        {
            OrderItemID = 500, OrderID = 50, WebsiteID = 1, SourceWebsiteID = 1,
            ProductVariantID = 1, Quantity = 2,
            UnitPrice = 50, UnitPriceUsd = 50, UnitCostUsd = 20, TotalPrice = 100,
            TitleSnapshot = "Widget", SkuSnapshot = "SKU-1",
        });
        _context.Orders.Add(order);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateCustomerReturn_CreatesDraftAtQcWarehouse()
    {
        var (ok, err, doc) = await _docs.CreateCustomerReturnAsync(50, 2, null, "RMA", 1);

        ok.Should().BeTrue(err);
        doc.Should().NotBeNull();
        doc!.DocumentType.Should().Be((byte)StockDocumentType.Return);
        doc.Status.Should().Be((byte)StockDocumentStatus.Draft);
        doc.ToWarehouseID.Should().Be(2);
        doc.OrderID.Should().Be(50);
        doc.StockDocumentLines.Should().ContainSingle(l => l.ProductVariantID == 1 && l.Quantity == 2);
        (await _context.StockDocumentHistories.CountAsync(h => h.StockDocumentID == doc.StockDocumentID)).Should().Be(1);
    }

    [Fact]
    public async Task CreateCustomerReturn_SecondOpen_Fails()
    {
        await _docs.CreateCustomerReturnAsync(50, 2, null, null, 1);
        var (ok, err, existing) = await _docs.CreateCustomerReturnAsync(50, 2, null, null, 1);

        ok.Should().BeFalse();
        err.Should().Contain("already has an open return");
        existing.Should().NotBeNull();
    }

    [Fact]
    public async Task SetDestinationWarehouse_UpdatesAndLogs()
    {
        var created = await _docs.CreateCustomerReturnAsync(50, 2, null, null, 1);
        var (ok, err) = await _docs.SetDestinationWarehouseAsync(created.Doc!.StockDocumentID, 1, 1);

        ok.Should().BeTrue(err);
        var saved = await _context.StockDocuments.AsNoTracking().FirstAsync(d => d.StockDocumentID == created.Doc.StockDocumentID);
        saved.ToWarehouseID.Should().Be(1);
    }

    [Fact]
    public async Task EnsureReturnForRefund_LinksExistingRma()
    {
        var created = await _docs.CreateCustomerReturnAsync(50, 2, null, null, 1);
        var (ok, err, doc) = await _docs.EnsureReturnForRefundAsync(50, paymentRefundId: 99, 1);

        ok.Should().BeTrue(err);
        doc!.StockDocumentID.Should().Be(created.Doc!.StockDocumentID);
        doc.PaymentRefundID.Should().Be(99);
    }

    [Fact]
    public async Task GetPaged_FiltersReturnTypeAndNumber()
    {
        await _docs.CreateCustomerReturnAsync(50, 2, null, null, 1);
        var q = new GridQuery();
        q.Search[nameof(StockDocument.DocumentNumber)] = "RET-ORD";
        var page = await _docs.GetPagedAsync(1, null, (byte)StockDocumentType.Return, q);

        page.TotalCount.Should().Be(1);
        page.Items.Should().ContainSingle(d => d.DocumentNumber.StartsWith("RET-ORD-50") || d.DocumentNumber.Contains("ORD-50"));
    }
}
