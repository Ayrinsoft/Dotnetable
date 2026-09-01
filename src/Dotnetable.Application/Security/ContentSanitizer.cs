using Ganss.Xss;

namespace Dotnetable.Application.Security;

/// <summary>
/// Strips scripts and event handlers out of admin-authored rich text before it reaches a page.
///
/// <para>Product descriptions, CMS pages and blog posts are stored as raw HTML and rendered with
/// <c>@Html.Raw</c>, which is what makes the editor useful. It also means whoever can edit that text
/// can run script on every visitor's browser — and "whoever can edit" is not only the owner: vendor
/// members edit their own product content, and a single stolen editor session would otherwise turn
/// into script on the storefront's checkout pages.</para>
///
/// <para>Sanitising on render rather than on save is deliberate. Content already in the database was
/// never filtered, so filtering only new writes would leave the existing rows dangerous; and keeping
/// the original means a policy that turns out too strict can be loosened without data loss.</para>
/// </summary>
public static class ContentSanitizer
{
    private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();

    /// <summary>
    /// Returns <paramref name="html"/> with anything executable removed. Null and blank pass through
    /// unchanged so callers do not have to special-case empty content.
    /// </summary>
    public static string? Sanitize(string? html) =>
        string.IsNullOrWhiteSpace(html) ? html : Sanitizer.Sanitize(html);

    private static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer();

        // The rich-text editor produces media, tables and embeds; the defaults are narrower than what
        // a shop's product page legitimately uses.
        foreach (var tag in new[]
                 {
                     "figure", "figcaption", "picture", "source",
                     "video", "audio", "track",
                     "iframe",
                     "details", "summary", "mark", "small",
                 })
            sanitizer.AllowedTags.Add(tag);

        foreach (var attribute in new[]
                 {
                     "class", "style", "id",
                     "controls", "autoplay", "loop", "muted", "playsinline", "poster", "preload",
                     "srcset", "sizes", "loading", "decoding",
                     "allow", "allowfullscreen", "frameborder", "referrerpolicy",
                     "colspan", "rowspan", "dir", "lang",
                 })
            sanitizer.AllowedAttributes.Add(attribute);

        // data: URIs are how the editor inlines a pasted image. Anything else — javascript:, vbscript:,
        // file: — is denied by omission, which is what neutralises <a href="javascript:...">.
        sanitizer.AllowedSchemes.Clear();
        foreach (var scheme in new[] { "http", "https", "mailto", "tel", "data" })
            sanitizer.AllowedSchemes.Add(scheme);

        // Embedded video is the one iframe use a shop actually needs, and an unrestricted iframe is a
        // phishing surface — an attacker-controlled login form rendered inside a page the visitor
        // trusts — so iframe sources are limited to known embed hosts rather than allowed wholesale.
        sanitizer.PostProcessNode += (_, args) =>
        {
            if (args.Node is not AngleSharp.Html.Dom.IHtmlInlineFrameElement iframe) return;

            if (!IsAllowedEmbed(iframe.GetAttribute("src")))
                iframe.Remove();
        };

        return sanitizer;
    }

    /// <summary>
    /// Hosts whose pages may be embedded in an iframe. Everything else has its iframe dropped by
    /// <see cref="Sanitize"/> before the markup reaches a browser.
    /// </summary>
    private static readonly string[] AllowedEmbedHosts =
    {
        "www.youtube.com", "youtube.com", "www.youtube-nocookie.com", "youtu.be",
        "player.vimeo.com", "vimeo.com",
        "www.aparat.com", "aparat.com",
        "w.soundcloud.com", "open.spotify.com",
        "www.google.com", "maps.google.com",
    };

    /// <summary>True when an iframe pointing at <paramref name="url"/> is safe to keep.</summary>
    public static bool IsAllowedEmbed(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
        && AllowedEmbedHosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase);
}
