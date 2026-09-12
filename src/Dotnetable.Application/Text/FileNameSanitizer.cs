using System.Text.RegularExpressions;

namespace Dotnetable.Application.Text;

/// <summary>Makes an uploaded/renamed file's display name URL-safe — same motivation as
/// <see cref="SlugGenerator"/> for Category/Post/Page/Tag slugs, but extension-preserving and not
/// lowercased (a file's original casing is usually meaningful, unlike a slug). The actual storage
/// key (<c>FileRecord.StoredFileName</c>) is already a GUID and never derived from this value; this
/// only cleans up what's shown/searched/used as the download name.</summary>
public static class FileNameSanitizer
{
    /// <summary>Any run of characters that isn't a letter (any script), a digit, a dot, a hyphen or
    /// an underscore becomes a single "-". No transliteration.</summary>
    private static readonly Regex UnsafeRun = new(@"[^\p{L}\p{Nd}._-]+", RegexOptions.Compiled);

    private static readonly Regex UnsafeExtensionChar = new(@"[^\p{L}\p{Nd}.]", RegexOptions.Compiled);

    public static string Normalize(string? fileName, int maxLength = 120)
    {
        var trimmed = (fileName ?? string.Empty).Trim();
        if (trimmed.Length == 0) return "file";

        var ext = SanitizeExtension(Path.GetExtension(trimmed));
        var baseName = Path.GetFileNameWithoutExtension(trimmed);

        var safeBase = UnsafeRun.Replace(baseName, "-").Trim('-', '.', '_');
        if (safeBase.Length == 0) safeBase = "file";

        var budget = Math.Max(1, maxLength - ext.Length);
        if (safeBase.Length > budget)
            safeBase = safeBase[..budget].TrimEnd('-', '.', '_');
        if (safeBase.Length == 0) safeBase = "file";

        return safeBase + ext;
    }

    private static string SanitizeExtension(string ext)
    {
        if (string.IsNullOrEmpty(ext)) return string.Empty;
        var clean = UnsafeExtensionChar.Replace(ext, "");
        return clean.Length <= 1 ? string.Empty : clean.ToLowerInvariant();
    }
}
