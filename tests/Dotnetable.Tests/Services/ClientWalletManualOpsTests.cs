using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Dotnetable.Tests.Services;

public class ClientWalletManualOpsTests : IDisposable
{
    private readonly RelationalTestDb _db;
    private readonly AppDbContext _context;
    private readonly ClientWalletService _wallets;
    private readonly ClientWalletWithdrawalService _withdrawals;
    private readonly PaymentService _payments;

    public ClientWalletManualOpsTests()
    {
        // Relational: the wallet balance now moves with a conditional UPDATE, which the InMemory
        // provider cannot execute at all. See RelationalTestDb.
        _db = new RelationalTestDb();
        _context = _db.NewContext();
        var currency = new CurrencyConversionService(_context);
        _wallets = new ClientWalletService(_context, currency);
        _withdrawals = new ClientWalletWithdrawalService(
            _context, _wallets, new Mock<IAdminNotificationService>().Object, currency);
        _payments = new PaymentService(
            _context, _wallets, new Mock<IOrderService>().Object,
            new Mock<IAdminNotificationService>().Object,
            new Mock<IFinancialLedgerService>().Object,
            new Mock<IStockDocumentService>().Object);
        Seed();
    }

    private void Seed()
    {
        _context.Currencies.Add(new Currency
        {
            CurrencyCode = "IRR",
            Name = "Rial",
            Symbol = "﷼",
            DecimalDigits = 0,
            IsActive = true,
        });
        _context.Websites.Add(new Website
        {
            WebsiteID = 1,
            TradeName = "Shop",
            BrandName = "Shop",
            WebsiteAddress = "shop.local",
            AuthCode = Guid.NewGuid(),
            Active = true,
            Manager = "Mgr",
            Mobile = "100",
            Email = "admin@shop.local",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today),
            DefaultLanguageCode = "fa",
            DefaultCurrencyCode = "IRR",
        });
        _context.WebsiteClients.Add(new WebsiteClient
        {
            WebsiteClientID = 100,
            WebsiteID = 1,
            Cellphone = "09120000000",
            CountryCode = "98",
            Email = "cust@shop.local",
            Givenname = "Ali",
            Surname = "Customer",
            Active = true,
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            HashKey = Guid.NewGuid(),
            ClientLevel = 1,
            Password = "x",
        });
        _context.Members.Add(new Member
        {
            MemberID = 10,
            WebsiteID = 1,
            Username = "admin",
            Password = "x",
            Email = "admin@shop.local",
            CellphoneNumber = "09121111111",
            CountryCode = "98",
            Givenname = "Admin",
            Surname = "User",
            Active = true,
            RegisterDate = DateOnly.FromDateTime(DateTime.Today),
            HashKey = Guid.NewGuid(),
            PolicyID = 1,
        });
        _context.ClientBankAccounts.Add(new ClientBankAccount
        {
            ClientBankAccountID = 7,
            WebsiteID = 1,
            WebsiteClientID = 100,
            OwnerName = "Ali Customer",
            IBAN = "IR000000000000000000000001",
            IsActive = true,
            IsDefault = true,
            CreatedAt = DateTime.UtcNow,
        });
        _context.BankAccounts.Add(new BankAccount
        {
            BankAccountID = 3,
            BankID = 1,
            WebsiteID = 1,
            Title = "Shop IRR",
            AccountNumber = "123",
            IsActive = true,
            CreatedByMemberId = 10,
        });
        _context.WebsiteWalletCurrencies.Add(new WebsiteWalletCurrency
        {
            WebsiteID = 1,
            CurrencyCode = "IRR",
            IsDefault = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        });
        _context.ClientWallets.Add(new ClientWallet
        {
            ClientWalletID = 1,
            WebsiteID = 1,
            WebsiteClientID = 100,
            CurrencyCode = "IRR",
            Balance = 0,
            BalanceUsd = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 },
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task RecordWalletDeposit_CreditsWallet_AndMarksAdminPayment()
    {
        var (ok, err, payment) = await _payments.RecordWalletDepositAsync(
            1, 100, 250_000, "IRR", 3, "Card-to-card from customer", "TRX-1", null, 10);

        ok.Should().BeTrue(err);
        payment.Should().NotBeNull();
        payment!.OrderID.Should().BeNull();
        payment.CreatedByMemberID.Should().Be(10);
        payment.Status.Should().Be((byte)PaymentStatus.Paid);
        payment.ClientWalletTransactionID.Should().NotBeNull();
        payment.GatewayRefNumber.Should().Be("Card-to-card from customer");

        var balance = await _wallets.GetBalanceAsync(1, 100, "IRR");
        balance.Should().Be(250_000);

        var history = await _wallets.GetHistoryAsync(1, 100, new GridQuery { PageSize = 10 }, "IRR");
        history.Items.Should().ContainSingle(t =>
            t.Type == (byte)ClientWalletTransactionType.AdminDeposit
            && t.Amount == 250_000
            && t.CreatedByMemberID == 10);
    }

    [Fact]
    public async Task RecordWalletDeposit_RequiresDescription()
    {
        var (ok, err, _) = await _payments.RecordWalletDepositAsync(
            1, 100, 10, "IRR", null, "  ", null, null, 10);
        ok.Should().BeFalse();
        err.Should().Contain("description");
    }

    [Fact]
    public async Task RecordAdminPayout_DebitsWallet_AndLabelsAdminCreated()
    {
        await _wallets.ApplyAsync(1, 100, (byte)ClientWalletTransactionType.AdminAdjustment, 500_000,
            (byte)ClientWalletSourceType.AdminManual, null, "seed", 10, "IRR");

        var (ok, err, w) = await _withdrawals.RecordAdminPayoutAsync(
            1, 100, 7, 100_000, "IRR", "Paid to customer IBAN", "REF-9", 10);

        ok.Should().BeTrue(err);
        w.Should().NotBeNull();
        w!.Status.Should().Be((byte)ClientWalletWithdrawalStatus.Paid);
        w.CreatedByMemberID.Should().Be(10);
        w.Note.Should().Be("Paid to customer IBAN");
        w.PaymentRefNumber.Should().Be("REF-9");

        (await _wallets.GetBalanceAsync(1, 100, "IRR")).Should().Be(400_000);
    }

    [Fact]
    public async Task RecordAdminPayout_RejectsInsufficientBalance()
    {
        var (ok, err, _) = await _withdrawals.RecordAdminPayoutAsync(
            1, 100, 7, 10, "IRR", "No money", null, 10);
        ok.Should().BeFalse();
        err.Should().Contain("Insufficient");
    }

    [Fact]
    public async Task GetPagedWallets_ReturnsClientBalance()
    {
        await _wallets.ApplyAsync(1, 100, (byte)ClientWalletTransactionType.AdminDeposit, 12,
            (byte)ClientWalletSourceType.AdminManual, null, "x", 10, "IRR");

        var page = await _wallets.GetPagedAsync(1, new GridQuery { PageSize = 20 });
        page.Items.Should().Contain(w => w.WebsiteClientID == 100 && w.Balance == 12);
    }

    public void Dispose()
    {
        _context.Dispose();
        _db.Dispose();
    }
}
