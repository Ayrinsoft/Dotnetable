using Dotnetable.Application.DTOs;
using Dotnetable.Application.Instagram;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Instagram;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class PostGalleryAndInstagramTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;
    private readonly PostService _posts;

    public PostGalleryAndInstagramTests()
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
    public async Task SetImages_FirstPhotoIsCover_AndDetailReturnsTheGallery()
    {
        var (websiteId, postId, first, second) = await SeedPostAsync();

        await _posts.SetImagesAsync(postId, [second, first]);

        using var check = new AppDbContext(_options);
        var post = await check.Posts.SingleAsync(p => p.PostID == postId);
        post.FeaturedImageFileID.Should().Be(second);
        var order = await check.PostImages.Where(i => i.PostID == postId).OrderBy(i => i.SortOrder).Select(i => i.FileRecordID).ToListAsync();
        order.Should().Equal(second, first);

        var detail = await _posts.GetBySlugAsync(websiteId, "gallery-post", "fa");
        detail!.Images.Select(i => i.SortOrder).Should().Equal(0, 1);
        detail.Images[0].Url.Should().Be("https://cdn.test/t2.png");
        detail.Images[0].FullUrl.Should().Be("https://cdn.test/f2.png");
        detail.FeaturedImageUrl.Should().Be("https://cdn.test/t2.png");
    }

    [Fact]
    public async Task Publish_UsesPrimaryExcerpt_SendsCoverFirst_AndBlocksASecondSend()
    {
        var (websiteId, postId, first, second) = await SeedPostAsync();
        using (var seed = new AppDbContext(_options))
        {
            var post = await seed.Posts.SingleAsync(p => p.PostID == postId);
            post.Excerpt = "Primary summary";
            post.PostTranslations.Add(new PostTranslation
            {
                LanguageCode = "fa",
                Title = "ترجمه",
                Slug = "gallery-post-fa",
                Excerpt = "خلاصه ترجمه",
            });
            seed.WebsiteInstagramAccounts.Add(new WebsiteInstagramAccount
            {
                WebsiteID = websiteId,
                IgUserId = "1789",
                AccessToken = "page-token",
                IsActive = true,
                UpdatedAt = DateTime.UtcNow,
            });
            await seed.SaveChangesAsync();
        }

        var graph = new FakeGraph();
        var host = new FakeHost();
        var publisher = new InstagramPublishService(new TestDbContextFactory(_options), graph, host);

        var link = await publisher.PublishAsync(postId, new InstagramPublishRequest
        {
            UsePrimaryExcerpt = true,
            Caption = "should not win",
            CoverIndex = 1,
            Items =
            [
                new InstagramMediaItem { Kind = InstagramMediaKind.Image, FileRecordId = first },
                new InstagramMediaItem { Kind = InstagramMediaKind.Image, FileRecordId = second },
            ],
        });

        link.Should().Be("https://instagram.com/p/abc");
        graph.Caption.Should().Be("Primary summary");
        graph.ImageUrls.Should().HaveCount(2);
        graph.ImageUrls[0].Should().EndWith(".jpg");
        host.Deleted.Should().BeEquivalentTo(graph.ImageUrls);
        host.Deleted.Should().OnlyContain(url => url.StartsWith("https://cdn.test/", StringComparison.Ordinal));

        var again = await publisher.PublishAsync(postId, new InstagramPublishRequest
        {
            Items = [new InstagramMediaItem { Kind = InstagramMediaKind.Image, Url = "https://cdn.test/direct.jpg" }],
        });
        again.Should().Be("https://instagram.com/p/abc");
        using var rows = new AppDbContext(_options);
        (await rows.PostInstagramPublications.CountAsync()).Should().Be(1);
    }

    [Fact]
    public void Plan_MovesCoverFirst_AndTreatsOneVideoAsReel()
    {
        var slides = new[]
        {
            new PreparedSlide { Url = "https://cdn.test/a.jpg", IsVideo = false },
            new PreparedSlide { Url = "https://cdn.test/b.mp4", IsVideo = true },
            new PreparedSlide { Url = "https://cdn.test/c.jpg", IsVideo = false },
        };

        var planned = InstagramPublishPlanner.Plan("Hello", slides, coverIndex: 2, videoCoverUrl: "https://cdn.test/thumb.jpg");
        planned.IsCarousel.Should().BeTrue();
        planned.IsReel.Should().BeFalse();
        planned.Slides[0].Url.Should().Be("https://cdn.test/c.jpg");
        planned.VideoCoverUrl.Should().BeNull();

        var reel = InstagramPublishPlanner.Plan("Clip", [slides[1]], 0, "https://cdn.test/thumb.jpg");
        reel.IsReel.Should().BeTrue();
        reel.VideoCoverUrl.Should().Be("https://cdn.test/thumb.jpg");
    }

    [Theory]
    [InlineData("https://cdn.test/a.jpg", true)]
    [InlineData("http://cdn.test/a.jpg", false)]
    [InlineData("https://localhost/a.jpg", false)]
    [InlineData("https://192.168.1.8/a.jpg", false)]
    public void PublicHttps_RejectsLocalAndPlainHttp(string url, bool allowed)
    {
        InstagramUrls.IsPublicHttps(url).Should().Be(allowed);
    }

    private async Task<(int WebsiteId, int PostId, int FirstFile, int SecondFile)> SeedPostAsync()
    {
        using var seed = new AppDbContext(_options);
        seed.Currencies.Add(new Currency { CurrencyCode = "USD", Name = "US Dollar", Symbol = "$", DecimalDigits = 2, IsActive = true });
        var website = new Website
        {
            TradeName = "Site",
            BrandName = "Site",
            WebsiteAddress = "https://site.test",
            AuthCode = Guid.NewGuid(),
            Active = true,
            Manager = "Mgr",
            Mobile = "1",
            Email = "mgr@site.test",
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            DefaultLanguageCode = "en",
            DefaultCurrencyCode = "USD",
        };
        seed.Websites.Add(website);
        await seed.SaveChangesAsync();
        var storage = new WebsiteStorageSetting
        {
            WebsiteID = website.WebsiteID,
            StorageSettingsJSON = "{}",
            Active = true,
            MaxFileSizeKB = 1024,
        };
        seed.WebsiteStorageSettings.Add(storage);
        await seed.SaveChangesAsync();
        var type = new PostType { WebsiteID = website.WebsiteID, Name = "News", Slug = "news", CommentsEnabled = true };
        seed.PostTypes.Add(type);
        await seed.SaveChangesAsync();
        var first = File(storage.WebsiteStorageSettingsID, website.WebsiteID, 1);
        var second = File(storage.WebsiteStorageSettingsID, website.WebsiteID, 2);
        seed.FileRecords.AddRange(first, second);
        await seed.SaveChangesAsync();
        var post = new Post
        {
            WebsiteID = website.WebsiteID,
            PostTypeID = type.PostTypeID,
            Slug = "gallery-post",
            Title = "Gallery",
            Status = PostService.PublishedStatus,
            IsActive = true,
            PublishedAt = DateTime.UtcNow,
        };
        seed.Posts.Add(post);
        await seed.SaveChangesAsync();
        return (website.WebsiteID, post.PostID, first.FileRecordID, second.FileRecordID);
    }

    private static FileRecord File(int storageId, int websiteId, int n) => new()
    {
        WebsiteStorageSettingsID = storageId,
        WebsiteID = websiteId,
        FileCategory = (byte)FileCategory.Image,
        OriginalFileName = $"f{n}.png",
        StoredFileName = $"f{n}.png",
        MimeType = "image/png",
        CNDUrl = $"https://cdn.test/f{n}.png",
        ThumbnailCDN = $"https://cdn.test/t{n}.png",
        UploadDate = DateTime.UtcNow,
    };

    public void Dispose() => _connection.Dispose();

    private sealed class FakeGraph : IInstagramGraph
    {
        public string? Caption { get; private set; }
        public List<string> ImageUrls { get; } = new();

        public Task<string> GetUsernameAsync(string igUserId, string accessToken, CancellationToken ct = default) =>
            Task.FromResult("shop");

        public Task<string> CreateImageAsync(string igUserId, string accessToken, string imageUrl, string? caption, bool carouselItem, CancellationToken ct = default)
        {
            ImageUrls.Add(imageUrl);
            if (!carouselItem) Caption = caption;
            return Task.FromResult("image-" + ImageUrls.Count);
        }

        public Task<string> CreateVideoAsync(string igUserId, string accessToken, string videoUrl, string? caption, bool carouselItem, string? coverUrl, CancellationToken ct = default) =>
            Task.FromResult("video");

        public Task<string> CreateCarouselAsync(string igUserId, string accessToken, IReadOnlyList<string> children, string caption, CancellationToken ct = default)
        {
            Caption = caption;
            return Task.FromResult("carousel");
        }

        public Task WaitUntilReadyAsync(string containerId, string accessToken, bool video, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task<string> PublishAsync(string igUserId, string accessToken, string creationId, CancellationToken ct = default) =>
            Task.FromResult("media-1");

        public Task<string?> GetPermalinkAsync(string mediaId, string accessToken, CancellationToken ct = default) =>
            Task.FromResult<string?>("https://instagram.com/p/abc");
    }

    private sealed class FakeHost : IInstagramMediaHost
    {
        private int _n;
        public List<string> Deleted { get; } = new();

        public Task<string> HostImageAsync(int websiteId, byte[] content, string fileName, Dotnetable.Application.Interfaces.ImageCropRect? crop, CancellationToken ct)
        {
            _n++;
            return Task.FromResult($"https://cdn.test/hosted-{_n}.jpg");
        }

        public Task<string> HostVideoAsync(int websiteId, byte[] content, string fileName, string? mime, CancellationToken ct) =>
            Task.FromResult("https://cdn.test/hosted.mp4");

        public Task<byte[]> DownloadAsync(string url, CancellationToken ct) =>
            Task.FromResult(new byte[] { 1, 2, 3 });

        public Task<byte[]> DownloadAsync(string url, int maxBytes, CancellationToken ct) =>
            DownloadAsync(url, ct);

        public Task DeleteHostedAsync(IReadOnlyCollection<string> urls, CancellationToken ct)
        {
            Deleted.AddRange(urls);
            return Task.CompletedTask;
        }
    }
}
