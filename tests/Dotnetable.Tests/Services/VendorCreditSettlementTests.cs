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

public class VendorCreditSettlementTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly VendorCreditService _service;

    public VendorCreditSettlementTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _context = new AppDbContext(opts);

        var tax = new Mock<ITaxService>();
        tax.Setup(t => t.ComputeTaxDetailedAsync(
                It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int _, int? _, int? _, decimal net, decimal _, CancellationToken _) =>
                new TaxComputationResult { TaxAmount = 0, PricesIncludeTax = true, TaxEnabled = false });

        var vendors = new Mock<IVendorService>();
        var suppliers = new Mock<ISupplierService>();
        var currency = new Mock<ICurrencyConversionService>();
        currency.Setup(c => c.ConvertViaUsdAsync(
                It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int _, decimal amt, string from, string to, decimal? usd, CancellationToken _) =>
                new FxViaUsdQuote
                {
                    FromCurrency = from,
                    ToCurrency = to,
                    FromAmount = amt,
                    ToAmount = amt,
                    UsdAmount = usd ?? 0,
                    RateFromPerUsd = 90_000,
                    RateToPerUsd = 90_000,
                });
        var ledger = new Mock<IFinancialLedgerService>();
        ledger.Setup(l => l.PostVendorSettlementAsync(
                It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(),
                It.IsAny<bool>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _service = new VendorCreditService(_context, vendors.Object, currency.Object, suppliers.Object, tax.Object, ledger.Object);
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
        _context.Vendors.Add(new Vendor
        {
            VendorID = 7,
            WebsiteID = 1,
            Name = "Seller",
            Slug = "seller",
            IsActive = true,
            VendorType = (byte)VendorType.Member,
            SettlementMode = 0,
        });
        _context.WebsiteClients.Add(new WebsiteClient
        {
            WebsiteClientID = 100,
            WebsiteID = 1,
            Email = "c@shop.local",
            Active = true,
            RegisterDate = DateOnly.FromDateTime(DateTime.Today),
            HashKey = Guid.NewGuid(),
            ClientLevel = 1,
            Password = "x",
        });
        _context.Orders.Add(new Order
        {
            OrderID = 50,
            WebsiteID = 1,
            WebsiteClientID = 100,
            OrderNumber = "ORD-IRR-1",
            Status = (byte)OrderStatus.Paid,
            CurrencyCode = "IRR",
            ExchangeRateToUsd = 90_000,
            SubTotal = 1_800_000,
            DiscountTotal = 0,
            ShippingTotal = 0,
            TaxTotal = 0,
            GrandTotal = 1_800_000,
            GrandTotalUsd = 20,
            CreatedAt = DateTime.UtcNow,
            PaidAt = DateTime.UtcNow,
        });
        _context.OrderItems.Add(new OrderItem
        {
            OrderItemID = 501,
            OrderID = 50,
            WebsiteID = 1,
            SourceWebsiteID = 1,
            VendorID = 7,
            VendorProductID = 1,
            TitleSnapshot = "Phone",
            SkuSnapshot = "PH-1",
            Quantity = 1,
            UnitPrice = 1_800_000,
            CatalogUnitPrice = 1_800_000,
            UnitPriceUsd = 20,
            UnitCostUsd = 12,
            DiscountAmount = 0,
            TotalPrice = 1_800_000,
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task SettleHostOrder_UsesOrderCurrency_NotUsd()
    {
        await _service.SettleHostOrderAsync(50);

        var settlement = await _context.Settlements.Include(s => s.SettlementItems).SingleAsync();
        settlement.CurrencyCode.Should().Be("IRR");
        settlement.SourceCurrencyCode.Should().Be("IRR");
        settlement.NetAmount.Should().Be(1_800_000);
        settlement.SourceTotalAmount.Should().Be(1_800_000);
        settlement.TotalAmount.Should().Be(1_800_000);
        settlement.SettlementItems.Should().ContainSingle(i => i.Amount == 1_800_000);
    }

    [Fact]
    public async Task SettleHostOrder_ConvertsViaUsd_WhenVendorCurrencyDiffers()
    {
        var vendor = await _context.Vendors.FirstAsync();
        vendor.SettlementCurrencyCode = "KRW";
        await _context.SaveChangesAsync();

        // Rebuild service with a conversion mock that actually converts.
        var tax = new Mock<ITaxService>();
        tax.Setup(t => t.ComputeTaxDetailedAsync(
                It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaxComputationResult { TaxAmount = 0, PricesIncludeTax = true });
        var currency = new Mock<ICurrencyConversionService>();
        currency.Setup(c => c.ConvertViaUsdAsync(
                It.IsAny<int>(), It.IsAny<decimal>(), "IRR", "KRW",
                It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int _, decimal amt, string from, string to, decimal? usd, CancellationToken _) =>
            {
                var usdAmt = usd is > 0 ? usd.Value : amt / 90_000m;
                return new FxViaUsdQuote
                {
                    FromCurrency = from,
                    ToCurrency = to,
                    FromAmount = amt,
                    UsdAmount = usdAmt,
                    ToAmount = Math.Round(usdAmt * 1350m, 0),
                    RateFromPerUsd = 90_000,
                    RateToPerUsd = 1350,
                };
            });
        var service = new VendorCreditService(
            _context, new Mock<IVendorService>().Object, currency.Object,
            new Mock<ISupplierService>().Object, tax.Object, new Mock<IFinancialLedgerService>().Object);

        await service.SettleHostOrderAsync(50);

        var settlement = await _context.Settlements.SingleAsync();
        settlement.CurrencyCode.Should().Be("KRW");
        settlement.SourceCurrencyCode.Should().Be("IRR");
        settlement.SourceTotalAmount.Should().Be(1_800_000);
        settlement.BridgeUsdAmount.Should().Be(20);
        settlement.TotalAmount.Should().Be(27_000);
        settlement.ExchangeRateToUsd.Should().Be(90_000);
        settlement.ExchangeRateUsdToSettle.Should().Be(1350);
    }

    public void Dispose() => _context.Dispose();
}
