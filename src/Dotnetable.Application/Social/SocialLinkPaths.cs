using Dotnetable.Application.Instagram;
using Dotnetable.Application.Seo;

namespace Dotnetable.Application.Social;

/// <summary>
/// Public storefront paths appended to Instagram, Telegram, and X captions.
/// An empty saved path keeps the route the storefront already serves.
/// A shorter path is redirected there.
/// </summary>
public static class SocialLinkPaths
{
    public const string SlugToken = "{slug}";
    public const string DefaultPost = "Blog/Post/{slug}";
    public const string DefaultPage = "page/{slug}";
    public const string DefaultProduct = "Shop/Product/{slug}";
    public const string DefaultCategory = "Blog/Category/{slug}";

    public static string Effective(string? stored, string fallback) =>
        string.IsNullOrWhiteSpace(stored) ? fallback : stored.Trim().Trim('/');

    public static string? NormalizeBase(string? raw)
    {
        var text = raw?.Trim();
        if (string.IsNullOrEmpty(text)) return null;
        if (!text.Contains("://", StringComparison.Ordinal))
            text = "https://" + text;
        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || string.IsNullOrEmpty(uri.IdnHost))
            throw new SocialPublishException("The site address must be an http or https address.");

        var path = uri.AbsolutePath.TrimEnd('/');
        if (path == "/") path = "";
        var host = uri.IsDefaultPort ? uri.IdnHost : $"{uri.IdnHost}:{uri.Port}";
        return $"{uri.Scheme}://{host}{path}";
    }

    /// <summary>Blank means “use the default”. Anything else must be a relative path with one {slug}.</summary>
    public static string? NormalizePath(string? raw)
    {
        var text = raw?.Trim().Trim('/');
        if (string.IsNullOrEmpty(text)) return null;
        if (text.Contains('\\') || text.Contains('?') || text.Contains('#') || text.Contains(' '))
            throw new SocialPublishException("A public path cannot contain spaces, ?, #, or backslashes.");

        var parts = text.Split('/');
        var slugCount = 0;
        if (parts.Length < 2)
            throw new SocialPublishException("A public path needs a folder and {slug}, for example p/{slug}.");

        foreach (var part in parts)
        {
            if (part.Equals(SlugToken, StringComparison.OrdinalIgnoreCase))
            {
                slugCount++;
                continue;
            }

            if (part.Length == 0 || part is "." or "..")
                throw new SocialPublishException("A public path cannot contain empty or parent segments.");
            foreach (var ch in part)
            {
                if (ch is not ((>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '-' or '_'))
                    throw new SocialPublishException("A public path may use letters, digits, hyphen, and underscore, plus {slug}.");
            }
        }

        if (slugCount != 1 || !parts[^1].Equals(SlugToken, StringComparison.OrdinalIgnoreCase))
            throw new SocialPublishException("Put {slug} once, at the end of the path.");
        if (text.Length > 80)
            throw new SocialPublishException("A public path must be 80 characters or fewer.");
        return text;
    }

    public static void EnsureDistinct(params string?[] storedOrEmpty)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in storedOrEmpty)
        {
            var key = string.IsNullOrWhiteSpace(raw) ? "" : raw.Trim().Trim('/');
            if (key.Length == 0) continue;
            if (!seen.Add(key))
                throw new SocialPublishException("Each public path must be different.");
        }
    }

    public static string? Build(string? websiteAddress, string? baseUrl, string? template, string fallbackTemplate, string? slug)
    {
        var clean = slug?.Trim();
        if (string.IsNullOrEmpty(clean)) return null;

        var origin = NormalizeBase(baseUrl);
        if (origin is null && !IndexNowOrigins.TryParse(websiteAddress, out origin, out _))
            return null;

        var path = Effective(template, fallbackTemplate)
            .Replace(SlugToken, Uri.EscapeDataString(clean), StringComparison.OrdinalIgnoreCase);
        return origin!.TrimEnd('/') + "/" + path.TrimStart('/');
    }

    /// <summary>
    /// When the request is a configured short path, returns the storefront route that already renders it.
    /// The same path as that route returns null so the page is not redirected to itself.
    /// </summary>
    public static string? Redirect(string? requestPath, SocialLinkMap map)
    {
        var path = (requestPath ?? "").Split('?', 2)[0].Trim();
        if (path.Length == 0) return null;
        if (!path.StartsWith('/')) path = "/" + path;
        if (path.Length > 1) path = path.TrimEnd('/');

        return Try(path, map.PostPath, DefaultPost)
            ?? Try(path, map.PagePath, DefaultPage)
            ?? Try(path, map.ProductPath, DefaultProduct)
            ?? Try(path, map.CategoryPath, DefaultCategory);
    }

    private static string? Try(string path, string template, string canonical)
    {
        var wanted = Effective(template, canonical).Split('/');
        var actual = path.Trim('/').Split('/');
        if (wanted.Length != actual.Length) return null;

        string? slug = null;
        for (var i = 0; i < wanted.Length; i++)
        {
            if (wanted[i].Equals(SlugToken, StringComparison.OrdinalIgnoreCase))
            {
                if (!IsSlug(actual[i])) return null;
                slug = Uri.UnescapeDataString(actual[i]);
                continue;
            }

            if (!wanted[i].Equals(actual[i], StringComparison.OrdinalIgnoreCase)) return null;
        }

        if (slug is null || Same(template, canonical)) return null;
        var target = canonical.Replace(SlugToken, Uri.EscapeDataString(slug), StringComparison.OrdinalIgnoreCase);
        return "/" + target.Trim('/');
    }

    private static bool Same(string template, string canonical) =>
        Effective(template, canonical).Equals(canonical, StringComparison.OrdinalIgnoreCase);

    private static bool IsSlug(string segment) =>
        segment.Length is > 0 and <= 200 && segment is not "." and not "..";
}

public sealed class SocialLinkMap
{
    public string PostPath { get; init; } = SocialLinkPaths.DefaultPost;
    public string PagePath { get; init; } = SocialLinkPaths.DefaultPage;
    public string ProductPath { get; init; } = SocialLinkPaths.DefaultProduct;
    public string CategoryPath { get; init; } = SocialLinkPaths.DefaultCategory;
}
