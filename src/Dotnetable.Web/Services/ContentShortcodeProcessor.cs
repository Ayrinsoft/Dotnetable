using System.Text.RegularExpressions;

namespace Dotnetable.Web.Services;

/// <summary>
/// Expands WordPress-style shortcodes typed directly into Post/Page HTML content. Content authors
/// write <c>[slideshow:ID]</c> anywhere inside the body (copied from the admin's Slideshows page);
/// this replaces every occurrence with the actual rendered slideshow before the view outputs the
/// content with <c>@Html.Raw</c>.
/// </summary>
public partial class ContentShortcodeProcessor
{
    private readonly ApiClient _api;

    public ContentShortcodeProcessor(ApiClient api) => _api = api;

    [GeneratedRegex(@"\[slideshow:(\d+)\]")]
    private static partial Regex ShortcodePattern();

    /// <summary>Replaces every <c>[slideshow:ID]</c> shortcode in raw HTML content with the rendered
    /// slideshow markup. Unknown/inactive ids resolve to an empty string, so a stale shortcode never
    /// leaks into the page as literal text.</summary>
    public async Task<string> ExpandAsync(string? content, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(content)) return content ?? string.Empty;

        var matches = ShortcodePattern().Matches(content);
        if (matches.Count == 0) return content;

        var replacements = new Dictionary<string, string>();
        foreach (Match match in matches)
        {
            if (replacements.ContainsKey(match.Value)) continue;
            var id = int.Parse(match.Groups[1].Value);
            var slideshow = await _api.GetSlideshowByIdAsync(id, ct);
            replacements[match.Value] = SlideshowHtmlRenderer.Render(slideshow);
        }

        return ShortcodePattern().Replace(content, m => replacements[m.Value]);
    }
}
