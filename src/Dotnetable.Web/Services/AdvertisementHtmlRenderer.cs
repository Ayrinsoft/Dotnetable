using System.Net;
using System.Text;
using Dotnetable.Application.DTOs;

namespace Dotnetable.Web.Services;

/// <summary>
/// Renders keyword advertisements into a list of sponsored links. Used by the
/// <c>_Advertisements</c> partial (fixed theme zones) and by <see cref="ContentShortcodeProcessor"/>
/// (<c>[ads:Location]</c> shortcodes).
/// </summary>
public static class AdvertisementHtmlRenderer
{
    public static string Render(IReadOnlyList<AdvertisementDto>? ads, string? modifier = null)
    {
        if (ads is null || ads.Count == 0) return string.Empty;

        var css = string.IsNullOrWhiteSpace(modifier)
            ? "site-ads"
            : $"site-ads site-ads--{EncodeClass(modifier)}";

        var html = new StringBuilder();
        html.Append($"<nav class=\"{css}\" aria-label=\"Sponsored links\">");
        html.Append("<ul class=\"site-ads-list\">");
        foreach (var ad in ads)
        {
            var href = Encode(ad.Url);
            var text = Encode(ad.Keyword);
            html.Append("<li>");
            html.Append($"<a href=\"{href}\" rel=\"sponsored nofollow");
            if (ad.OpenInNewTab) html.Append(" noopener");
            html.Append('"');
            if (ad.OpenInNewTab) html.Append(" target=\"_blank\"");
            html.Append($">{text}</a>");
            html.Append("</li>");
        }
        html.Append("</ul></nav>");
        return html.ToString();
    }

    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private static string EncodeClass(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            if (char.IsAsciiLetterOrDigit(c) || c is '-' or '_')
                sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }
}
