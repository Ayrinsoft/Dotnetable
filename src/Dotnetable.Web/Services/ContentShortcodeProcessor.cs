using Dotnetable.Application.Security;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Dotnetable.Web.Services;

/// <summary>
/// Expands WordPress-style shortcodes typed directly into Post/Page HTML content. Content authors
/// write <c>[slideshow:ID]</c>, <c>[form:ID]</c> (alias <c>[survey:ID]</c>), or <c>[ads:Header]</c>
/// (any <see cref="Dotnetable.Domain.Enums.AdvertisementLocation"/> name) anywhere inside the
/// body; this replaces every occurrence with the actual rendered widget before the view outputs
/// the content with <c>@Html.Raw</c>.
/// </summary>
public partial class ContentShortcodeProcessor
{
    private readonly ApiClient _api;
    private readonly IHttpContextAccessor _httpContext;

    public ContentShortcodeProcessor(ApiClient api, IHttpContextAccessor httpContext)
    {
        _api = api;
        _httpContext = httpContext;
    }

    [GeneratedRegex(@"\[(slideshow|form|survey):(\d+)\]")]
    private static partial Regex ShortcodePattern();

    [GeneratedRegex(@"\[ads:([A-Za-z]+)\]", RegexOptions.IgnoreCase)]
    private static partial Regex AdsShortcodePattern();

    /// <summary>Replaces every supported shortcode in raw HTML content with rendered markup.
    /// Unknown/inactive ids resolve to an empty string, so a stale shortcode never leaks into the
    /// page as literal text.</summary>
    public async Task<string> ExpandAsync(string? content, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(content)) return content ?? string.Empty;

        // Admin-authored HTML is rendered with @Html.Raw, so it is stripped of scripts and event
        // handlers here — the single point every CMS page, post and home body passes through on its
        // way to a view. Shortcodes are expanded afterwards so the widget markup this class generates
        // itself (which is not user input) is not re-parsed by the sanitizer.
        content = ContentSanitizer.Sanitize(content) ?? string.Empty;

        content = await ExpandAdsAsync(content, ct);

        var matches = ShortcodePattern().Matches(content);
        if (matches.Count == 0) return content;

        var replacements = new Dictionary<string, string>();
        foreach (Match match in matches)
        {
            if (replacements.ContainsKey(match.Value)) continue;
            var kind = match.Groups[1].Value;
            var id = int.Parse(match.Groups[2].Value);

            if (kind == "slideshow")
            {
                var slideshow = await _api.GetSlideshowByIdAsync(id, ct);
                replacements[match.Value] = SlideshowHtmlRenderer.Render(slideshow);
            }
            else // form / survey
            {
                var form = await _api.GetFormByIdAsync(id, ct);
                // Send the visitor back to the page hosting the shortcode after submitting.
                var returnUrl = _httpContext.HttpContext?.Request.Path.Value;
                replacements[match.Value] = FormHtmlRenderer.Render(form, returnUrl);
            }
        }

        return ShortcodePattern().Replace(content, m => replacements[m.Value]);
    }

    private async Task<string> ExpandAdsAsync(string content, CancellationToken ct)
    {
        var matches = AdsShortcodePattern().Matches(content);
        if (matches.Count == 0) return content;

        var lang = _httpContext.HttpContext?.Request.Cookies["lang"];
        var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in matches)
        {
            if (replacements.ContainsKey(match.Value)) continue;
            var location = match.Groups[1].Value;
            var ads = await _api.GetAdvertisementsAsync(location, lang, ct);
            replacements[match.Value] = AdvertisementHtmlRenderer.Render(ads, location);
        }

        return AdsShortcodePattern().Replace(content, m => replacements[m.Value]);
    }
}
