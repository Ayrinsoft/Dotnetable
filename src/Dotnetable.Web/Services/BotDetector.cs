using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Dotnetable.Web.Services;

/// <summary>What kind of automated client, if any, sent a request.</summary>
public enum BotKind
{
    /// <summary>A person's browser (including in-app browsers such as Telegram's or Instagram's).</summary>
    None,

    /// <summary>
    /// A link-preview fetcher (Telegram, WhatsApp, Twitter, Facebook, …). It reads the page's
    /// <c>og:image</c> to build a share card and nothing else, so that one tag is left in place.
    /// </summary>
    LinkPreview,

    /// <summary>A search-engine crawler, SEO tool, AI scraper or plain HTTP library.</summary>
    Crawler,
}

/// <summary>
/// Classifies a request by its User-Agent. Bots still get every page — nothing is hidden from search
/// engines — but <see cref="Infrastructure.BotMediaFilterMiddleware"/> renders it without links to
/// uploaded files, so crawlers never spend the storage CDN's bandwidth.
///
/// <para>In-app browsers are real people and must never match: Telegram's is
/// <c>Telegram-Android/…</c> (its preview bot is <c>TelegramBot</c>), Facebook's carries
/// <c>FBAN/FBAV</c>, and phone brands such as Cubot contain "bot" inside a model name. Those tokens
/// are removed before the bot patterns are tested.</para>
/// </summary>
public static partial class BotDetector
{
    private const string ItemKey = "Dotnetable.BotKind";

    /// <summary>Tokens of real browsers that would otherwise trip the bot patterns.</summary>
    [GeneratedRegex(@"Telegram-(Android|iOS|Desktop)|FBAN|FBAV|FB_IAB|Instagram|Line/|MicroMessenger|Cubot",
        RegexOptions.IgnoreCase)]
    private static partial Regex HumanTokens();

    [GeneratedRegex(@"facebookexternalhit|facebookcatalog|Twitterbot|TelegramBot|WhatsApp|LinkedInBot|Slackbot|Discordbot|SkypeUriPreview|Pinterestbot|redditbot|vkShare|Embedly|Iframely|bitlybot|SnapchatBot",
        RegexOptions.IgnoreCase)]
    private static partial Regex LinkPreviewPattern();

    [GeneratedRegex(@"bot\b|bot/|bot;|crawl|spider|slurp|archiver|scraper|fetcher|headless|lighthouse|pagespeed|gtmetrix|pingdom|preview|mediapartners|adsbot|feedfetcher|python|curl/|wget|httpclient|http-client|okhttp|java/|go-http|libwww|axios|node-fetch|undici|scrapy|phantomjs|puppeteer|playwright|selenium|semrush|ahrefs|mj12|majestic|baiduspider|bytespider|petalbot|sogou|seznam|gptbot|chatgpt|oai-searchbot|claudebot|claude-web|anthropic|perplexity|ccbot|cohere|diffbot|amazonbot|applebot|duckduckbot|ia_archiver|dataforseo|screaming frog",
        RegexOptions.IgnoreCase)]
    private static partial Regex CrawlerPattern();

    /// <summary>The request's <see cref="BotKind"/>, worked out once per request.</summary>
    public static BotKind Classify(HttpContext context)
    {
        if (context.Items.TryGetValue(ItemKey, out var cached) && cached is BotKind known)
            return known;

        var kind = Classify(context.Request.Headers.UserAgent.ToString());
        context.Items[ItemKey] = kind;
        return kind;
    }

    /// <summary>True for any automated client, link-preview fetchers included.</summary>
    public static bool IsBot(HttpContext context) => Classify(context) != BotKind.None;

    public static BotKind Classify(string? userAgent)
    {
        // Every browser sends a User-Agent; an empty one is a script.
        if (string.IsNullOrWhiteSpace(userAgent)) return BotKind.Crawler;

        var ua = HumanTokens().Replace(userAgent, " ");

        if (LinkPreviewPattern().IsMatch(ua)) return BotKind.LinkPreview;
        if (CrawlerPattern().IsMatch(ua)) return BotKind.Crawler;

        // Real browsers all claim Mozilla/…; anything else is a library or a tool.
        return ua.Contains("Mozilla/", StringComparison.OrdinalIgnoreCase) ? BotKind.None : BotKind.Crawler;
    }
}
