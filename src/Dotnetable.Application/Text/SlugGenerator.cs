using System.Text.RegularExpressions;

namespace Dotnetable.Application.Text;

/// <summary>Turns an admin-entered title into a URL-safe slug. Used everywhere a Category/Post/Page/
/// Tag (or one of their per-language translations) is created or updated — including the API/service
/// layer itself, not just the admin UI — so a raw, space-containing title can never reach the
/// database and end up percent-encoded in a public URL.</summary>
public static class SlugGenerator
{
    /// <summary>Any run of characters that isn't a letter (any script — Latin, Persian, Arabic, ...)
    /// or a digit becomes a single "-". No transliteration: Persian/Arabic letters and digits pass
    /// through unchanged.</summary>
    private static readonly Regex UnsafeRun = new(@"[^\p{L}\p{Nd}]+", RegexOptions.Compiled);

    /// <summary>Normalizes <paramref name="title"/> into a slug, trimmed to fit
    /// <paramref name="maxLength"/> (the column's max length) with room left for a uniqueness
    /// suffix. Returns "item" for a title that collapses to nothing (e.g. all punctuation).</summary>
    public static string Normalize(string? title, int maxLength = 200)
    {
        var trimmed = (title ?? string.Empty).Trim();
        if (trimmed.Length == 0) return "item";

        var slug = UnsafeRun.Replace(trimmed, "-").Trim('-').ToLowerInvariant();
        if (slug.Length == 0) return "item";

        // Leave headroom for MakeUnique's "-NN" suffix so the final slug never exceeds maxLength.
        var budget = Math.Max(1, maxLength - 4);
        if (slug.Length > budget)
            slug = slug[..budget].Trim('-');

        return slug.Length == 0 ? "item" : slug;
    }

    /// <summary>Appends "-2", "-3", ... to <paramref name="baseSlug"/> until it no longer collides
    /// with anything in <paramref name="existingSlugs"/> (case-insensitive) — callers should use a
    /// case-insensitive set and add each slug they assign so a single batch of writes never
    /// hands out the same slug twice.</summary>
    public static string MakeUnique(string baseSlug, ISet<string> existingSlugs)
    {
        if (!existingSlugs.Contains(baseSlug)) return baseSlug;

        for (var i = 2; ; i++)
        {
            var candidate = $"{baseSlug}-{i}";
            if (!existingSlugs.Contains(candidate)) return candidate;
        }
    }
}
