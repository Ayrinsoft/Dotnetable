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

public class PaymentServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PaymentService _service;
    private readonly Mock<IClientWalletService> _wallet = new();
    private readonly Mock<IOrderService> _orders = new();
    private readonly Mock<IAdminNotificationService> _notifications = new();

    public PaymentServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        _service = new PaymentService(_context, _wallet.Object, _orders.Object, _notifications.Object);
        SeedBasics();
    }

    private void SeedBasics()
    {
        _context.Currencies.Add(new Currency
        {
            CurrencyCode = "USD",
            Name = "US Dollar",
            Symbol = "$",
            DecimalDigits = 2,
            IsActive = true,
        });
        _context.Websites.Add(new Website
        {
            WebsiteID = 1,
            TradeName = "Test Shop",
            BrandName = "Test Shop",
            WebsiteAddress = "test.local",
            AuthCode = Guid.NewGuid(),
            Active = true,
            Manager = "Mgr",
            Mobile = "100",
            Email = "admin@test.local",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today),
            DefaultLanguageCode = "en",
            DefaultCurrencyCode = "USD",
        });
        _context.WebsiteClients.Add(new WebsiteClient
        {
            WebsiteClientID = 100,
            WebsiteID = 1,
            Cellphone = "09120000000",
            CountryCode = "98",
            Email = "cust@test.local",
            Givenname = "Ali",
            Surname = "Customer",
            Active = true,
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            HashKey = Guid.NewGuid(),
            ClientLevel = 1,
            Password = "x",
        });
        _context.Orders.Add(new Order
        {
            OrderID = 500,
            WebsiteID = 1,
            WebsiteClientID = 100,
            OrderNumber = "ORD-PAY-1",
            Status = (byte)OrderStatus.PendingPayment,
            CurrencyCode = "USD",
            ExchangeRateToUsd = 1,
            SubTotal = 100,
            DiscountTotal = 0,
            ShippingTotal = 0,
            TaxTotal = 0,
            GrandTotal = 100,
            GrandTotalUsd = 100,
            CreatedAt = DateTime.UtcNow,
        });
        _context.SaveChanges();

        _orders
            .Setup(o => o.TransitionStatusAsync(It.IsAny<int>(), It.IsAny<OrderStatus>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _notifications
            .Setup(n => n.NotifySiteAdminsAsync(
                It.IsAny<int>(), It.IsAny<AdminNotificationType>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task RecordReceivedPayment_Cod_CreatesPaidPayment_AndTransitionsOrder()
    {
        var (success, error, payment) = await _service.RecordReceivedPaymentAsync(
            500, PaymentMethod.CashOnDelivery, null, "COD-1", "Collected by courier", 10);

        success.Should().BeTrue(error);
        payment.Should().NotBeNull();
        payment!.Method.Should().Be((byte)PaymentMethod.CashOnDelivery);
        payment.Status.Should().Be((byte)PaymentStatus.Paid);
        payment.Amount.Should().Be(100);
        payment.TrackingCode.Should().Be("COD-1");
        payment.CreatedByMemberID.Should().Be(10);
        payment.VerifiedByMemberID.Should().Be(10);

        _orders.Verify(o => o.TransitionStatusAsync(
            500, OrderStatus.Paid, 10, It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordReceivedPayment_ManualPartial_UsesCustomAmount()
    {
        var (success, _, payment) = await _service.RecordReceivedPaymentAsync(
            500, PaymentMethod.Manual, 40m, null, "POS terminal", 10);

        success.Should().BeTrue();
        payment!.Amount.Should().Be(40);
        payment.Method.Should().Be((byte)PaymentMethod.Manual);
    }

    [Fact]
    public async Task RecordReceivedPayment_RejectsSecondPaidPayment()
    {
        await _service.RecordReceivedPaymentAsync(500, PaymentMethod.Manual, null, null, null, 10);

        var (success, error, _) = await _service.RecordReceivedPaymentAsync(
            500, PaymentMethod.CashOnDelivery, null, null, null, 10);

        success.Should().BeFalse();
        error.Should().Contain("already has a paid payment");
    }

    [Fact]
    public async Task RecordReceivedPayment_RejectsWalletMethod()
    {
        var (success, error, _) = await _service.RecordReceivedPaymentAsync(
            500, PaymentMethod.Wallet, null, null, null, 10);

        success.Should().BeFalse();
        error.Should().Contain("Manual or CashOnDelivery");
    }

    [Fact]
    public async Task Refund_CashManual_CompletesWithoutWalletOrBank()
    {
        await _service.RecordReceivedPaymentAsync(500, PaymentMethod.CashOnDelivery, null, null, null, 10);
        var paid = await _context.Payments.SingleAsync();

        var (success, error, refund) = await _service.RefundAsync(
            paid.PaymentID, 100m, "Returned COD goods", toWallet: false, bankAccountId: null, memberId: 10);

        success.Should().BeTrue(error);
        refund.Should().NotBeNull();
        refund!.Status.Should().Be((byte)PaymentRefundStatus.Completed);
        refund.BankAccountID.Should().BeNull();
        refund.ClientWalletTransactionID.Should().BeNull();
        refund.RefundedAt.Should().NotBeNull();

        paid = await _context.Payments.SingleAsync();
        paid.Status.Should().Be((byte)PaymentStatus.Refunded);

        _orders.Verify(o => o.TransitionStatusAsync(
            500, OrderStatus.Refunded, 10, "Returned COD goods", It.IsAny<CancellationToken>()), Times.Once);
        _wallet.Verify(w => w.ApplyAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<byte>(), It.IsAny<decimal>(),
            It.IsAny<byte?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Refund_Wallet_CreditsCustomer()
    {
        await _service.RecordReceivedPaymentAsync(500, PaymentMethod.Manual, null, null, null, 10);
        var paid = await _context.Payments.SingleAsync();

        _wallet.Setup(w => w.ApplyAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<byte>(), It.IsAny<decimal>(),
                It.IsAny<byte?>(), It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClientWalletTransaction
            {
                ClientWalletTransactionID = 9,
                WebsiteID = 1,
                ClientWalletID = 1,
                Type = (byte)ClientWalletTransactionType.RefundCredit,
                Amount = 100,
                BalanceAfter = 100,
                CreatedAt = DateTime.UtcNow,
            });

        var (success, _, refund) = await _service.RefundAsync(
            paid.PaymentID, 100m, "Goodwill", toWallet: true, bankAccountId: null, memberId: 10);

        success.Should().BeTrue();
        refund!.Status.Should().Be((byte)PaymentRefundStatus.Completed);
        refund.ClientWalletTransactionID.Should().Be(9);
    }

    public void Dispose() => _context.Dispose();
}
