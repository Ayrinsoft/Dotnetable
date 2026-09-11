using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class CategoryServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CategoryService _service;
    private readonly Website _website;

    public CategoryServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        // Services open a context per call now, so they get a factory over the same options;
        // the fixture keeps its own _context for seeding and asserting.
        var factory = new TestDbContextFactory(opts);
        _service = new CategoryService(factory);
        _website = new Website
        {
            TradeName = "Test", WebsiteAddress = "test.com", AuthCode = Guid.NewGuid(),
            Active = true, Manager = "Mgr", Mobile = "123", Email = "admin@test.com",
            RegisterDate = DateOnly.FromDateTime(DateTime.Today), DefaultLanguageCode = "en",
            DefaultCurrencyCode = "USD", BrandName = "Test"
        };
        _context.Websites.Add(_website);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Create_Then_SetTranslations_Then_GetPaged_Works()
    {
        var cat = new Category { WebsiteID = _website.WebsiteID, Name = "News", Slug = "news", IsActive = true };
        await _service.CreateAsync(cat);
        cat.CategoryID.Should().BeGreaterThan(0);
        await _service.SetTranslationsAsync(cat.CategoryID, new Dictionary<string, (string, string)>());
        var paged = await _service.GetPagedAsync(_website.WebsiteID, new GridQuery { PageIndex = 1, PageSize = 25 });
        paged.TotalCount.Should().Be(1);
        paged.Items.Should().ContainSingle(c => c.Name == "News");
    }

    [Fact]
    public async Task Update_Detached_Copy_After_Create_Works()
    {
        var cat = new Category { WebsiteID = _website.WebsiteID, Name = "A", Slug = "a", IsActive = true };
        await _service.CreateAsync(cat);
        var copy = new Category
        {
            CategoryID = cat.CategoryID, WebsiteID = cat.WebsiteID, Name = "C", Slug = "c",
            IsActive = true, SortOrder = 0
        };
        await _service.UpdateAsync(copy);
        (await _service.GetByIdAsync(cat.CategoryID))!.Name.Should().Be("C");
    }

    [Fact]
    public async Task Create_Normalizes_Zero_Optional_Fks_To_Null()
    {
        var cat = new Category
        {
            WebsiteID = _website.WebsiteID, Name = "Root", Slug = "root", IsActive = true,
            ParentCategoryID = 0, PostTypeID = 0,
        };
        await _service.CreateAsync(cat);
        cat.ParentCategoryID.Should().BeNull();
        cat.PostTypeID.Should().BeNull();
        var stored = await _service.GetByIdAsync(cat.CategoryID);
        stored!.ParentCategoryID.Should().BeNull();
        stored.PostTypeID.Should().BeNull();
    }

    [Fact]
    public async Task Create_With_Translations_Works()
    {
        var cat = new Category { WebsiteID = _website.WebsiteID, Name = "News", Slug = "news", IsActive = true };
        await _service.CreateAsync(cat);
        await _service.SetTranslationsAsync(cat.CategoryID, new Dictionary<string, (string, string)>
        {
            ["fa"] = ("Ø§Ø®Ø¨Ø§Ø±", "akhbar"),
        });
        var tr = await _service.GetTranslationsAsync(cat.CategoryID);
        tr.Should().ContainSingle(t => t.LanguageCode == "fa" && t.Name == "Ø§Ø®Ø¨Ø§Ø±");
    }

    [Fact]
    public async Task GetTree_Orders_Siblings_By_SortOrder_Not_Globally()
    {
        var rootA = await _service.CreateAsync(new Category
        {
            WebsiteID = _website.WebsiteID, Name = "A", Slug = "a", IsActive = true, SortOrder = 5,
        });
        var rootB = await _service.CreateAsync(new Category
        {
            WebsiteID = _website.WebsiteID, Name = "B", Slug = "b", IsActive = true, SortOrder = 2,
        });
        await _service.CreateAsync(new Category
        {
            WebsiteID = _website.WebsiteID, Name = "A2", Slug = "a2", IsActive = true, SortOrder = 20,
            ParentCategoryID = rootA.CategoryID,
        });
        await _service.CreateAsync(new Category
        {
            WebsiteID = _website.WebsiteID, Name = "A1", Slug = "a1", IsActive = true, SortOrder = 1,
            ParentCategoryID = rootA.CategoryID,
        });
        await _service.CreateAsync(new Category
        {
            WebsiteID = _website.WebsiteID, Name = "B1", Slug = "b1", IsActive = true, SortOrder = 5,
            ParentCategoryID = rootB.CategoryID,
        });

        var tree = await _service.GetTreeAsync(_website.WebsiteID);

        // Sibling sort: B (2) then A (5). A1 (1) stays under A even though 1 is smaller than B.
        tree.Select(c => c.Name).Should().Equal("B", "A");
        tree[0].Children.Select(c => c.Name).Should().Equal("B1");
        tree[1].Children.Select(c => c.Name).Should().Equal("A1", "A2");
    }

    [Fact]
    public async Task Create_Sanitizes_A_Raw_Title_Slug_With_Spaces()
    {
        var cat = new Category { WebsiteID = _website.WebsiteID, Name = "تجهیز و تعمیر", Slug = "تجهیز و تعمیر", IsActive = true };
        await _service.CreateAsync(cat);

        cat.Slug.Should().NotContain(" ");
        cat.Slug.Should().Be("تجهیز-و-تعمیر");
    }

    [Fact]
    public async Task Create_Appends_Suffix_On_Slug_Collision()
    {
        await _service.CreateAsync(new Category { WebsiteID = _website.WebsiteID, Name = "News", Slug = "news", IsActive = true });
        var second = new Category { WebsiteID = _website.WebsiteID, Name = "News Again", Slug = "news", IsActive = true };
        await _service.CreateAsync(second);

        second.Slug.Should().Be("news-2");
    }

    [Fact]
    public async Task SetTranslationsAsync_Sanitizes_And_Dedupes_Translation_Slug()
    {
        var cat = new Category { WebsiteID = _website.WebsiteID, Name = "News", Slug = "news", IsActive = true };
        await _service.CreateAsync(cat);
        var other = new Category { WebsiteID = _website.WebsiteID, Name = "Other", Slug = "other-fa", IsActive = true };
        await _service.CreateAsync(other);

        // A translation slug that collides with another category's slug (main or translation) in
        // the same website must be de-duplicated, and raw spaces must never survive.
        await _service.SetTranslationsAsync(cat.CategoryID, new Dictionary<string, (string, string)>
        {
            ["fa"] = ("اخبار روز", "other fa"),
        });

        var tr = await _service.GetTranslationsAsync(cat.CategoryID);
        tr.Should().ContainSingle(t => t.LanguageCode == "fa" && t.Slug == "other-fa-2");
    }

    public void Dispose() => _context.Dispose();
}

