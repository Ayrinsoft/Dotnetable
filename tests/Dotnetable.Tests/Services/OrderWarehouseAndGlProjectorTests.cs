using Dotnetable.Application.DTOs;
using Dotnetable.Application.Financial;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Dotnetable.Tests.Services;

/// <summary>
/// Order → warehouse path (WMS outbound on pay, post on ship) and GL projector COGS timing.
/// </summary>
public class OrderWarehouseAndGlProjectorTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TestDbContextFactory _factory;
    private readonly Mock<IWarehouseService> _warehouses = new();
    private readonly Mock<IInventoryService> _inventory = new();
    private readonly Mock<IVendorProductService> _vendorProducts = new();
    private readonly Mock<IAdminNotificationService> _notifications = new();
    private readonly Mock<IChartOfAccountService> _coa = new();
    private readonly Mock<IJournalService> _journals = new();
    private readonly List<JournalLineDto> _lastJournalLines = new();
    private readonly GlProjector _projector;
    private readonly FinancialLedgerService _ledger;
    private readonly StockDocumentService _stockDocs;

    public OrderWarehouseAndGlProjectorTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        // Services open a context per call now, so they get a _factory over the same options;
        // the fixture keeps its own _context for seeding and asserting.
        _factory = new TestDbContextFactory(opts);

        _coa.Setup(c => c.EnsureSeededAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(async (int websiteId, CancellationToken ct) =>
            {
                if (await _context.ChartOfAccounts.AnyAsync(a => a.WebsiteID == websiteId, ct)) return;
                void Add(string code, string name, GlAccountType type)
                {
                    _context.ChartOfAccounts.Add(new ChartOfAccount
                    {
                        WebsiteID = websiteId,
                        Code = code,
                        Name = name,
                        AccountType = (byte)type,
                        IsActive = true,
                        IsSystem = true,
                        SortOrder = 0,
                    });
                }
                Add("1100", "Cash", GlAccountType.Asset);
                Add("1200", "Bank", GlAccountType.Asset);
                Add("1400", "Inventory", GlAccountType.Asset);
                Add("2100", "AP", GlAccountType.Liability);
                Add("2200", "Tax payable", GlAccountType.Liability);
                Add("4100", "Sales", GlAccountType.Income);
                Add("4200", "Shipping income", GlAccountType.Income);
                Add("4300", "Markup", GlAccountType.Income);
                Add("4400", "Other income", GlAccountType.Income);
                Add("5100", "COGS", GlAccountType.Expense);
                Add("5400", "Refunds", GlAccountType.Expense);
                Add("5500", "Vendor exp", GlAccountType.Expense);
                await _context.SaveChangesAsync(ct);
            });

        _journals.Setup(j => j.CreateDraftAsync(
                It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<string?>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<IReadOnlyList<JournalLineDto>>(), It.IsAny<int?>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int websiteId, DateOnly date, string? desc, string currency, bool tax,
                IReadOnlyList<JournalLineDto> lines, int? memberId, string? sourceType, string? sourceKey, CancellationToken _) =>
            {
                _lastJournalLines.Clear();
                _lastJournalLines.AddRange(lines);
                var entry = new JournalEntry
                {
                    WebsiteID = websiteId,
                    EntryNumber = "J-TEST",
                    EntryDate = date,
                    Description = desc,
                    CurrencyCode = currency,
                    ReportToTax = tax,
                    SourceType = sourceType,
                    SourceKey = sourceKey,
                    IsPosted = false,
                    CreatedAt = DateTime.UtcNow,
                };
                _context.JournalEntries.Add(entry);
                _context.SaveChanges();
                return (true, (string?)null, entry);
            });

        _journals.Setup(j => j.PostAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, (string?)null));

        _projector = new GlProjector(_factory, _coa.Object, _journals.Object, NullLogger<GlProjector>.Instance);
        _ledger = new FinancialLedgerService(_factory, _projector);

        _warehouses.Setup(w => w.EnsureDefaultAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _warehouses.Setup(w => w.GetDefaultWarehouseIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _warehouses.Setup(w => w.GetAvailableAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);
        _warehouses.Setup(w => w.ReserveAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _warehouses.Setup(w => w.ReleaseReservationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _warehouses.Setup(w => w.SumOnHandForVariantAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);

        _inventory.Setup(i => i.AdjustAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _inventory.Setup(i => i.DecrementOnFulfillAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _inventory.Setup(i => i.ReleaseReservationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _inventory.Setup(i => i.RestockReturnAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _vendorProducts.Setup(v => v.CommitSaleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _vendorProducts.Setup(v => v.ReleaseReservationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _vendorProducts.Setup(v => v.RestockAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _vendorProducts.Setup(v => v.SyncInventoryOnHandFromListingsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _notifications.Setup(n => n.NotifyRoleAsync(
                It.IsAny<int>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<AdminNotificationType>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _stockDocs = new StockDocumentService(
            _factory, _warehouses.Object, _inventory.Object, _vendorProducts.Object,
            _notifications.Object, _ledger);

        SeedCatalog();
    }

    public void Dispose() => _context.Dispose();

    private void SeedCatalog()
    {
        _context.Currencies.Add(new Currency
        {
            CurrencyCode = "USD", Name = "US Dollar", Symbol = "$", DecimalDigits = 2, IsActive = true,
        });
        _context.Websites.Add(new Website
        {
            WebsiteID = 1, TradeName = "Shop", BrandName = "Shop", WebsiteAddress = "shop.local",
            AuthCode = Guid.NewGuid(), Active = true, Manager = "M", Mobile = "1", Email = "a@b.c",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today), DefaultLanguageCode = "en", DefaultCurrencyCode = "USD",
        });
        _context.Warehouses.Add(new Warehouse
        {
            WarehouseID = 1, WebsiteID = 1, Code = "MAIN", Name = "Main", IsDefault = true, IsActive = true,
            CreatedAt = DateTime.UtcNow,
        });
        _context.WarehouseStocks.Add(new WarehouseStock
        {
            WarehouseID = 1, ProductVariantID = 1, QuantityOnHand = 100, QuantityReserved = 2,
            RowVersion = Array.Empty<byte>(),
        });
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
        _context.Vendors.Add(new Vendor
        {
            VendorID = 1, WebsiteID = 1, Name = "Store", Slug = "store",
            IsActive = true, VendorType = (byte)VendorType.Display,
        });
        _context.VendorProducts.Add(new VendorProduct
        {
            VendorProductID = 1, WebsiteID = 1, VendorID = 1, ProductVariantID = 1,
            ReferencePrice = 100, ReferencePriceUsd = 100, StockQuantity = 50, IsActive = true,
        });
        _context.SaveChanges();
    }

    private Order CreatePaidOrder(int orderId = 100)
    {
        var order = new Order
        {
            OrderID = orderId,
            WebsiteID = 1,
            WebsiteClientID = 10,
            OrderNumber = $"ORD-{orderId}",
            Status = (byte)OrderStatus.Paid,
            CurrencyCode = "USD",
            ExchangeRateToUsd = 1,
            SubTotal = 100,
            GrandTotal = 100,
            GrandTotalUsd = 100,
            PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            ReportToTax = true,
        };
        order.OrderItems.Add(new OrderItem
        {
            OrderItemID = orderId * 10,
            OrderID = orderId,
            ProductVariantID = 1,
            VendorProductID = 1,
            VendorID = 1,
            Quantity = 2,
            UnitPrice = 50,
            UnitPriceUsd = 50,
            UnitCostUsd = 20,
            CatalogUnitPrice = 50,
            TotalPrice = 100,
            TitleSnapshot = "Widget",
            SkuSnapshot = "SKU-1",
        });
        _context.Orders.Add(order);
        _context.Payments.Add(new Payment
        {
            PaymentID = orderId,
            WebsiteID = 1,
            OrderID = orderId,
            WebsiteClientID = 10,
            Method = (byte)PaymentMethod.Manual,
            Amount = 100,
            AmountUsd = 100,
            CurrencyCode = "USD",
            Status = (byte)PaymentStatus.Paid,
            PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
        });
        _context.SaveChanges();
        return order;
    }

    [Fact]
    public async Task EnsureOutbound_ThenPost_CreatesPostedOutboundAndInventoryCogs()
    {
        CreatePaidOrder(200);
        var (ok, err, doc) = await _stockDocs.EnsureOutboundForOrderAsync(200, memberId: 1);
        ok.Should().BeTrue(err);
        doc.Should().NotBeNull();
        doc!.DocumentType.Should().Be((byte)StockDocumentType.Outbound);
        doc.Status.Should().Be((byte)StockDocumentStatus.Submitted);

        var post = await _stockDocs.PostOutboundForOrderAsync(200, memberId: 1);
        post.Success.Should().BeTrue(post.Error);

        var posted = await _context.StockDocuments.SingleAsync(d => d.OrderID == 200 && d.DocumentType == (byte)StockDocumentType.Outbound);
        posted.Status.Should().Be((byte)StockDocumentStatus.Posted);

        var cogs = await _context.FinancialLedgerEntries
            .Where(e => e.OrderID == 200 && e.TransactionType == FinancialTransactionTypes.InventoryCogs && e.IsCurrent)
            .ToListAsync();
        cogs.Should().NotBeEmpty();
        cogs.Sum(e => e.Amount).Should().Be(40); // 2 × 20

        // Re-post is idempotent
        var again = await _stockDocs.PostOutboundForOrderAsync(200, memberId: 1);
        again.Success.Should().BeTrue();
        (await _context.FinancialLedgerEntries.CountAsync(e =>
            e.OrderID == 200 && e.TransactionType == FinancialTransactionTypes.InventoryCogs && e.IsCurrent))
            .Should().Be(cogs.Count);
    }

    [Fact]
    public async Task EnsureReturnForRefund_WhenOutboundPosted_CreatesReturnDoc()
    {
        CreatePaidOrder(300);
        await _stockDocs.EnsureOutboundForOrderAsync(300, 1);
        await _stockDocs.PostOutboundForOrderAsync(300, 1);

        _context.PaymentRefunds.Add(new PaymentRefund
        {
            PaymentRefundID = 1,
            PaymentID = 300,
            Amount = 100,
            Status = (byte)PaymentRefundStatus.Completed,
            CreatedByMemberID = 1,
            CreatedAt = DateTime.UtcNow,
            RefundedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();

        var (ok, err, ret) = await _stockDocs.EnsureReturnForRefundAsync(300, 1, memberId: 1);
        ok.Should().BeTrue(err);
        ret.Should().NotBeNull();
        ret!.DocumentType.Should().Be((byte)StockDocumentType.Return);
        ret.PaymentRefundID.Should().Be(1);
    }

    [Fact]
    public async Task GlProjector_PaymentGroup_DoesNotPostCogs_FromOrderLineCost()
    {
        CreatePaidOrder(400);
        var group = Guid.NewGuid();
        _context.FinancialLedgerEntries.Add(new FinancialLedgerEntry
        {
            WebsiteID = 1,
            TransactionType = FinancialTransactionTypes.CustomerPayment,
            Flow = FinancialFlow.In,
            Amount = 100,
            AmountUsd = 100,
            CurrencyCode = "USD",
            OccurredDate = DateOnly.FromDateTime(DateTime.Today),
            OccurredTime = TimeOnly.FromDateTime(DateTime.Now),
            OccurredAtUtc = DateTime.UtcNow,
            Title = "Pay",
            ReportToTax = true,
            EventGroupId = group,
            IsCurrent = true,
            Version = 1,
            OrderID = 400,
            CreatedAt = DateTime.UtcNow,
        });
        _context.FinancialLedgerEntries.Add(new FinancialLedgerEntry
        {
            WebsiteID = 1,
            TransactionType = FinancialTransactionTypes.OrderLineCost,
            Flow = FinancialFlow.Component,
            Amount = 40,
            AmountUsd = 40,
            CurrencyCode = "USD",
            OccurredDate = DateOnly.FromDateTime(DateTime.Today),
            OccurredTime = TimeOnly.FromDateTime(DateTime.Now),
            OccurredAtUtc = DateTime.UtcNow,
            Title = "Cost memo",
            ReportToTax = true,
            EventGroupId = group,
            IsCurrent = true,
            Version = 1,
            OrderID = 400,
            CreatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();

        await _projector.ProjectEventGroupAsync(1, group);

        _lastJournalLines.Should().NotBeEmpty();
        var cogsAcc = await _context.ChartOfAccounts.SingleAsync(a => a.WebsiteID == 1 && a.Code == "5100");
        _lastJournalLines.Any(l => l.ChartOfAccountID == cogsAcc.ChartOfAccountID && l.Debit > 0)
            .Should().BeFalse("analytical OrderLineCost must not drive GL COGS at payment");
    }

    [Fact]
    public async Task GlProjector_InventoryCogsGroup_PostsDrCogsCrInventory()
    {
        CreatePaidOrder(500);
        var group = Guid.NewGuid();
        _context.FinancialLedgerEntries.Add(new FinancialLedgerEntry
        {
            WebsiteID = 1,
            TransactionType = FinancialTransactionTypes.InventoryCogs,
            Flow = FinancialFlow.Component,
            Amount = 40,
            AmountUsd = 40,
            CurrencyCode = "USD",
            OccurredDate = DateOnly.FromDateTime(DateTime.Today),
            OccurredTime = TimeOnly.FromDateTime(DateTime.Now),
            OccurredAtUtc = DateTime.UtcNow,
            Title = "COGS stock-out",
            ReportToTax = true,
            EventGroupId = group,
            IsCurrent = true,
            Version = 1,
            OrderID = 500,
            MetaJson = "{\"sourceKey\":\"STOCK-COGS:99\"}",
            CreatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();

        await _projector.ProjectEventGroupAsync(1, group);

        var cogsAcc = await _context.ChartOfAccounts.SingleAsync(a => a.WebsiteID == 1 && a.Code == "5100");
        var invAcc = await _context.ChartOfAccounts.SingleAsync(a => a.WebsiteID == 1 && a.Code == "1400");
        _lastJournalLines.Should().Contain(l => l.ChartOfAccountID == cogsAcc.ChartOfAccountID && l.Debit == 40);
        _lastJournalLines.Should().Contain(l => l.ChartOfAccountID == invAcc.ChartOfAccountID && l.Credit == 40);
        _lastJournalLines.Sum(l => l.Debit).Should().Be(_lastJournalLines.Sum(l => l.Credit));
    }

    [Fact]
    public async Task OrderService_Cancel_Blocked_WhenPaymentCaptured()
    {
        CreatePaidOrder(600);
        var orders = BuildOrderService();
        var ok = await orders.TransitionStatusAsync(600, OrderStatus.Cancelled, memberId: 1, note: "try cancel");
        ok.Should().BeFalse();
        var status = await _context.Orders.Where(o => o.OrderID == 600).Select(o => o.Status).SingleAsync();
        status.Should().Be((byte)OrderStatus.Paid);
    }

    [Fact]
    public async Task OrderService_Cancel_Allowed_WhenPendingPayment()
    {
        var order = CreatePaidOrder(700);
        order.Status = (byte)OrderStatus.PendingPayment;
        order.PaidAt = null;
        var pay = await _context.Payments.SingleAsync(p => p.OrderID == 700);
        pay.Status = (byte)PaymentStatus.Pending;
        await _context.SaveChangesAsync();

        var orders = BuildOrderService();
        var ok = await orders.TransitionStatusAsync(700, OrderStatus.Cancelled, memberId: 1, note: "customer abandoned");
        ok.Should().BeTrue();
        (await _context.Orders.Where(o => o.OrderID == 700).Select(o => o.Status).SingleAsync())
            .Should().Be((byte)OrderStatus.Cancelled);
    }

    private OrderService BuildOrderService()
    {
        var shipping = new Mock<IShippingService>();
        var tax = new Mock<ITaxService>();
        var coupons = new Mock<ICouponService>();
        var currency = new Mock<ICurrencyConversionService>();
        var cart = new Mock<ICartService>();
        var credit = new Mock<IVendorCreditService>();
        var digital = new Mock<IDigitalDeliveryService>();
        digital.Setup(d => d.GrantForOrderAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var email = new Mock<IEmailService>();
        var sms = new Mock<ISmsSender>();
        var wa = new Mock<IWhatsAppSender>();

        return new OrderService(
            _factory, _inventory.Object, _vendorProducts.Object, shipping.Object, tax.Object, coupons.Object,
            currency.Object, cart.Object, _notifications.Object, credit.Object, digital.Object,
            email.Object, sms.Object, wa.Object, _ledger, _stockDocs, _warehouses.Object,
            NullLogger<OrderService>.Instance);
    }
}
