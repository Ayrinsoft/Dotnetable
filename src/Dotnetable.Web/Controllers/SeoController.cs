using System.Text;
using System.Xml;
using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

/// <summary>
/// The two files every crawler asks for before it reads anything else. Both are generated rather than
/// static, because the URLs that matter — products, categories, CMS pages, posts — are database rows
/// the shop owner adds without touching the deployment.
/// </summary>
public class SeoController : Controller
{
    /// <summary>Sitemaps cap at 50,000 URLs; this stays well under while covering a normal catalogue.</summary>
    private const int MaxUrlsPerSection = 5000;

    private readonly ApiClient _api;

    public SeoController(ApiClient api) => _api = api;

    /// <summary>
    /// <c>/robots.txt</c>. Account, cart and checkout paths are disallowed: they are per-visitor, never
    /// useful in an index, and crawling them burns crawl budget on pages that cannot rank.
    /// </summary>
    [HttpGet("/robots.txt")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public IActionResult Robots()
    {
        var baseUrl = BaseUrl();
        var body = $"""
            User-agent: *
            Disallow: /Account/
            Disallow: /Cart/
            Disallow: /Checkout/
            Disallow: /Home/StatusCode
            Disallow: /Home/Error
            Allow: /

            Sitemap: {baseUrl}/sitemap.xml
            """;

        return Content(body, "text/plain", Encoding.UTF8);
    }

    /// <summary>
    /// <c>/sitemap.xml</c>. Built from the same public API the pages themselves render from, so a URL
    /// can never appear here that a visitor cannot actually open.
    /// </summary>
    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Sitemap(CancellationToken ct = default)
    {
        var baseUrl = BaseUrl();
        var settings = new XmlWriterSettings { Async = true, Indent = true, Encoding = new UTF8Encoding(false) };

        await using var buffer = new MemoryStream();
        await using (var writer = XmlWriter.Create(buffer, settings))
        {
            await writer.WriteStartDocumentAsync();
            await writer.WriteStartElementAsync(null, "urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

            await WriteUrlAsync(writer, baseUrl, null, "daily", "1.0");

            await WriteUrlAsync(writer, $"{baseUrl}/Shop", null, "daily", "0.9");
            await WriteUrlAsync(writer, $"{baseUrl}/price-lists", null, "daily", "0.8");
            await WriteUrlAsync(writer, $"{baseUrl}/Blog", null, "daily", "0.7");

            // Categories change rarely but anchor the crawl, so they rank above individual products.
            var categories = await SafeAsync(async () =>
                (IReadOnlyList<string>)(await _api.GetCategoriesAsync(null, null, ct))
                    .Select(c => c.Slug).Where(s => !string.IsNullOrWhiteSpace(s)).ToList());

            foreach (var slug in categories.Take(MaxUrlsPerSection))
                await WriteUrlAsync(writer, $"{baseUrl}/Shop?category={Uri.EscapeDataString(slug)}", null, "weekly", "0.8");

            // Paged so a large catalogue is fully covered rather than truncated at the first page.
            var productSlugs = await SafeAsync(() => CollectAsync(
                async page => (await _api.GetProductsAsync(page: page, pageSize: 100, ct: ct)).Items.Select(p => p.Slug)));

            foreach (var slug in productSlugs)
                await WriteUrlAsync(writer, $"{baseUrl}/Shop/Product/{Uri.EscapeDataString(slug)}", null, "weekly", "0.7");

            var priceLists = await SafeAsync(async () => await _api.GetPriceListsAsync(ct));
            foreach (var list in priceLists.Take(MaxUrlsPerSection))
                await WriteUrlAsync(writer, $"{baseUrl}/price-lists/{Uri.EscapeDataString(list.Slug)}", list.LastUpdatedAt, "daily", "0.8");

            var postSlugs = await SafeAsync(() => CollectAsync(
                async page => (await _api.GetPostsAsync(page: page, pageSize: 100, ct: ct)).Items.Select(p => p.Slug)));

            foreach (var slug in postSlugs)
                await WriteUrlAsync(writer, $"{baseUrl}/Blog/Post/{Uri.EscapeDataString(slug)}", null, "monthly", "0.6");

            await writer.WriteEndElementAsync();
            await writer.WriteEndDocumentAsync();
        }

        return File(buffer.ToArray(), "application/xml");
    }

    private static async Task WriteUrlAsync(XmlWriter writer, string location, DateTime? lastModified,
        string changeFrequency, string priority)
    {
        await writer.WriteStartElementAsync(null, "url", null);
        await writer.WriteElementStringAsync(null, "loc", null, location);
        if (lastModified is DateTime when)
            await writer.WriteElementStringAsync(null, "lastmod", null, when.ToString("yyyy-MM-dd"));
        await writer.WriteElementStringAsync(null, "changefreq", null, changeFrequency);
        await writer.WriteElementStringAsync(null, "priority", null, priority);
        await writer.WriteEndElementAsync();
    }

    /// <summary>
    /// Walks a paged endpoint until it runs dry or hits <see cref="MaxUrlsPerSection"/>. A shop with
    /// 4,000 products would otherwise get 100 of them indexed.
    /// </summary>
    private static async Task<IReadOnlyList<string>> CollectAsync(Func<int, Task<IEnumerable<string>>> loadPage)
    {
        var slugs = new List<string>();

        for (var page = 1; slugs.Count < MaxUrlsPerSection; page++)
        {
            var batch = (await loadPage(page)).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            if (batch.Count == 0) break;

            slugs.AddRange(batch);
            if (batch.Count < 100) break;
        }

        return slugs.Distinct(StringComparer.Ordinal).Take(MaxUrlsPerSection).ToList();
    }

    /// <summary>
    /// A sitemap that 500s is worse than a partial one — crawlers back off after repeated failures —
    /// so an unreachable API degrades to fewer URLs rather than no file.
    /// </summary>
    private static async Task<IReadOnlyList<T>> SafeAsync<T>(Func<Task<IReadOnlyList<T>>> load)
    {
        try
        {
            return await load();
        }
        catch (Exception)
        {
            return Array.Empty<T>();
        }
    }

    private string BaseUrl() => $"{Request.Scheme}://{Request.Host}".TrimEnd('/');
}
