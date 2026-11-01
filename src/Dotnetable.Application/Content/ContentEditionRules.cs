using System.Text.RegularExpressions;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Content;

/// <summary>
/// Opt-in stable keys and release ranges for help centers, policies, and release-scoped articles.
/// A blog post type leaves both flags off, and these rules then clear the extra fields.
/// Version labels are content release lines such as <c>1.2</c>, not the API <c>X-Api-Version</c> header.
/// </summary>
public static class ContentEditionRules
{
    public const int KeyMaxLength = 80;
    public const int VersionMaxLength = 40;

    private static readonly Regex KeyPattern = new("^[a-z0-9][a-z0-9._-]{0,79}$", RegexOptions.Compiled);
    private static readonly Regex VersionPattern = new("^[A-Za-z0-9][A-Za-z0-9._+-]{0,39}$", RegexOptions.Compiled);

    /// <summary>
    /// Compares release labels. A null or blank label is older than every concrete label.
    /// Numeric segments compare as integers, so <c>1.10</c> is after <c>1.2</c>.
    /// A missing segment counts as 0. Mixed text falls back to ordinal ignore-case.
    /// </summary>
    public static int Compare(string? left, string? right)
    {
        var aMissing = string.IsNullOrEmpty(left);
        var bMissing = string.IsNullOrEmpty(right);
        if (aMissing && bMissing) return 0;
        if (aMissing) return -1;
        if (bMissing) return 1;

        var a = left!.Split('.');
        var b = right!.Split('.');
        var count = Math.Max(a.Length, b.Length);
        for (var i = 0; i < count; i++)
        {
            var asg = i < a.Length ? a[i] : "0";
            var bsg = i < b.Length ? b[i] : "0";
            var aNum = long.TryParse(asg, NumberStyles(), null, out var av);
            var bNum = long.TryParse(bsg, NumberStyles(), null, out var bv);
            var cmp = aNum && bNum
                ? av.CompareTo(bv)
                : string.Compare(asg, bsg, StringComparison.OrdinalIgnoreCase);
            if (cmp != 0) return cmp;
        }
        return 0;
    }

    public static readonly IComparer<string?> LabelComparer = Comparer<string?>.Create(Compare);

    /// <summary>Blank becomes null. Otherwise lower-case <c>a-z0-9._-</c>, max 80, starting with a letter or digit.</summary>
    public static string? NormalizeKey(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var key = raw.Trim().ToLowerInvariant();
        if (key.Length > KeyMaxLength || !KeyPattern.IsMatch(key))
            throw new ContentEditionException("Stable key must be 1-80 characters: a-z, 0-9, dot, underscore, or hyphen.");
        return key;
    }

    /// <summary>Blank becomes null. Otherwise a release label such as <c>1.2</c>, max 40.</summary>
    public static string? NormalizeVersion(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var label = raw.Trim();
        if (label.Length > VersionMaxLength || !VersionPattern.IsMatch(label))
            throw new ContentEditionException("Version must be 1-40 characters: letters, digits, dot, underscore, plus, or hyphen.");
        return label;
    }

    /// <summary>
    /// Writes the edition fields the post type allows. Versioning implies a stable key is allowed.
    /// A range without a key is rejected. When both ends are set, <c>AppliesFrom</c> must be &lt;= <c>AppliesTo</c>.
    /// </summary>
    public static void Apply(Post post, PostType type)
    {
        var allowKey = type.SupportsStableKey || type.SupportsVersioning;
        if (!allowKey)
        {
            post.StableKey = null;
            post.AppliesFrom = null;
            post.AppliesTo = null;
            return;
        }

        post.StableKey = NormalizeKey(post.StableKey);
        if (!type.SupportsVersioning)
        {
            post.AppliesFrom = null;
            post.AppliesTo = null;
        }
        else
        {
            post.AppliesFrom = NormalizeVersion(post.AppliesFrom);
            post.AppliesTo = NormalizeVersion(post.AppliesTo);
        }

        if (post.StableKey is null)
        {
            if (post.AppliesFrom is not null || post.AppliesTo is not null)
                throw new ContentEditionException("A version range needs a stable key.");
            return;
        }

        if (post.AppliesFrom is not null && post.AppliesTo is not null && Compare(post.AppliesFrom, post.AppliesTo) > 0)
            throw new ContentEditionException("Applies from must be the same as or earlier than applies to.");
    }

    /// <summary>
    /// Picks the edition for release <paramref name="version"/>.
    /// Null <paramref name="version"/> prefers the unversioned row (both ends null), then the highest
    /// <c>AppliesFrom</c>. A concrete version matches when <c>AppliesFrom</c> is null or &lt;= V and
    /// <c>AppliesTo</c> is null or &gt;= V. The highest <c>AppliesFrom</c> wins. Ties use the highest id.
    /// </summary>
    public static EditionCandidate? Resolve(IReadOnlyList<EditionCandidate> editions, string? version)
    {
        if (editions.Count == 0) return null;

        if (version is null)
        {
            var unversioned = editions
                .Where(e => e.AppliesFrom is null && e.AppliesTo is null)
                .OrderByDescending(e => e.Id)
                .FirstOrDefault();
            if (unversioned is not null) return unversioned;
            return editions
                .OrderByDescending(e => e.AppliesFrom, LabelComparer)
                .ThenByDescending(e => e.Id)
                .First();
        }

        var matches = editions.Where(e => Matches(e, version)).ToList();
        if (matches.Count == 0) return null;
        return matches
            .OrderByDescending(e => e.AppliesFrom, LabelComparer)
            .ThenByDescending(e => e.Id)
            .First();
    }

    /// <summary>
    /// One row per stable key for a help-center index at <paramref name="version"/>.
    /// Posts whose type does not support versioning, and posts with no stable key, are kept as they are.
    /// </summary>
    public static List<int> SelectForVersion(IReadOnlyList<EditionCandidate> rows, string version)
    {
        var kept = new List<EditionCandidate>();
        var groups = new Dictionary<(int PostTypeId, string Key), List<EditionCandidate>>();
        foreach (var row in rows)
        {
            if (!row.SupportsVersioning || string.IsNullOrEmpty(row.StableKey))
            {
                kept.Add(row);
                continue;
            }

            var key = (row.PostTypeId, row.StableKey);
            if (!groups.TryGetValue(key, out var list))
            {
                list = new List<EditionCandidate>();
                groups[key] = list;
            }
            list.Add(row);
        }

        foreach (var list in groups.Values)
        {
            var winner = Resolve(list, version);
            if (winner is not null) kept.Add(winner);
        }

        return kept
            .OrderByDescending(r => r.SortAt)
            .ThenByDescending(r => r.Id)
            .Select(r => r.Id)
            .ToList();
    }

    private static bool Matches(EditionCandidate edition, string version)
    {
        var fromOk = edition.AppliesFrom is null || Compare(edition.AppliesFrom, version) <= 0;
        var toOk = edition.AppliesTo is null || Compare(edition.AppliesTo, version) >= 0;
        return fromOk && toOk;
    }

    private static System.Globalization.NumberStyles NumberStyles() =>
        System.Globalization.NumberStyles.None;
}

/// <summary>One logical post row used by <see cref="ContentEditionRules.Resolve"/>.</summary>
public sealed record EditionCandidate(
    int Id,
    int PostTypeId,
    bool SupportsVersioning,
    string? StableKey,
    string? AppliesFrom,
    string? AppliesTo,
    DateTime SortAt);

/// <summary>Rejected stable key, version label, or overlapping edition.</summary>
public sealed class ContentEditionException : InvalidOperationException
{
    public ContentEditionException(string message) : base(message) { }
}
