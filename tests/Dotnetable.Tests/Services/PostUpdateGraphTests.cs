using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

/// <summary>
/// The admin editor saves the graph returned by GetById. Update must write the post row only;
/// attaching tags or location tries to insert PostTags keys that already exist.
/// </summary>
public class PostUpdateGraphTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly PostService _posts;

    public PostUpdateGraphTests()
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
        _posts = new PostService(new TestDbContextFactory(_options));
    }

    [Fact]
    public async Task UpdateAsync_LoadedGraph_SavesScalarsAndLeavesRelations()
    {
        int postId;
        int views;
        using (var seed = new AppDbContext(_options))
        {
            seed.Currencies.Add(new Currency { CurrencyCode = "USD", Name = "US Dollar", Symbol = "$", DecimalDigits = 2, IsActive = true });
            var country = new Country { CountryCode = "IR", LanguageCode = "en", Title = "Iran", PhonePerfix = "98" };
            seed.Countries.Add(country);
            await seed.SaveChangesAsync();
            var city = new City { CountryID = country.CountryID, Title = "Tehran", LanguageCode = "en", Longitude = 51.4, Active = true };
            seed.Cities.Add(city);
            var website = new Website
            {
                TradeName = "Site", BrandName = "Site", WebsiteAddress = "site.test",
                AuthCode = Guid.NewGuid(), Active = true, Manager = "Mgr", Mobile = "1",
                Email = "mgr@site.test", RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DefaultLanguageCode = "en", DefaultCurrencyCode = "USD",
            };
            seed.Websites.Add(website);
            await seed.SaveChangesAsync();
            var type = new PostType { WebsiteID = website.WebsiteID, Name = "News", Slug = "news", CommentsEnabled = true };
            seed.PostTypes.Add(type);
            var tag = new Tag { WebsiteID = website.WebsiteID, Name = "Travel", Slug = "travel" };
            seed.Tags.Add(tag);
            var category = new Category { WebsiteID = website.WebsiteID, Name = "Guides", Slug = "guides", IsActive = true };
            seed.Categories.Add(category);
            await seed.SaveChangesAsync();
            var post = new Post
            {
                WebsiteID = website.WebsiteID,
                PostTypeID = type.PostTypeID,
                Slug = "office",
                Title = "Office",
                Status = PostService.PublishedStatus,
                IsActive = true,
                HasLocation = true,
                ViewCount = 12,
                LikeCount = 3,
            };
            post.Tags.Add(tag);
            post.PostCategories.Add(new PostCategory { CategoryID = category.CategoryID, IsPrimary = true });
            post.PostTranslations.Add(new PostTranslation { LanguageCode = "fa", Title = "دفتر", Slug = "daftar" });
            seed.Posts.Add(post);
            await seed.SaveChangesAsync();
            seed.PostLocations.Add(new PostLocation
            {
                PostID = post.PostID,
                Latitude = 35.7,
                Longitude = 51.4,
                Address = "Valiasr",
                CityID = city.CityID,
                PostLocationTranslations = { new PostLocationTranslation { LanguageCode = "fa", Address = "ولیعصر" } },
            });
            await seed.SaveChangesAsync();
            postId = post.PostID;
            views = post.ViewCount;
        }

        var loaded = await _posts.GetByIdAsync(postId);
        loaded.Should().NotBeNull();
        loaded!.Title = "Office updated";
        loaded.ViewCount = 0;

        await _posts.UpdateAsync(loaded);

        using var check = new AppDbContext(_options);
        var saved = await check.Posts.SingleAsync(p => p.PostID == postId);
        saved.Title.Should().Be("Office updated");
        saved.Slug.Should().Be("office");
        saved.ViewCount.Should().Be(views);
        saved.LikeCount.Should().Be(3);
        (await check.Posts.Where(p => p.PostID == postId).SelectMany(p => p.Tags).CountAsync()).Should().Be(1);
        (await check.PostCategories.CountAsync(pc => pc.PostID == postId)).Should().Be(1);
        (await check.PostLocations.CountAsync(l => l.PostID == postId)).Should().Be(1);
        (await check.PostLocationTranslations.CountAsync(t => t.PostID == postId)).Should().Be(1);
        (await check.PostTranslations.CountAsync(t => t.PostID == postId)).Should().Be(1);
        (await check.Cities.Where(c => c.Title == "Tehran").Select(c => c.Title).SingleAsync()).Should().Be("Tehran");
    }

    public void Dispose() => _connection.Dispose();
}
