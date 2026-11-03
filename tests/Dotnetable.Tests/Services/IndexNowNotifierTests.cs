using System.Net;
using System.Text.Json;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Seo;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Dotnetable.Tests.Services;

public class IndexNowNotifierTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TestDbContextFactory _factory;
    private readonly CaptureHandler _handler = new();

    public IndexNowNotifierTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _factory = new TestDbContextFactory(options);
    }

    [Theory]
    [InlineData("abcdefgh", true)]
    [InlineData("ab", false)]
    [InlineData("has space1", false)]
    [InlineData("key_with_underscore", false)]
    [InlineData("ok-Key-123", true)]
    public void KeyRules_MatchIndexNowGrammar(string key, bool valid) =>
        IndexNowKeyRules.IsValid(key).Should().Be(valid);

    [Fact]
    public void Generate_ProducesValidKey() =>
        IndexNowKeyRules.IsValid(IndexNowKeyRules.Generate()).Should().BeTrue();

    [Theory]
    [InlineData("tvlm.ir", "https://tvlm.ir", "tvlm.ir")]
    [InlineData("https://www.example.com/ignored", "https://www.example.com", "www.example.com")]
    [InlineData("http://shop.local:8080", "http://shop.local:8080", "shop.local:8080")]
    public void Origins_AcceptBareHostAndFullUrl(string address, string origin, string host)
    {
        IndexNowOrigins.TryParse(address, out var parsedOrigin, out var parsedHost).Should().BeTrue();
        parsedOrigin.Should().Be(origin);
        parsedHost.Should().Be(host);
    }

    [Fact]
    public async Task PublishedPage_SubmitsCanonicalUrls()
    {
        var site = await SeedSiteAsync(enabled: true);
        _context.Pages.Add(new Page
        {
            WebsiteID = site.WebsiteID,
            Title = "About",
            Slug = "about us",
            Status = 1,
            IsActive = true,
            IsHomepage = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PageTranslations =
            {
                new PageTranslation { LanguageCode = "fa", Title = "تماس", Slug = "contact-us" },
            },
        });
        await _context.SaveChangesAsync();
        var pageId = _context.Pages.Single().PageID;

        await Notifier().NotifyPageAsync(pageId);

        var body = JsonDocument.Parse(_handler.Body!);
        body.RootElement.GetProperty("host").GetString().Should().Be("tvlm.ir");
        body.RootElement.GetProperty("keyLocation").GetString().Should().Be($"https://tvlm.ir/{site.Key}.txt");
        var urls = body.RootElement.GetProperty("urlList").EnumerateArray().Select(x => x.GetString()).ToList();
        urls.Should().BeEquivalentTo(new[]
        {
            "https://tvlm.ir/page/about%20us",
            "https://tvlm.ir/page/contact-us",
            "https://tvlm.ir/Home/Contact",
        });
    }

    [Fact]
    public async Task Homepage_AlsoSubmitsSiteRoot()
    {
        var site = await SeedSiteAsync(enabled: true);
        var page = new Page
        {
            WebsiteID = site.WebsiteID,
            Title = "Home",
            Slug = "home",
            Status = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _context.Pages.Add(page);
        await _context.SaveChangesAsync();

        await Notifier().NotifyPageAsync(page.PageID);

        var urls = JsonDocument.Parse(_handler.Body!).RootElement.GetProperty("urlList").EnumerateArray()
            .Select(x => x.GetString()).ToList();
        urls.Should().Contain(new[] { "https://tvlm.ir/", "https://tvlm.ir/page/home" });
    }

    [Fact]
    public async Task DraftPage_IsNotSubmitted()
    {
        var site = await SeedSiteAsync(enabled: true);
        var page = new Page
        {
            WebsiteID = site.WebsiteID,
            Title = "Draft",
            Slug = "draft",
            Status = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _context.Pages.Add(page);
        await _context.SaveChangesAsync();

        await Notifier().NotifyPageAsync(page.PageID);

        _handler.Body.Should().BeNull();
    }

    [Fact]
    public async Task DisabledIndexNow_IsNotSubmitted()
    {
        var site = await SeedSiteAsync(enabled: false);
        var post = new Post
        {
            WebsiteID = site.WebsiteID,
            Title = "News",
            Slug = "news",
            Status = PostService.PublishedStatus,
            IsActive = true,
            PublishedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        await Notifier().NotifyPostAsync(post.PostID);

        _handler.Body.Should().BeNull();
    }

    [Fact]
    public async Task ScheduledPost_IsNotSubmittedUntilPublishedAt()
    {
        var site = await SeedSiteAsync(enabled: true);
        var post = new Post
        {
            WebsiteID = site.WebsiteID,
            Title = "Later",
            Slug = "later",
            Status = PostService.PublishedStatus,
            IsActive = true,
            PublishedAt = DateTime.UtcNow.AddDays(2),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        await Notifier().NotifyPostAsync(post.PostID);

        _handler.Body.Should().BeNull();
    }

    [Fact]
    public async Task PublishedProduct_SubmitsShopUrl()
    {
        var site = await SeedSiteAsync(enabled: true);
        var product = new Product
        {
            WebsiteID = site.WebsiteID,
            Title = "Lamp",
            Slug = "lamp",
            Status = ProductService.PublishedStatus,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ProductTranslations =
            {
                new ProductTranslation { LanguageCode = "fa", Title = "چراغ", Slug = "cheragh" },
            },
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        await Notifier().NotifyProductAsync(product.ProductID);

        var urls = JsonDocument.Parse(_handler.Body!).RootElement.GetProperty("urlList").EnumerateArray()
            .Select(x => x.GetString()).ToList();
        urls.Should().BeEquivalentTo(new[]
        {
            "https://tvlm.ir/Shop/Product/lamp",
            "https://tvlm.ir/Shop/Product/cheragh",
        });
        _handler.Last!.RequestUri!.ToString().Should().Be(IndexNowNotifier.Endpoint);
    }

    [Fact]
    public async Task PageCreate_NotifiesAfterSave()
    {
        var site = await SeedSiteAsync(enabled: false);
        var recorder = new RecordingNotifier();
        var pages = new PageService(_factory, recorder);

        var saved = await pages.CreateAsync(new Page
        {
            WebsiteID = site.WebsiteID,
            Title = "Terms",
            Slug = "terms",
            Status = 1,
            IsActive = true,
        });

        recorder.Pages.Should().ContainSingle().Which.Should().Be(saved.PageID);
    }

    [Fact]
    public async Task RemoteFailure_DoesNotThrow()
    {
        var site = await SeedSiteAsync(enabled: true);
        _handler.Status = HttpStatusCode.Forbidden;

        var act = () => Notifier().NotifyPathsAsync(site.WebsiteID, ["/"]);

        await act.Should().NotThrowAsync();
        _handler.Body.Should().NotBeNull();
    }

    public void Dispose() => _context.Dispose();

    private IndexNowNotifier Notifier() =>
        new(_factory, new SingleClientFactory(_handler), NullLogger<IndexNowNotifier>.Instance);

    private async Task<(int WebsiteID, string Key)> SeedSiteAsync(bool enabled)
    {
        var key = IndexNowKeyRules.Generate();
        var website = new Website
        {
            TradeName = "Travel",
            BrandName = "Travel",
            WebsiteAddress = "tvlm.ir",
            AuthCode = Guid.NewGuid(),
            Active = true,
            Manager = "M",
            Mobile = "1",
            Email = "a@tvlm.ir",
            DefaultLanguageCode = "fa",
            DefaultCurrencyCode = "IRR",
            RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
        };
        _context.Websites.Add(website);
        await _context.SaveChangesAsync();
        _context.WebsiteSeoSettings.Add(new WebsiteSeoSetting
        {
            WebsiteID = website.WebsiteID,
            CustomRobotsTxt = string.Empty,
            IndexNowEnabled = enabled,
            IndexNowKey = key,
        });
        await _context.SaveChangesAsync();
        return (website.WebsiteID, key);
    }

    private sealed class RecordingNotifier : IIndexNowNotifier
    {
        public List<int> Pages { get; } = new();

        public Task NotifyPageAsync(int pageId, CancellationToken ct = default)
        {
            Pages.Add(pageId);
            return Task.CompletedTask;
        }

        public Task NotifyPostAsync(int postId, CancellationToken ct = default) => Task.CompletedTask;

        public Task NotifyProductAsync(int productId, CancellationToken ct = default) => Task.CompletedTask;

        public Task NotifyPathsAsync(int websiteId, IReadOnlyCollection<string> relativePaths, CancellationToken ct = default) =>
            Task.CompletedTask;
    }

    private sealed class CaptureHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Last { get; private set; }
        public string? Body { get; private set; }
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Last = request;
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(Status);
        }
    }

    private sealed class SingleClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public SingleClientFactory(HttpMessageHandler handler) => _client = new HttpClient(handler);

        public HttpClient CreateClient(string name) => _client;
    }
}
