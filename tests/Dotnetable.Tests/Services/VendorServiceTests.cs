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

public class VendorServiceTests : IDisposable
{
    private readonly RelationalTestDb _db;
    private readonly AppDbContext _context;
    private readonly VendorService _vendors;
    private readonly VendorProductService _listings;
    private readonly VendorCreditService _credit;
    private readonly Website _host;
    private readonly Website _source;

    public VendorServiceTests()
    {
        // Relational: stock reservation now moves with a conditional UPDATE, which the InMemory
        // provider cannot execute at all. See RelationalTestDb.
        _db = new RelationalTestDb();
        _context = _db.NewContext();
        // Services open a context per call now; the factory points at the same database so the
        // fixture can still seed and assert through its own _context.
        var factory = new TestDbContextFactory(_db.Options);

        var fx = new CurrencyConversionService(factory);
        _vendors = new VendorService(factory, fx);
        _listings = new VendorProductService(factory);
        var currency = new CurrencyConversionService(factory);
        var suppliers = new SupplierService(factory);
        var tax = new TaxService(factory);
        var ledger = new Mock<IFinancialLedgerService>();
        _credit = new VendorCreditService(factory, _vendors, currency, suppliers, tax, ledger.Object);

        _host = NewWebsite("Host", "host.test");
        _source = NewWebsite("Source", "source.test");
        _context.Websites.AddRange(_host, _source);
        _context.SaveChanges();
    }

    private static Website NewWebsite(string name, string addr) => new()
    {
        TradeName = name, WebsiteAddress = addr, AuthCode = Guid.NewGuid(),
        Active = true, Manager = "M", Mobile = "1", Email = $"{name}@t.com",
        RegisterDate = DateOnly.FromDateTime(DateTime.Today),
        DefaultLanguageCode = "en", DefaultCurrencyCode = "USD", BrandName = name,
    };

    public void Dispose()
    {
        _context.Dispose();
        _db.Dispose();
    }

    [Fact]
    public async Task Create_DisplayVendor_Works()
    {
        var v = await _vendors.CreateAsync(new Vendor
        {
            WebsiteID = _host.WebsiteID, Name = "Shop A", Slug = "shop-a",
            IsActive = true, VendorType = (byte)VendorType.Display,
        });
        v.VendorID.Should().BeGreaterThan(0);
        v.MemberID.Should().BeNull();
        v.LinkedWebsiteID.Should().BeNull();
    }

    [Fact]
    public async Task Create_MemberVendor_RequiresMemberOnSameSite()
    {
        var policy = new Policy { Title = "P", Active = true, WebsiteID = _host.WebsiteID };
        _context.Policies.Add(policy);
        await _context.SaveChangesAsync();
        var member = new Member
        {
            Username = "vendor1", Active = true, Password = "x", Email = "v@t.com",
            CellphoneNumber = "1", CountryCode = "US",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today),
            Givenname = "V", Surname = "One", HashKey = Guid.NewGuid(),
            PolicyID = policy.PolicyID, WebsiteID = _host.WebsiteID,
        };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var v = await _vendors.CreateAsync(new Vendor
        {
            WebsiteID = _host.WebsiteID, Name = "Member Vendor",
            IsActive = true, VendorType = (byte)VendorType.Member, MemberID = member.MemberID,
        });
        v.MemberID.Should().Be(member.MemberID);

        var loaded = await _vendors.GetByMemberIdAsync(member.MemberID);
        loaded.Should().NotBeNull();
        loaded!.VendorID.Should().Be(v.VendorID);
    }

    [Fact]
    public async Task Create_SiteVendor_SelfLink_Throws()
    {
        var act = async () => await _vendors.CreateAsync(new Vendor
        {
            WebsiteID = _host.WebsiteID, Name = "Bad", IsActive = true,
            VendorType = (byte)VendorType.Site, LinkedWebsiteID = _host.WebsiteID,
        });
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Create_Vendor_OtherCurrency_WithoutRate_Throws()
    {
        _context.Currencies.Add(new Currency
        {
            CurrencyCode = "KRW", Name = "Won", Symbol = "₩", DecimalDigits = 0, IsActive = true,
        });
        await _context.SaveChangesAsync();

        var act = async () => await _vendors.CreateAsync(new Vendor
        {
            WebsiteID = _host.WebsiteID, Name = "KRW seller",
            IsActive = true, VendorType = (byte)VendorType.Display,
            SettlementCurrencyCode = "KRW",
        });

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Contain("KRW");
        ex.Which.Message.Should().Contain("Exchange Rates");
    }

    [Fact]
    public async Task SiteVendor_CreditControlsCatalogExposure()
    {
        var v = await _vendors.CreateAsync(new Vendor
        {
            WebsiteID = _host.WebsiteID, Name = "Source Shop",
            IsActive = true, VendorType = (byte)VendorType.Site,
            LinkedWebsiteID = _source.WebsiteID, SettlementMode = 1,
            AvailableCreditUsd = 0,
        });

        _vendors.CanExposeCatalog(v).Should().BeFalse();

        var (ok, _, _) = await _credit.GrantAsync(v.VendorID, 100m, "open line", null);
        ok.Should().BeTrue();

        var reloaded = await _vendors.GetByIdAsync(v.VendorID);
        reloaded!.AvailableCreditUsd.Should().Be(100m);
        _vendors.CanExposeCatalog(reloaded).Should().BeTrue();

        var links = await _vendors.GetActiveSiteLinksAsync(_host.WebsiteID);
        links.Should().ContainSingle(x => x.VendorID == v.VendorID);
    }

    [Fact]
    public async Task VendorProduct_SiteLink_RejectsNonOwnedProduct()
    {
        var v = await _vendors.CreateAsync(new Vendor
        {
            WebsiteID = _host.WebsiteID, Name = "Source Shop",
            IsActive = true, VendorType = (byte)VendorType.Site,
            LinkedWebsiteID = _source.WebsiteID, SettlementMode = 0,
        });

        // Product owned by HOST (not source) — must not be listable on site vendor.
        var product = new Product
        {
            WebsiteID = _host.WebsiteID, Slug = "p1", Title = "Host product",
            Status = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        var variant = new ProductVariant
        {
            WebsiteID = _host.WebsiteID, ProductID = product.ProductID, Sku = "SKU1", Title = "Default",
            ReferencePrice = 10, ReferencePriceUsd = 10, IsActive = true, CreatedAt = DateTime.UtcNow,
        };
        _context.ProductVariants.Add(variant);
        await _context.SaveChangesAsync();

        var (ok, err, _) = await _listings.UpsertAsync(new VendorProduct
        {
            VendorID = v.VendorID, ProductVariantID = variant.ProductVariantID,
            ReferencePrice = 10, ReferencePriceUsd = 10, StockQuantity = 5, IsActive = true,
        });
        ok.Should().BeFalse();
        err.Should().Contain("re-shared");
    }

    [Fact]
    public async Task VendorProduct_SiteLink_AllowsSourceOwnedProduct()
    {
        var v = await _vendors.CreateAsync(new Vendor
        {
            WebsiteID = _host.WebsiteID, Name = "Source Shop",
            IsActive = true, VendorType = (byte)VendorType.Site,
            LinkedWebsiteID = _source.WebsiteID, SettlementMode = 0,
        });

        var product = new Product
        {
            WebsiteID = _source.WebsiteID, Slug = "owned", Title = "Source product",
            Status = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        var variant = new ProductVariant
        {
            WebsiteID = _source.WebsiteID, ProductID = product.ProductID, Sku = "SRC1", Title = "Default",
            ReferencePrice = 12, ReferencePriceUsd = 12, IsActive = true, CreatedAt = DateTime.UtcNow,
        };
        _context.ProductVariants.Add(variant);
        await _context.SaveChangesAsync();

        var (ok, err, item) = await _listings.UpsertAsync(new VendorProduct
        {
            VendorID = v.VendorID, ProductVariantID = variant.ProductVariantID,
            ReferencePrice = 12, ReferencePriceUsd = 12, StockQuantity = 3, IsActive = true,
        });
        ok.Should().BeTrue(err);
        item.Should().NotBeNull();
        item!.WebsiteID.Should().Be(_host.WebsiteID);
    }

    [Fact]
    public async Task VendorProduct_Upsert_SyncsInventoryOnHandAsSumOfStoreStocks()
    {
        var v1 = await _vendors.CreateAsync(new Vendor
        {
            WebsiteID = _host.WebsiteID, Name = "Store A", Slug = "store-a",
            IsActive = true, VendorType = (byte)VendorType.Display,
        });
        var v2 = await _vendors.CreateAsync(new Vendor
        {
            WebsiteID = _host.WebsiteID, Name = "Store B", Slug = "store-b",
            IsActive = true, VendorType = (byte)VendorType.Display,
        });

        var product = new Product
        {
            WebsiteID = _host.WebsiteID, Slug = "p1", Title = "Product",
            Status = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        var variant = new ProductVariant
        {
            WebsiteID = _host.WebsiteID, ProductID = product.ProductID, Sku = "P1", Title = "Default",
            ReferencePrice = 10, ReferencePriceUsd = 10, IsActive = true, CreatedAt = DateTime.UtcNow,
        };
        _context.ProductVariants.Add(variant);
        await _context.SaveChangesAsync();

        var (ok1, err1, listing1) = await _listings.UpsertAsync(new VendorProduct
        {
            VendorID = v1.VendorID, ProductVariantID = variant.ProductVariantID,
            ReferencePrice = 10, ReferencePriceUsd = 10, StockQuantity = 5, IsActive = true,
        });
        ok1.Should().BeTrue(err1);

        var (ok2, err2, _) = await _listings.UpsertAsync(new VendorProduct
        {
            VendorID = v2.VendorID, ProductVariantID = variant.ProductVariantID,
            ReferencePrice = 11, ReferencePriceUsd = 11, StockQuantity = 3, IsActive = true,
        });
        ok2.Should().BeTrue(err2);

        var inv = await _context.InventoryItems.SingleAsync(i =>
            i.WebsiteID == _host.WebsiteID && i.ProductVariantID == variant.ProductVariantID);
        inv.QuantityOnHand.Should().Be(8);

        // Reserve on store A, then commit sale → stock and inventory total drop after commit+sync.
        (await _listings.ReserveAsync(listing1!.VendorProductID, 2)).Should().BeTrue();
        var reserved = await _context.VendorProducts.SingleAsync(x => x.VendorProductID == listing1.VendorProductID);
        reserved.QuantityReserved.Should().Be(2);
        IVendorProductService.Available(reserved).Should().Be(3);

        await _listings.CommitSaleAsync(listing1.VendorProductID, 2);
        await _listings.SyncInventoryOnHandFromListingsAsync(_host.WebsiteID, variant.ProductVariantID);

        // The service wrote through its own short-lived context, so this fixture's context must
        // re-read rather than answer from entities it is still tracking.
        _context.ChangeTracker.Clear();

        reserved = await _context.VendorProducts.SingleAsync(x => x.VendorProductID == listing1.VendorProductID);
        reserved.StockQuantity.Should().Be(3);
        reserved.QuantityReserved.Should().Be(0);

        inv = await _context.InventoryItems.SingleAsync(i =>
            i.WebsiteID == _host.WebsiteID && i.ProductVariantID == variant.ProductVariantID);
        inv.QuantityOnHand.Should().Be(6); // 3 + 3
    }

    [Fact]
    public async Task VendorProduct_Cancel_ReleasesReservationWithoutReducingOnHand()
    {
        var v = await _vendors.CreateAsync(new Vendor
        {
            WebsiteID = _host.WebsiteID, Name = "Store C", Slug = "store-c",
            IsActive = true, VendorType = (byte)VendorType.Display,
        });
        var product = new Product
        {
            WebsiteID = _host.WebsiteID, Slug = "p2", Title = "Product 2",
            Status = 1, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        var variant = new ProductVariant
        {
            WebsiteID = _host.WebsiteID, ProductID = product.ProductID, Sku = "P2", Title = "Default",
            ReferencePrice = 5, ReferencePriceUsd = 5, IsActive = true, CreatedAt = DateTime.UtcNow,
        };
        _context.ProductVariants.Add(variant);
        await _context.SaveChangesAsync();

        var (ok, err, listing) = await _listings.UpsertAsync(new VendorProduct
        {
            VendorID = v.VendorID, ProductVariantID = variant.ProductVariantID,
            ReferencePrice = 5, ReferencePriceUsd = 5, StockQuantity = 4, IsActive = true,
        });
        ok.Should().BeTrue(err);

        (await _listings.ReserveAsync(listing!.VendorProductID, 1)).Should().BeTrue();
        await _listings.ReleaseReservationAsync(listing.VendorProductID, 1);

        var row = await _context.VendorProducts.SingleAsync(x => x.VendorProductID == listing.VendorProductID);
        row.StockQuantity.Should().Be(4);
        row.QuantityReserved.Should().Be(0);
    }
}
