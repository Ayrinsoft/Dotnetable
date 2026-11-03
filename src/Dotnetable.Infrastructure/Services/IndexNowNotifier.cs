using System.Net.Http.Json;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Seo;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Submits changed public URLs to the shared IndexNow endpoint. One submission is shared with
/// every participating engine, including Bing. Google does not implement IndexNow.
/// </summary>
public sealed class IndexNowNotifier : IIndexNowNotifier
{
    public const string HttpClientName = "IndexNow";
    public const string Endpoint = "https://api.indexnow.org/indexnow";

    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IndexNowNotifier> _logger;

    public IndexNowNotifier(
        IDbContextFactory<AppDbContext> contextFactory,
        IHttpClientFactory httpClientFactory,
        ILogger<IndexNowNotifier> logger)
    {
        _contextFactory = contextFactory;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task NotifyPageAsync(int pageId, CancellationToken ct = default)
    {
        try
        {
            await using var db = await _contextFactory.CreateDbContextAsync(ct);
            var page = await db.Pages.AsNoTracking()
                .Include(p => p.PageTranslations)
                .FirstOrDefaultAsync(p => p.PageID == pageId, ct);
            if (page is null || !page.IsActive || page.Status != 1) return;

            var paths = new List<string>();
            AddPagePaths(paths, page.Slug, page.IsHomepage);
            foreach (var translation in page.PageTranslations)
                AddPagePaths(paths, translation.Slug, homepage: false);

            await SubmitAsync(db, page.WebsiteID, paths, ct);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "IndexNow notification failed for page {PageId}.", pageId);
        }
    }

    public async Task NotifyPostAsync(int postId, CancellationToken ct = default)
    {
        try
        {
            await using var db = await _contextFactory.CreateDbContextAsync(ct);
            var post = await db.Posts.AsNoTracking()
                .Include(p => p.PostTranslations)
                .FirstOrDefaultAsync(p => p.PostID == postId, ct);
            if (!IsPublicPost(post)) return;

            var paths = new List<string>();
            AddSlugPath(paths, "/Blog/Post/", post!.Slug);
            foreach (var translation in post.PostTranslations)
                AddSlugPath(paths, "/Blog/Post/", translation.Slug);

            await SubmitAsync(db, post.WebsiteID, paths, ct);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "IndexNow notification failed for post {PostId}.", postId);
        }
    }

    public async Task NotifyProductAsync(int productId, CancellationToken ct = default)
    {
        try
        {
            await using var db = await _contextFactory.CreateDbContextAsync(ct);
            var product = await db.Products.AsNoTracking()
                .Include(p => p.ProductTranslations)
                .FirstOrDefaultAsync(p => p.ProductID == productId, ct);
            if (product is null || !product.IsActive || product.Status != ProductService.PublishedStatus) return;

            var paths = new List<string>();
            AddSlugPath(paths, "/Shop/Product/", product.Slug);
            foreach (var translation in product.ProductTranslations)
                AddSlugPath(paths, "/Shop/Product/", translation.Slug);

            await SubmitAsync(db, product.WebsiteID, paths, ct);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "IndexNow notification failed for product {ProductId}.", productId);
        }
    }

    public async Task NotifyPathsAsync(int websiteId, IReadOnlyCollection<string> relativePaths, CancellationToken ct = default)
    {
        try
        {
            await using var db = await _contextFactory.CreateDbContextAsync(ct);
            await SubmitAsync(db, websiteId, relativePaths, ct);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "IndexNow notification failed for website {WebsiteId}.", websiteId);
        }
    }

    private async Task SubmitAsync(AppDbContext db, int websiteId, IReadOnlyCollection<string> relativePaths, CancellationToken ct)
    {
        var paths = relativePaths
            .Where(p => !string.IsNullOrWhiteSpace(p) && p.StartsWith('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (paths.Count == 0) return;

        var seo = await db.WebsiteSeoSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.WebsiteID == websiteId, ct);
        if (seo is not { IndexNowEnabled: true }) return;

        var key = seo.IndexNowKey?.Trim();
        if (!IndexNowKeyRules.IsValid(key)) return;

        var address = await db.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId)
            .Select(w => w.WebsiteAddress)
            .FirstOrDefaultAsync(ct);
        if (!IndexNowOrigins.TryParse(address, out var origin, out var host))
        {
            _logger.LogWarning("IndexNow skipped for website {WebsiteId}: WebsiteAddress is not a public host.", websiteId);
            return;
        }

        var urls = paths.Select(p => origin + p).ToList();
        var payload = new
        {
            host,
            key,
            keyLocation = $"{origin}/{key}.txt",
            urlList = urls,
        };

        var client = _httpClientFactory.CreateClient(HttpClientName);
        using var response = await client.PostAsJsonAsync(Endpoint, payload, ct);
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("IndexNow accepted {Count} URL(s) for {Host} with status {StatusCode}.", urls.Count, host, (int)response.StatusCode);
            return;
        }

        _logger.LogWarning("IndexNow rejected {Count} URL(s) for {Host} with status {StatusCode}.", urls.Count, host, (int)response.StatusCode);
    }

    private static bool IsPublicPost(Post? post)
    {
        if (post is null || !post.IsActive || post.Status != PostService.PublishedStatus) return false;
        return post.PublishedAt is null || post.PublishedAt <= DateTime.UtcNow;
    }

    private static void AddPagePaths(List<string> paths, string? slug, bool homepage)
    {
        AddSlugPath(paths, "/page/", slug);
        if (homepage || string.Equals(slug, "home", StringComparison.OrdinalIgnoreCase))
            paths.Add("/");
        if (string.Equals(slug, "contact-us", StringComparison.OrdinalIgnoreCase))
            paths.Add("/Home/Contact");
    }

    private static void AddSlugPath(List<string> paths, string prefix, string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return;
        paths.Add(prefix + Uri.EscapeDataString(slug.Trim()));
    }
}

/// <summary>Used when a service is constructed without a notifier, including unit tests.</summary>
public sealed class NullIndexNowNotifier : IIndexNowNotifier
{
    public static readonly NullIndexNowNotifier Instance = new();

    public Task NotifyPageAsync(int pageId, CancellationToken ct = default) => Task.CompletedTask;

    public Task NotifyPostAsync(int postId, CancellationToken ct = default) => Task.CompletedTask;

    public Task NotifyProductAsync(int productId, CancellationToken ct = default) => Task.CompletedTask;

    public Task NotifyPathsAsync(int websiteId, IReadOnlyCollection<string> relativePaths, CancellationToken ct = default) =>
        Task.CompletedTask;
}
