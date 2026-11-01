using Dotnetable.Application.Catalog;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
