using Dotnetable.Application.Content;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class ContentEditionRulesTests
{
    [Fact]
    public void Compare_OrdersNumericSegments()
    {
        ContentEditionRules.Compare("1.2", "1.10").Should().BeNegative();
        ContentEditionRules.Compare("1.10", "2.0").Should().BeNegative();
        ContentEditionRules.Compare("1.2", "1.2.0").Should().Be(0);
        ContentEditionRules.Compare(null, "1.0").Should().BeNegative();
    }

    [Fact]
    public void Resolve_PicksHighestAppliesFrom_AndInclusiveEnd()
    {
        var rows = new List<EditionCandidate>
        {
            Row(1, "1.0", "2.0"),
            Row(2, "2.0", null),
        };

        ContentEditionRules.Resolve(rows, "1.0")!.Id.Should().Be(1);
        ContentEditionRules.Resolve(rows, "1.5")!.Id.Should().Be(1);
        ContentEditionRules.Resolve(rows, "2.0")!.Id.Should().Be(2);
        ContentEditionRules.Resolve(rows, "3.0")!.Id.Should().Be(2);
        ContentEditionRules.Resolve(rows, "0.9").Should().BeNull();
    }

    [Fact]
    public void Resolve_NullVersion_PrefersUnversioned_ThenHighestStart()
    {
        var unversioned = new List<EditionCandidate>
        {
            Row(1, null, null),
            Row(2, "1.0", null),
        };
        ContentEditionRules.Resolve(unversioned, null)!.Id.Should().Be(1);

        var onlyRanged = new List<EditionCandidate> { Row(4, "1.0", null), Row(3, "2.0", "2.0") };
        ContentEditionRules.Resolve(onlyRanged, null)!.Id.Should().Be(3);
    }

    [Fact]
    public void Resolve_Tie_UsesHigherId()
    {
        var rows = new List<EditionCandidate> { Row(5, "1.0", null), Row(9, "1.0", null) };
        ContentEditionRules.Resolve(rows, "1.0")!.Id.Should().Be(9);
    }

    [Fact]
    public void SelectForVersion_KeepsPlainPosts_AndOneEdition()
    {
        var rows = new List<EditionCandidate>
        {
            new(1, 1, false, null, null, null, new DateTime(2026, 1, 1)),
            new(2, 2, true, "getting-started", "1.0", "2.0", new DateTime(2026, 1, 2)),
            new(3, 2, true, "getting-started", "2.0", null, new DateTime(2026, 1, 3)),
        };

        ContentEditionRules.SelectForVersion(rows, "1.0").Should().Equal(2, 1);
        ContentEditionRules.SelectForVersion(rows, "2.0").Should().Equal(3, 1);
    }

    private static EditionCandidate Row(int id, string? from, string? to) =>
        new(id, 1, true, "getting-started", from, to, DateTime.UtcNow);
}

public class ContentEditionServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly PostService _posts;
    private readonly ContentVersionService _versions;

    public ContentEditionServiceTests()
    {
        _connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = ":memory:",
            Mode = SqliteOpenMode.Memory,
            ForeignKeys = true,
        }.ToString());
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using (var created = new AppDbContext(_options))
            created.Database.EnsureCreated();
        var factory = new TestDbContextFactory(_options);
        _posts = new PostService(factory);
        _versions = new ContentVersionService(factory);
    }

    [Fact]
    public async Task BlogType_StripsKey_VersionedType_ResolvesAndForks()
    {
        int websiteId;
        int blogId;
        int helpId;
        using (var seed = new AppDbContext(_options))
        {
            seed.Currencies.Add(new Currency { CurrencyCode = "USD", Name = "US Dollar", Symbol = "$", DecimalDigits = 2, IsActive = true });
            await seed.SaveChangesAsync();
            var website = new Website
            {
                TradeName = "Site", BrandName = "Site", WebsiteAddress = "site.test",
                AuthCode = Guid.NewGuid(), Active = true, Manager = "Mgr", Mobile = "1",
                Email = "mgr@site.test", RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DefaultLanguageCode = "en", DefaultCurrencyCode = "USD",
            };
            seed.Websites.Add(website);
            await seed.SaveChangesAsync();
            websiteId = website.WebsiteID;
            var blog = new PostType { WebsiteID = websiteId, Name = "Blog", Slug = "blog" };
            var help = new PostType { WebsiteID = websiteId, Name = "Help", Slug = "help", SupportsVersioning = true };
            seed.PostTypes.AddRange(blog, help);
            await seed.SaveChangesAsync();
            blogId = blog.PostTypeID;
            helpId = help.PostTypeID;
        }

        var plain = await _posts.CreateAsync(new Post
        {
            WebsiteID = websiteId, PostTypeID = blogId, Title = "Hello", Slug = "hello",
            Content = "blog", Status = PostService.PublishedStatus, IsActive = true,
            StableKey = "hello", AppliesFrom = "1.0",
        });
        plain.StableKey.Should().BeNull();
        plain.AppliesFrom.Should().BeNull();

        var first = await _posts.CreateAsync(new Post
        {
            WebsiteID = websiteId, PostTypeID = helpId, Title = "Getting started", Slug = "getting-started",
            Content = "for 1.0", Status = PostService.PublishedStatus, IsActive = true,
            StableKey = "Getting-Started", AppliesFrom = "1.0",
        });
        first.StableKey.Should().Be("getting-started");

        var act = async () => await _posts.CreateAsync(new Post
        {
            WebsiteID = websiteId, PostTypeID = helpId, Title = "Again", Slug = "again",
            Content = "dup", Status = PostService.PublishedStatus, IsActive = true,
            StableKey = "getting-started", AppliesFrom = "1.0",
        });
        await act.Should().ThrowAsync<ContentEditionException>();

        first.Content = "changed after publish";
        first.StableKey = "other-key";
        var locked = async () => await _posts.UpdateAsync(first);
        await locked.Should().ThrowAsync<ContentEditionException>();

        var second = await _posts.CreateEditionAsync(first.PostID, "2.0");
        second.StableKey.Should().Be("getting-started");
        second.AppliesFrom.Should().Be("2.0");
        second.AppliesTo.Should().BeNull();
        second.Content.Should().Be("for 1.0");
        second.Slug.Should().NotBe(first.Slug);

        using (var check = new AppDbContext(_options))
        {
            var closed = await check.Posts.SingleAsync(p => p.PostID == first.PostID);
            closed.AppliesTo.Should().Be("2.0");
            closed.StableKey.Should().Be("getting-started");
        }

        second.Content = "for 2.0";
        await _posts.UpdateAsync(second);

        (await _posts.GetByStableKeyAsync(websiteId, "getting-started", "1.0", "help"))!.Content.Should().Be("for 1.0");
        (await _posts.GetByStableKeyAsync(websiteId, "getting-started", "2.0", "help"))!.Content.Should().Be("for 2.0");
        (await _posts.GetByStableKeyAsync(websiteId, "getting-started", "1.5", "help"))!.Content.Should().Be("for 1.0");

        var atOne = await _posts.GetPublishedAsync(websiteId, "help", null, null, 1, 10, version: "1.0");
        atOne.Items.Should().ContainSingle().Which.PostID.Should().Be(first.PostID);

        await _versions.SaveAsync(new ContentVersion
        {
            WebsiteID = websiteId, Name = "2.0", IsLatest = true, IsPublished = true, SortOrder = 2,
        });
        (await _posts.GetByStableKeyAsync(websiteId, "getting-started", postTypeSlug: "help"))!.Content.Should().Be("for 2.0");
    }

    public void Dispose() => _connection.Dispose();
}
