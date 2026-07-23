using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class VendorServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly VendorService _vendors;
    private readonly VendorProductService _listings;
    private readonly VendorCreditService _credit;
    private readonly Website _host;
    private readonly Website _source;

    public VendorServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        _vendors = new VendorService(_context);
        _listings = new VendorProductService(_context);
        _credit = new VendorCreditService(_context, _vendors);

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

    public void Dispose() => _context.Dispose();

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
            WebsiteID = _host.WebsiteID, ProductID = product.ProductID, Sku = "SKU1",
            ReferencePriceUsd = 10, IsActive = true, CreatedAt = DateTime.UtcNow,
        };
        _context.ProductVariants.Add(variant);
        await _context.SaveChangesAsync();

        var (ok, err, _) = await _listings.UpsertAsync(new VendorProduct
        {
            VendorID = v.VendorID, ProductVariantID = variant.ProductVariantID,
            ReferencePriceUsd = 10, StockQuantity = 5, IsActive = true,
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
            WebsiteID = _source.WebsiteID, ProductID = product.ProductID, Sku = "SRC1",
            ReferencePriceUsd = 12, IsActive = true, CreatedAt = DateTime.UtcNow,
        };
        _context.ProductVariants.Add(variant);
        await _context.SaveChangesAsync();

        var (ok, err, item) = await _listings.UpsertAsync(new VendorProduct
        {
            VendorID = v.VendorID, ProductVariantID = variant.ProductVariantID,
            ReferencePriceUsd = 12, StockQuantity = 3, IsActive = true,
        });
        ok.Should().BeTrue(err);
        item.Should().NotBeNull();
        item!.WebsiteID.Should().Be(_host.WebsiteID);
    }
}
