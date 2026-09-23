using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace Dotnetable.Web.Services;

/// <summary>
/// Keeps uploaded files from costing storage-CDN bandwidth until a visitor actually looks at them.
///
/// <para><see cref="PrepareForVisitors"/> rewrites admin-authored HTML (post, page, product and author
/// bodies) so nothing is fetched on load: images get their small <c>{name}_thumb.webp</c> through
/// <c>data-src</c> and open the original only on click, videos become a play button that builds the
/// <c>&lt;video&gt;</c> on click, audio never preloads, and embedded documents turn into a link.
/// <c>wwwroot/js/media.js</c> loads each <c>data-src</c> once it scrolls into view, so hidden tabs,
/// albums and carousel slides stay unloaded until shown.</para>
///
/// <para><see cref="StripForBots"/> is the crawler side: the same page with every link to an uploaded
/// file removed (see <see cref="Infrastructure.BotMediaFilterMiddleware"/>).</para>
/// </summary>
public static partial class MediaHtml
{
    /// <summary>Transparent 1×1 GIF: a valid <c>src</c> until the real one is swapped in.</summary>
    public const string Placeholder = "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7";

    /// <summary>
    /// Every upload is stored as a 32-hex GUID plus its extension (thumbnails add <c>_thumb</c>, the
    /// older ones a <c>t_</c> prefix), on whichever CDN host the storage uses — so the name alone says
    /// "this is one of ours" without the site having to know the storage hosts.
    /// </summary>
    [GeneratedRegex(@"^(t_)?[0-9a-f]{32}(_thumb)?\.[a-z0-9]{2,5}$", RegexOptions.IgnoreCase)]
    private static partial Regex StoredNamePattern();

    [GeneratedRegex(@"url\(\s*(['""]?)(?<url>[^'""\)]+)\1\s*\)", RegexOptions.IgnoreCase)]
    private static partial Regex CssUrlPattern();

    /// <summary>Originals that have a <c>{name}_thumb.webp</c> next to them (see <c>ImageThumbnailer</c>).</summary>
    private static readonly HashSet<string> ThumbnailedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp",
    };

    private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv", ".ppt", ".pptx", ".txt", ".rtf",
        ".odt", ".ods", ".odp", ".zip", ".rar", ".7z", ".gz", ".tar",
    };

    /// <summary>Attributes that may carry a file URL on any element (the lazy markup included).</summary>
    private static readonly string[] UrlAttributes =
    [
        "src", "href", "poster", "data", "data-src", "data-full", "data-fallback", "data-bg", "data-poster",
        "content",
    ];

    private static readonly HtmlParser Parser = new();

    // ── Visitors ─────────────────────────────────────────────

    /// <summary>Rewrites content HTML so no uploaded media is downloaded before it is shown or clicked.</summary>
    public static string PrepareForVisitors(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return html ?? string.Empty;

        var document = Parser.ParseDocument("<!DOCTYPE html><html><body></body></html>");
        var body = document.Body!;
        body.InnerHtml = html;

        foreach (var video in body.QuerySelectorAll("video").ToList())
            ReplaceVideo(document, video);

        foreach (var audio in body.QuerySelectorAll("audio"))
        {
            audio.SetAttribute("preload", "none");
            audio.RemoveAttribute("autoplay");
        }

        foreach (var img in body.QuerySelectorAll("img"))
            MakeLazy(img);

        foreach (var frame in body.QuerySelectorAll("iframe, embed, object").ToList())
        {
            var url = frame.GetAttribute(frame.LocalName == "object" ? "data" : "src");
            if (IsUploadedFile(url))
                frame.Replace(DocumentLink(document, url!, frame.GetAttribute("title")));
            else if (frame.LocalName == "iframe")
                frame.SetAttribute("loading", "lazy");
        }

        foreach (var link in body.QuerySelectorAll("a[href]"))
        {
            var href = link.GetAttribute("href");
            if (!IsUploadedFile(href)) continue;

            link.SetAttribute("rel", "nofollow noopener");
            if (DocumentExtensions.Contains(Extension(href!)) && link.QuerySelector("img, i.bi") is null)
            {
                link.ClassList.Add("dn-doc-link");
                link.Prepend(Icon(document, href!));
            }
        }

        foreach (var styled in body.QuerySelectorAll("[style]"))
            MoveBackgroundToLazy(styled);

        return body.InnerHtml;
    }

    /// <summary>Swaps an image to the lazy form; the original becomes the click-to-open lightbox source.</summary>
    private static void MakeLazy(IElement img)
    {
        var src = img.GetAttribute("src");
        if (img.HasAttribute("data-src") || string.IsNullOrWhiteSpace(src) || src.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            return;

        var thumb = ThumbnailFor(src);
        img.SetAttribute("data-src", thumb ?? src);
        img.SetAttribute("src", Placeholder);
        img.SetAttribute("loading", "lazy");
        img.SetAttribute("decoding", "async");
        img.ClassList.Add("dn-lazy");

        var srcset = img.GetAttribute("srcset");
        img.RemoveAttribute("srcset");
        img.RemoveAttribute("sizes");
        // A responsive set only makes sense for the original; with a thumbnail shown it would pull the big files back in.
        if (thumb is null && !string.IsNullOrWhiteSpace(srcset))
            img.SetAttribute("data-srcset", srcset);

        if (thumb is null) return;

        // Old uploads may not have their thumbnail yet (the backfill is gradual): media.js falls back to the original.
        img.SetAttribute("data-fallback", src);
        if (img.Closest("a") is null)
        {
            img.ClassList.Add("dn-zoom");
            img.SetAttribute("data-full", src);
        }
    }

    /// <summary>
    /// Replaces a <c>&lt;video&gt;</c> with a play button. The original element is parked in an inert
    /// <c>&lt;template&gt;</c> (a browser never fetches anything inside one) and cloned in on click.
    /// </summary>
    private static void ReplaceVideo(IDocument document, IElement video)
    {
        video.RemoveAttribute("autoplay");
        video.SetAttribute("controls", "controls");
        video.SetAttribute("preload", "metadata");
        video.SetAttribute("playsinline", "playsinline");

        var poster = video.GetAttribute("poster");
        if (ThumbnailFor(poster) is { } posterThumb)
            video.SetAttribute("poster", posterThumb);

        var wrapper = document.CreateElement("div");
        wrapper.ClassName = "dn-video";
        wrapper.SetAttribute("role", "button");
        wrapper.SetAttribute("tabindex", "0");
        wrapper.SetAttribute("aria-label", video.GetAttribute("title") ?? "Play video");
        var width = video.GetAttribute("width");
        if (!string.IsNullOrWhiteSpace(width) && int.TryParse(width, out var w) && w > 0)
            wrapper.SetAttribute("style", $"max-width:{w}px");

        if (!string.IsNullOrWhiteSpace(poster) && !poster.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var img = document.CreateElement("img");
            img.SetAttribute("src", poster);
            img.SetAttribute("alt", "");
            img.ClassName = "dn-video-poster";
            MakeLazy(img);
            img.ClassList.Remove("dn-zoom");
            img.RemoveAttribute("data-full");
            wrapper.AppendChild(img);
        }

        var play = document.CreateElement("span");
        play.ClassName = "dn-video-play";
        play.InnerHtml = "<i class=\"bi bi-play-fill\" aria-hidden=\"true\"></i>";
        wrapper.AppendChild(play);

        var template = (IHtmlTemplateElement)document.CreateElement("template");
        video.Replace(wrapper);
        template.Content.AppendChild(video);
        wrapper.AppendChild(template);
    }

    private static IElement DocumentLink(IDocument document, string url, string? title)
    {
        var link = document.CreateElement("a");
        link.SetAttribute("href", url);
        link.SetAttribute("target", "_blank");
        link.SetAttribute("rel", "nofollow noopener");
        link.ClassName = "dn-doc-link";
        link.AppendChild(Icon(document, url));
        link.AppendChild(document.CreateTextNode(string.IsNullOrWhiteSpace(title) ? FileLabel(url) : title));

        var paragraph = document.CreateElement("p");
        paragraph.AppendChild(link);
        return paragraph;
    }

    private static IElement Icon(IDocument document, string url)
    {
        var icon = document.CreateElement("i");
        icon.ClassName = "bi " + Extension(url).ToLowerInvariant() switch
        {
            ".pdf" => "bi-file-earmark-pdf",
            ".doc" or ".docx" or ".odt" or ".rtf" or ".txt" => "bi-file-earmark-text",
            ".xls" or ".xlsx" or ".ods" or ".csv" => "bi-file-earmark-spreadsheet",
            ".ppt" or ".pptx" or ".odp" => "bi-file-earmark-slides",
            ".zip" or ".rar" or ".7z" or ".gz" or ".tar" => "bi-file-earmark-zip",
            _ => "bi-file-earmark",
        } + " me-1";
        icon.SetAttribute("aria-hidden", "true");
        return icon;
    }

    /// <summary>Inline <c>background-image</c> pointing at an upload moves to <c>data-bg</c> for media.js.</summary>
    private static void MoveBackgroundToLazy(IElement element)
    {
        var style = element.GetAttribute("style");
        if (string.IsNullOrEmpty(style) || element.HasAttribute("data-bg")) return;

        var match = CssUrlPattern().Matches(style).FirstOrDefault(m => IsUploadedFile(m.Groups["url"].Value));
        if (match is null) return;

        var url = match.Groups["url"].Value.Trim();
        element.SetAttribute("data-bg", ThumbnailFor(url) ?? url);
        element.SetAttribute("style", CssUrlPattern().Replace(style, m => IsUploadedFile(m.Groups["url"].Value) ? "none" : m.Value));
        element.ClassList.Add("dn-lazy-bg");
    }

    // ── Bots ─────────────────────────────────────────────────

    /// <summary>
    /// Returns a whole HTML page with every link to an uploaded file removed, for crawlers: media
    /// elements go, images go, links to files keep their text but lose the link, and any attribute
    /// or inline style still pointing at an upload is dropped. The page itself stays complete and
    /// indexable. <paramref name="keepShareImage"/> leaves <c>og:image</c> for link-preview fetchers.
    /// </summary>
    public static string StripForBots(string html, bool keepShareImage)
    {
        var document = Parser.ParseDocument(html);

        foreach (var element in document.QuerySelectorAll("template, video, audio, source, track, picture, embed, object").ToList())
            element.Remove();

        // Theme artwork and third-party images are not ours to save, so only uploads go.
        foreach (var element in document.QuerySelectorAll("img, iframe").ToList())
        {
            if (new[] { "src", "data-src", "data-full", "data-fallback" }.Any(a => IsUploadedFile(element.GetAttribute(a))))
                element.Remove();
            else if (element.GetAttribute("data-src") is { Length: > 0 } src)
                element.SetAttribute("src", src); // crawlers do not run media.js
        }

        foreach (var link in document.QuerySelectorAll("a[href]").ToList())
        {
            if (!IsUploadedFile(link.GetAttribute("href"))) continue;
            if (string.IsNullOrWhiteSpace(link.TextContent))
                link.Remove();
            else
                link.Replace(link.ChildNodes.ToArray());
        }

        foreach (var element in document.All.ToList())
        {
            // The favicon is tiny and is what search results show next to the site name.
            if (element.LocalName == "link") continue;

            var isMeta = element.LocalName == "meta";
            if (isMeta && keepShareImage) continue;

            foreach (var name in UrlAttributes)
            {
                if (IsUploadedFile(element.GetAttribute(name)))
                {
                    // A share-card tag without its image is just noise, so drop the whole tag.
                    if (isMeta) { element.Remove(); break; }
                    element.RemoveAttribute(name);
                }
            }

            foreach (var name in new[] { "srcset", "data-srcset" })
                element.RemoveAttribute(name);

            var style = element.GetAttribute("style");
            if (!string.IsNullOrEmpty(style) && style.Contains("url(", StringComparison.OrdinalIgnoreCase))
                element.SetAttribute("style", CssUrlPattern().Replace(style, m => IsUploadedFile(m.Groups["url"].Value) ? "none" : m.Value));
        }

        return document.ToHtml(HtmlMarkupFormatter.Instance);
    }

    // ── URLs ─────────────────────────────────────────────────

    /// <summary>True for a URL of a file uploaded through the media library, on any storage host.</summary>
    public static bool IsUploadedFile(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        url = url.Trim();
        if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) return false;

        var path = PathOf(url);
        if (path.Contains("/api/files/", StringComparison.OrdinalIgnoreCase)) return true;

        var name = path[(path.LastIndexOf('/') + 1)..];
        return StoredNamePattern().IsMatch(name);
    }

    /// <summary>
    /// The <c>{name}_thumb.webp</c> next to an uploaded jpg/png/webp original, or null when the URL is
    /// not an upload, is already a thumbnail, or its type never gets one.
    /// </summary>
    public static string? ThumbnailFor(string? url)
    {
        if (!IsUploadedFile(url)) return null;

        var path = PathOf(url!.Trim());
        var name = path[(path.LastIndexOf('/') + 1)..];
        var stem = Path.GetFileNameWithoutExtension(name);
        if (stem.StartsWith("t_", StringComparison.OrdinalIgnoreCase)
            || stem.EndsWith("_thumb", StringComparison.OrdinalIgnoreCase)
            || !ThumbnailedExtensions.Contains(Path.GetExtension(name)))
            return null;

        return path[..(path.Length - name.Length)] + stem + "_thumb.webp";
    }

    /// <summary>The URL without its query string or fragment.</summary>
    private static string PathOf(string url)
    {
        var end = url.IndexOfAny(['?', '#']);
        return end < 0 ? url : url[..end];
    }

    private static string Extension(string url) => Path.GetExtension(PathOf(url));

    private static string FileLabel(string url)
    {
        var ext = Extension(url).TrimStart('.').ToUpperInvariant();
        return string.IsNullOrEmpty(ext) ? "Download file" : $"Download {ext}";
    }
}
