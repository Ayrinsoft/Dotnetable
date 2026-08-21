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

public class CustomerReturnServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CustomerReturnService _svc;
    private readonly Mock<IStockDocumentService> _stock = new();
    private readonly Mock<IRecordAttachmentService> _attach = new();
    private readonly Mock<IFinancialLedgerService> _ledger = new();

    public CustomerReturnServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        var factory = new TestDbContextFactory(opts);

        _attach.Setup(a => a.ListAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<RecordAttachmentDto>());
        _attach.Setup(a => a.AttachAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<int>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, (string?)null, (RecordAttachment?)null));

        _stock.Setup(s => s.CreateCustomerReturnAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<IReadOnlyList<StockDocumentLineRequest>?>(),
                It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, (string?)null, new StockDocument { StockDocumentID = 77 }));
        _stock.Setup(s => s.SetDestinationWarehouseAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, (string?)null));
        _stock.Setup(s => s.SetReturnLineConditionAsync(It.IsAny<int>(), It.IsAny<StockItemCondition>(), It.IsAny<StockHealthGrade>(),
                It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, (string?)null));
        _stock.Setup(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StockDocument { StockDocumentID = 77 });
        _ledger.Setup(l => l.PostCustomerReturnImpactAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<decimal>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<decimal>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _svc = new CustomerReturnService(factory, _stock.Object, _attach.Object, _ledger.Object);
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
            ReturnsEnabled = true, ReturnWindowDays = 14, ReturnWindowFrom = (byte)ReturnWindowFrom.ShippedAt,
        });
        _context.WebsiteClients.Add(new WebsiteClient
        {
            WebsiteClientID = 10, WebsiteID = 1, Cellphone = "09", CountryCode = "98", Email = "c@t.local",
            Givenname = "C", Surname = "U", Active = true, RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            HashKey = Guid.NewGuid(), ClientLevel = 1, Password = "x",
        });
        var order = new Order
        {
            OrderID = 80, WebsiteID = 1, WebsiteClientID = 10, OrderNumber = "ORD-80",
            Status = (byte)OrderStatus.Shipped, ShippingStatus = (byte)OrderShippingStatus.Shipped,
            CurrencyCode = "USD", ExchangeRateToUsd = 1, SubTotal = 250, GrandTotal = 250, GrandTotalUsd = 250,
            PaidAt = DateTime.UtcNow.AddDays(-5), ShippedAt = DateTime.UtcNow.AddDays(-4), CreatedAt = DateTime.UtcNow.AddDays(-6),
        };
        order.OrderItems.Add(new OrderItem
        {
            OrderItemID = 801, OrderID = 80, WebsiteID = 1, SourceWebsiteID = 1, ProductVariantID = 1,
            Quantity = 5, UnitPrice = 30, UnitPriceUsd = 30, UnitCostUsd = 10, TotalPrice = 150, TitleSnapshot = "A", SkuSnapshot = "A-1",
        });
        order.OrderItems.Add(new OrderItem
        {
            OrderItemID = 802, OrderID = 80, WebsiteID = 1, SourceWebsiteID = 1, ProductVariantID = 2,
            Quantity = 3, UnitPrice = 33.33m, UnitPriceUsd = 33.33m, UnitCostUsd = 12, TotalPrice = 100, TitleSnapshot = "B", SkuSnapshot = "B-1",
        });
        _context.Orders.Add(order);
        _context.Warehouses.Add(new Warehouse
        {
            WarehouseID = 2, WebsiteID = 1, Code = "MAIN", Name = "Main", IsDefault = true, IsActive = true,
            CreatedAt = DateTime.UtcNow,
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task Eligibility_AllowsPartialLines()
    {
        var elig = await _svc.GetEligibilityAsync(80, 10);
        elig.Eligible.Should().BeTrue();
        elig.Lines.Should().HaveCount(2);
        elig.Lines.Single(l => l.OrderItemID == 801).RemainingQty.Should().Be(5);
    }

    [Fact]
    public async Task Create_PartialQty_ThenRemainingDrops()
    {
        var (ok, err, row) = await _svc.CreateAsync(1, 10, 80, CustomerReturnReason.Damaged, null, "box crushed", "Post",
            new[]
            {
                new CustomerReturnLineInput { OrderItemID = 801, Quantity = 2, UnitRefundRequested = 25 },
            }, null, ReturnShippingPayer.Customer);
        ok.Should().BeTrue(err);
        row!.Lines.Should().ContainSingle(l => l.Quantity == 2 && l.UnitRefundRequested == 25);
        row.RequestedRefundTotal.Should().Be(50);

        var elig = await _svc.GetEligibilityAsync(80, 10);
        elig.Lines.Single(l => l.OrderItemID == 801).RemainingQty.Should().Be(3);
        elig.Lines.Single(l => l.OrderItemID == 802).RemainingQty.Should().Be(3);
    }

    [Fact]
    public async Task Approve_RequiresShippingPayer_AndCreatesWms()
    {
        var created = await _svc.CreateAsync(1, 10, 80, CustomerReturnReason.WrongItem, null, "wrong color", null,
            new[] { new CustomerReturnLineInput { OrderItemID = 802, Quantity = 1, UnitRefundRequested = 20 } }, null,
            ReturnShippingPayer.Customer);

        var bad = await _svc.ApproveAsync(created.Request!.CustomerReturnRequestID, ReturnShippingPayer.Unset, null,
            Array.Empty<CustomerReturnLineApproval>(), 1);
        bad.Success.Should().BeFalse();

        var ok = await _svc.ApproveAsync(created.Request.CustomerReturnRequestID, ReturnShippingPayer.Seller, "ok",
            new[]
            {
                new CustomerReturnLineApproval
                {
                    CustomerReturnRequestLineID = created.Request.Lines[0].CustomerReturnRequestLineID,
                    UnitRefundApproved = 18,
                }
            }, 1);
        ok.Success.Should().BeTrue(ok.Error);

        var saved = await _svc.GetByIdAsync(created.Request.CustomerReturnRequestID);
        saved!.Status.Should().Be((byte)CustomerReturnStatus.ApprovedAwaitingShipment);
        saved.ShippingPayer.Should().Be((byte)ReturnShippingPayer.Seller);
        saved.ApprovedRefundTotal.Should().Be(18);
        saved.StockDocumentID.Should().Be(77);
    }

    [Fact]
    public async Task Ship_ThenUpdateTracking()
    {
        var created = await _svc.CreateAsync(1, 10, 80, CustomerReturnReason.ChangedMind, null, "too big", "Courier",
            new[] { new CustomerReturnLineInput { OrderItemID = 801, Quantity = 1 } }, null, ReturnShippingPayer.Customer);
        await _svc.ApproveAsync(created.Request!.CustomerReturnRequestID, ReturnShippingPayer.Customer, null, Array.Empty<CustomerReturnLineApproval>(), 1);

        var ship = await _svc.SubmitShipmentAsync(created.Request.CustomerReturnRequestID, 10, "Tipax", "TRK-1");
        ship.Success.Should().BeTrue(ship.Error);

        var upd = await _svc.UpdateTrackingAsync(created.Request.CustomerReturnRequestID, 10, "TRK-2", null);
        upd.Success.Should().BeTrue(upd.Error);
        (await _svc.GetByIdAsync(created.Request.CustomerReturnRequestID))!.TrackingCode.Should().Be("TRK-2");
    }

    [Fact]
    public async Task WindowExpired_BlocksCreate()
    {
        var order = await _context.Orders.FirstAsync(o => o.OrderID == 80);
        order.ShippedAt = DateTime.UtcNow.AddDays(-40);
        await _context.SaveChangesAsync();

        var elig = await _svc.GetEligibilityAsync(80, 10);
        elig.Eligible.Should().BeFalse();
        elig.WindowExpired.Should().BeTrue();
        elig.CanAcceptAfterWindow.Should().BeTrue();
        elig.BlockReason.Should().Contain("expired");

        var blocked = await _svc.CreateAsync(1, 10, 80, CustomerReturnReason.ChangedMind, null, "late", null,
            new[] { new CustomerReturnLineInput { OrderItemID = 801, Quantity = 1 } }, null, ReturnShippingPayer.Customer);
        blocked.Success.Should().BeFalse();
    }

    [Fact]
    public async Task WindowExpired_AllowsCreateWithAcknowledgment()
    {
        var order = await _context.Orders.FirstAsync(o => o.OrderID == 80);
        order.ShippedAt = DateTime.UtcNow.AddDays(-40);
        await _context.SaveChangesAsync();

        var (ok, err, row) = await _svc.CreateAsync(1, 10, 80, CustomerReturnReason.ChangedMind, null, "late ok", null,
            new[] { new CustomerReturnLineInput { OrderItemID = 801, Quantity = 1 } }, null,
            ReturnShippingPayer.DropOffAtCenter, acceptExpiredWindow: true);
        ok.Should().BeTrue(err);
        row!.AcceptedAfterWindowExpired.Should().BeTrue();
        row.ShippingPayer.Should().Be((byte)ReturnShippingPayer.DropOffAtCenter);
    }

    [Fact]
    public async Task Receive_StoresConditionAndWarehouse_CompletePostsImpact()
    {
        var created = await _svc.CreateAsync(1, 10, 80, CustomerReturnReason.Damaged, null, "dented", null,
            new[] { new CustomerReturnLineInput { OrderItemID = 801, Quantity = 2, UnitRefundRequested = 30 } }, null,
            ReturnShippingPayer.SplitFiftyFifty);
        await _svc.ApproveAsync(created.Request!.CustomerReturnRequestID, ReturnShippingPayer.SplitFiftyFifty, null,
            Array.Empty<CustomerReturnLineApproval>(), 1, returnShippingCost: 10, receivedWarehouseId: 2);
        await _svc.SubmitShipmentAsync(created.Request.CustomerReturnRequestID, 10, "Post", "TRK-9");

        var recv = await _svc.MarkReceivedAsync(created.Request.CustomerReturnRequestID, 1, 2,
            new[]
            {
                new CustomerReturnLineReceive
                {
                    CustomerReturnRequestLineID = created.Request.Lines[0].CustomerReturnRequestLineID,
                    ReceivedCondition = (byte)StockItemCondition.Used,
                    HealthGrade = (byte)StockHealthGrade.B,
                }
            });
        recv.Success.Should().BeTrue(recv.Error);

        var afterRecv = await _svc.GetByIdAsync(created.Request.CustomerReturnRequestID);
        afterRecv!.ReceivedWarehouseID.Should().Be(2);
        afterRecv.Lines[0].ReceivedCondition.Should().Be((byte)StockItemCondition.Used);
        afterRecv.Lines[0].HealthGrade.Should().Be((byte)StockHealthGrade.B);

        var done = await _svc.MarkCompletedAsync(created.Request.CustomerReturnRequestID, 1);
        done.Success.Should().BeTrue(done.Error);

        var saved = await _svc.GetByIdAsync(created.Request.CustomerReturnRequestID);
        saved!.SiteShippingShare.Should().Be(5);
        saved.RecoveredInventoryValue.Should().Be(20);
        saved.ApprovedRefundTotal.Should().Be(60);
        saved.SiteImpactAmount.Should().Be(-45);
        saved.ImpactPostedAt.Should().NotBeNull();
        _ledger.Verify(l => l.PostCustomerReturnImpactAsync(80, saved.CustomerReturnRequestID, 5, -45, "USD",
            It.IsAny<string?>(), It.IsAny<int?>(), 0, 1, It.IsAny<CancellationToken>()), Times.Once);
    }
}
