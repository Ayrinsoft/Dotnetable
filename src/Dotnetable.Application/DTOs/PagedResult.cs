namespace Dotnetable.Application.DTOs;

/// <summary>A single page of results plus the total row count for the whole (filtered) set.</summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
}

/// <summary>Server-side paging / sorting / per-column search request, produced from the grid state.</summary>
public sealed class GridQuery
{
    /// <summary>
    /// Largest page any caller can ask for. Public API endpoints pass a query-string value straight
    /// into <see cref="PageSize"/>, so without a ceiling <c>?pageSize=10000000</c> is an unauthenticated
    /// request to materialise an entire table — the cheapest denial of service the product offers.
    /// </summary>
    public const int MaxPageSize = 100;

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    /// <summary>Sort clause(s), e.g. "Username DESC, Email ASC". Column names are validated server-side.</summary>
    public string? OrderBy { get; set; }

    /// <summary>Per-column search values keyed by column name (case-insensitive).</summary>
    public Dictionary<string, string> Search { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public int Skip => (PageIndex < 1 ? 0 : PageIndex - 1) * Take;

    /// <summary>The page size actually applied: defaulted when unset and clamped to <see cref="MaxPageSize"/>.</summary>
    public int Take => PageSize < 1 ? 10 : Math.Min(PageSize, MaxPageSize);

    /// <summary>
    /// Clamps a caller-supplied page size for callers that do not build a <see cref="GridQuery"/>
    /// (endpoints taking a bare <c>take</c> or <c>pageSize</c> parameter).
    /// </summary>
    public static int ClampPageSize(int requested, int fallback = 20) =>
        requested < 1 ? fallback : Math.Min(requested, MaxPageSize);

    /// <summary>Trimmed search value for a column, or null when absent/blank.</summary>
    public string? GetSearch(string column) =>
        Search.TryGetValue(column, out var v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : null;
}

/// <summary>A localization key/value row for the translations grid.</summary>
public sealed record TranslationEntry(string Key, string Value);
