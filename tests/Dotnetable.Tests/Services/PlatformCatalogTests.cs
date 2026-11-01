using Dotnetable.Application.Catalog;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Dotnetable.Tests.Services;

public class PlatformCatalogTests
{
    [Fact]
    public void Parser_ResolvesParentFromPath()
    {
        var rows = GoogleProductTaxonomyParser.Parse("""
            # Google_Product_Taxonomy_Version: test
            1 - Animals
            2 - Animals > Pets
            """);

        rows.Should().HaveCount(2);
        rows[1].ParentGoogleId.Should().Be(1);
        rows[1].Name.Should().Be("Pets");
        rows[1].Depth.Should().Be(2);
    }

    [Fact]
    public async Task CreateCategory_OnChildSite_IsLockedWhenPlatform()
    {
        using var db = new RelationalTestDb();
        var factory = new TestDbContextFactory(db.Options);
        SeedSites(db, platform: true);
        var service = new ProductCategoryService(factory);

        var act = () => service.CreateAsync(new ProductCategory
        {
            WebsiteID = 2,
            Name = "Local",
            Slug = "local",
            IsActive = true,
        });

        await act.Should().ThrowAsync<CatalogLockedException>();
    }

    [Fact]
    public async Task Deactivate_HidesCategory_Detach_ClearsParent_DeleteBranch_RemovesUnusedTree()
    {
        using var db = new RelationalTestDb();
        var factory = new TestDbContextFactory(db.Options);
        SeedSites(db, platform: true);
        await using (var seed = db.NewContext())
        {
            seed.ProductCategories.Add(new ProductCategory
            {
                WebsiteID = 1, Name = "Root", Slug = "root", IsActive = true,
            });
            await seed.SaveChangesAsync();
            var root = await seed.ProductCategories.SingleAsync(c => c.Slug == "root");
            seed.ProductCategories.Add(new ProductCategory
            {
                WebsiteID = 1, ParentCategoryID = root.ProductCategoryID, Name = "Child", Slug = "child", IsActive = true,
            });
            var other = new ProductCategory
            {
                WebsiteID = 1, Name = "Other", Slug = "other", IsActive = true,
            };
            seed.ProductCategories.Add(other);
            await seed.SaveChangesAsync();
            seed.ProductCategories.Add(new ProductCategory
            {
                WebsiteID = 1, ParentCategoryID = other.ProductCategoryID, Name = "Other child", Slug = "other-child", IsActive = true,
            });
            await seed.SaveChangesAsync();
        }

        var service = new ProductCategoryService(factory);
        var rootId = (await service.GetChildrenAsync(1, null, null)).Single(c => c.Name == "Root").ProductCategoryID;
        var childId = (await service.GetChildrenAsync(1, rootId, null)).Single().ProductCategoryID;

        await service.SetActiveAsync(rootId, false);
        (await service.GetChildrenAsync(1, null, null)).Should().NotContain(c => c.Name == "Root");

        await service.SetActiveAsync(rootId, true);
        var otherId = (await service.GetChildrenAsync(1, null, null)).Single(c => c.Name == "Other").ProductCategoryID;
        await service.DeleteBranchAsync(otherId);
        await using (var mid = db.NewContext())
        {
            (await mid.ProductCategories.AnyAsync(c => c.Slug == "other" || c.Slug == "other-child")).Should().BeFalse();
        }

        await service.DetachFromParentAsync(childId);
        await using (var check = db.NewContext())
        {
            var child = await check.ProductCategories.SingleAsync(c => c.ProductCategoryID == childId);
            child.ParentCategoryID.Should().BeNull();
        }

        await service.DeleteBranchAsync(rootId);
        await service.DeleteBranchAsync(childId);
        await using (var check = db.NewContext())
        {
            (await check.ProductCategories.CountAsync()).Should().Be(0);
        }
    }

    [Fact]
    public async Task Diff_ShowsGoogleId_ThatIsNotImported()
    {
        using var db = new RelationalTestDb();
        var factory = new TestDbContextFactory(db.Options);
        SeedSites(db, platform: false);
        await using (var seed = db.NewContext())
        {
            seed.GoogleProductTaxonomyEntries.AddRange(
                Entry(10, "Animals"),
                Entry(11, "Animals > Pets"));
            seed.ProductCategories.Add(new ProductCategory
            {
                WebsiteID = 1, Name = "Animals", Slug = "animals", IsActive = true, GoogleProductCategoryId = 10,
            });
            await seed.SaveChangesAsync();
        }

        var service = new GoogleProductTaxonomyService(factory, new EmptyHttp(), TestExcel.Registry);
        var diff = await service.GetDiffAsync(onlyMissing: true, 0, 50);
        diff.Items.Select(i => i.GoogleId).Should().Equal(11);

        var imported = await service.ImportMissingAsync(new[] { 11 });
        imported.Created.Should().Be(1);
        var after = await service.GetDiffAsync(onlyMissing: true, 0, 50);
        after.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task TranslationWorkbook_RoundTripsTranslatedName()
    {
        using var db = new RelationalTestDb();
        var factory = new TestDbContextFactory(db.Options);
        SeedSites(db, platform: false);
        int categoryId;
        await using (var seed = db.NewContext())
        {
            var category = new ProductCategory
            {
                WebsiteID = 1, Name = "Animals", Slug = "animals", IsActive = true,
            };
            seed.ProductCategories.Add(category);
            await seed.SaveChangesAsync();
            categoryId = category.ProductCategoryID;
        }

        var service = new GoogleProductTaxonomyService(factory, new EmptyHttp(), TestExcel.Registry);
        var bytes = await service.ExportTranslationsAsync("fa");
        var table = TestExcel.Registry.Read(new MemoryStream(bytes));
        var nameColumn = Array.IndexOf(table[0], "TranslatedName");
        table[1][nameColumn] = "Heyvanat";
        var edited = TestExcel.Registry.Write(
            "Categories",
            table[0],
            table.Skip(1).Select(r => (IReadOnlyList<object?>)r.Cast<object?>().ToList()));

        var result = await service.ImportTranslationsAsync(new MemoryStream(edited));
        result.Updated.Should().Be(1);
        result.Errors.Should().BeEmpty();

        await using var check = db.NewContext();
        var translation = await check.ProductCategoryTranslations.SingleAsync();
        translation.ProductCategoryID.Should().Be(categoryId);
        translation.LanguageCode.Should().Be("fa");
        translation.Name.Should().Be("Heyvanat");
    }

    [Fact]
    public async Task Copy_KeepsSourceRows()
    {
        using var db = new RelationalTestDb();
        var factory = new TestDbContextFactory(db.Options);
        SeedSites(db, platform: true);
        await using (var seed = db.NewContext())
        {
            seed.Pages.Add(new Page
            {
                WebsiteID = 2, Slug = "about", Title = "About", Status = 1, IsActive = true, CreatedAt = DateTime.UtcNow,
            });
            seed.ProductCategories.Add(new ProductCategory
            {
                WebsiteID = 2, Name = "Local", Slug = "local", IsActive = true, GoogleProductCategoryId = 99,
            });
            await seed.SaveChangesAsync();
        }

        var service = new WebsiteContentCopyService(factory);
        var result = await service.CopyAsync(2, 3);
        result.Pages.Should().Be(1);
        result.ProductCategories.Should().Be(0);

        await using var check = db.NewContext();
        (await check.Pages.CountAsync(p => p.WebsiteID == 2)).Should().Be(1);
        (await check.Pages.CountAsync(p => p.WebsiteID == 3)).Should().Be(1);
        (await check.ProductCategories.CountAsync(c => c.WebsiteID == 2)).Should().Be(1);
    }

    [Fact]
    public async Task VisibleCategories_LimitEachWebsite_WithoutPullingInEveryChild()
    {
        using var db = new RelationalTestDb();
        var factory = new TestDbContextFactory(db.Options);
        SeedSites(db, platform: true);
        int toys, cars, dolls, stationery, electronics;
        await using (var seed = db.NewContext())
        {
            var toy = new ProductCategory { WebsiteID = 1, Name = "Toys", Slug = "toys", IsActive = true };
            var paper = new ProductCategory { WebsiteID = 1, Name = "Stationery", Slug = "stationery", IsActive = true };
            var other = new ProductCategory { WebsiteID = 1, Name = "Electronics", Slug = "electronics", IsActive = true };
            seed.ProductCategories.AddRange(toy, paper, other);
            await seed.SaveChangesAsync();
            seed.ProductCategories.AddRange(
                new ProductCategory { WebsiteID = 1, ParentCategoryID = toy.ProductCategoryID, Name = "Cars", Slug = "cars", IsActive = true },
                new ProductCategory { WebsiteID = 1, ParentCategoryID = toy.ProductCategoryID, Name = "Dolls", Slug = "dolls", IsActive = true });
            await seed.SaveChangesAsync();
            toys = toy.ProductCategoryID;
            stationery = paper.ProductCategoryID;
            electronics = other.ProductCategoryID;
            cars = seed.ProductCategories.Single(c => c.Slug == "cars").ProductCategoryID;
            dolls = seed.ProductCategories.Single(c => c.Slug == "dolls").ProductCategoryID;

            seed.Products.AddRange(
                ProductIn(1, "car", cars),
                ProductIn(1, "pen", stationery),
                ProductIn(1, "phone", electronics));
            await seed.SaveChangesAsync();
        }

        var categories = new ProductCategoryService(factory);
        var branch = await categories.GetBranchIdsAsync(3, toys);
        branch.Should().BeEquivalentTo(new[] { toys, cars, dolls });

        await categories.SetVisibilityAsync(3, limitToSelection: true, new[] { toys, cars });
        var storeTree = await categories.GetTreeAsync(3);
        storeTree.Select(c => c.Slug).Should().Equal("toys");
        storeTree[0].Children.Select(c => c.Slug).Should().Equal("cars");
        (await categories.GetBySlugAsync(3, "dolls")).Should().BeNull();
        (await categories.GetBySlugAsync(3, "toys")).Should().NotBeNull();

        var vendors = new Mock<IVendorService>();
        vendors.Setup(v => v.GetActiveSiteLinksAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vendor>());
        var products = new ProductService(factory, new Mock<ICurrencyConversionService>().Object, vendors.Object);
        var listed = await products.GetPublishedAsync(3, null, null, null, null, null, 1, 20);
        listed.Items.Select(p => p.Slug).Should().BeEquivalentTo("car");

        await categories.SetVisibilityAsync(1, limitToSelection: true, new[] { toys, stationery });
        var masterTree = await categories.GetTreeAsync(1);
        masterTree.Select(c => c.Slug).Should().BeEquivalentTo("toys", "stationery");
        masterTree.Single(c => c.Slug == "toys").Children.Should().BeEmpty();
        var masterListed = await products.GetPublishedAsync(1, null, null, null, null, null, 1, 20);
        masterListed.Items.Select(p => p.Slug).Should().BeEquivalentTo("pen");
    }

    [Fact]
    public async Task ChildStore_PricesCatalogVariants_AndMayAddUsedGoods()
    {
        using var db = new RelationalTestDb();
        var factory = new TestDbContextFactory(db.Options);
        SeedSites(db, platform: true);
        var currency = new Mock<ICurrencyConversionService>();
        currency.Setup(c => c.ToDisplayFromLocalAsync(
                It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<decimal?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MoneyDto { Amount = 1, CurrencyCode = "USD", AmountUsd = 1 });
        var vendors = new Mock<IVendorService>();
        vendors.Setup(v => v.GetActiveSiteLinksAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vendor>());
        var products = new ProductService(factory, currency.Object, vendors.Object);

        var blocked = () => products.CreateAsync(new Product
        {
            WebsiteID = 3, Title = "New toy", Slug = "new-toy", Status = ProductService.PublishedStatus, IsActive = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });
        await blocked.Should().ThrowAsync<CatalogLockedException>();

        var used = await products.CreateAsync(new Product
        {
            WebsiteID = 3, IsPreOwned = true, Title = "Used doll", Slug = "used-doll",
            Status = ProductService.PublishedStatus, IsActive = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });
        await using (var seed = db.NewContext())
        {
            seed.ProductCategories.Add(new ProductCategory { WebsiteID = 1, Name = "Toys", Slug = "toys", IsActive = true });
            await seed.SaveChangesAsync();
        }
        var toys = (await new ProductCategoryService(factory).GetChildrenAsync(3, null, null)).Single().ProductCategoryID;
        await products.SetCategoriesAsync(used.ProductID, new[] { toys }, toys);

        var catalog = await products.CreateAsync(new Product
        {
            WebsiteID = 1, Title = "Doll", Slug = "doll", Status = ProductService.PublishedStatus, IsActive = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
            ProductVariants =
            {
                new ProductVariant
                {
                    WebsiteID = 1, Sku = "DOLL", Title = "Doll", IsDefault = true, IsActive = true,
                    ReferencePrice = 10, ReferencePriceUsd = 10,
                },
            },
        });
        var variantId = catalog.ProductVariants.Single().ProductVariantID;
        var listings = new VendorProductService(factory, Mock.Of<IExcelExporterRegistry>());
        var priced = await listings.SetStoreVariantPricesAsync(3, catalog.ProductID, new[]
        {
            new StoreVariantPriceDto { ProductVariantID = variantId, ReferencePrice = 25, ReferencePriceUsd = 25, StockQuantity = 2 },
        });
        priced.Success.Should().BeTrue(priced.Error);

        await using (var check = db.NewContext())
        {
            (await check.ProductVariants.SingleAsync()).ReferencePrice.Should().Be(10);
            var row = await check.VendorProducts.SingleAsync();
            row.WebsiteID.Should().Be(3);
            row.ReferencePrice.Should().Be(25);
            row.StockQuantity.Should().Be(2);
        }

        var page = await products.GetPublishedAsync(3, null, null, null, null, null, 1, 20);
        page.Items.Select(p => p.Slug).Should().BeEquivalentTo("used-doll", "doll");
        (await products.GetBySlugAsync(3, "used-doll")).Should().NotBeNull();
    }

    private static Product ProductIn(int websiteId, string slug, int categoryId) => new()
    {
        WebsiteID = websiteId,
        Slug = slug,
        Title = slug,
        Status = ProductService.PublishedStatus,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        ProductCategoryMaps = { new ProductCategoryMap { ProductCategoryID = categoryId, IsPrimary = true } },
    };

    private static void SeedSites(RelationalTestDb db, bool platform)
    {
        using var context = db.NewContext();
        context.Websites.Add(Site("Master", platform));
        context.SaveChanges();
        context.Websites.Single().WebsiteID.Should().Be(1);
        context.Websites.Add(Site("Blog", false));
        context.Websites.Add(Site("Store", false));
        context.SaveChanges();
    }

    private static Website Site(string name, bool platform) => new()
    {
        TradeName = name,
        BrandName = name,
        WebsiteAddress = name.ToLowerInvariant() + ".test",
        AuthCode = Guid.NewGuid(),
        Active = true,
        Manager = "Mgr",
        Mobile = "1",
        Email = name.ToLowerInvariant() + "@test.com",
        RegisterDate = new DateOnly(2026, 1, 1),
        DefaultLanguageCode = "en",
        DefaultCurrencyCode = "USD",
        IsPlatform = platform,
    };

    private static GoogleProductTaxonomyEntry Entry(int id, string path) => new()
    {
        GoogleId = id,
        Name = path.Split(" > ").Last(),
        FullPath = path,
        Depth = (byte)path.Split(" > ").Length,
        ParentGoogleId = path.Contains(" > ") ? 10 : null,
        LastSeenUtc = DateTime.UtcNow,
    };

    private sealed class EmptyHttp : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }
}
