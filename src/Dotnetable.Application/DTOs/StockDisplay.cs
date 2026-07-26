namespace Dotnetable.Application.DTOs;

/// <summary>
/// Storefront stock presentation: exact counts only when scarce (&lt; threshold);
/// otherwise the product/seller is simply "in stock" without a number.
/// Negative stock values mean unlimited (digital goods).
/// </summary>
public static class StockDisplay
{
    /// <summary>Exact remaining count is shown only when available quantity is below this value.</summary>
    public const int ExactCountThreshold = 9;

    /// <summary>Sentinel / convention: stock &lt; 0 means unlimited sellable quantity.</summary>
    public static bool IsUnlimited(int available) => available < 0;

    public static bool IsInStock(int available) => available < 0 || available > 0;

    /// <summary>
    /// Returns the quantity for public display when 1..8; null when out of stock, unlimited, or "plenty" (≥9).
    /// </summary>
    public static int? ExactCountOrNull(int available) =>
        available > 0 && available < ExactCountThreshold ? available : null;

    /// <summary>
    /// Aggregates listing stocks: any unlimited (-1) wins; otherwise sum of non-negative quantities.
    /// </summary>
    public static int Aggregate(IEnumerable<int> quantities)
    {
        var any = false;
        var sum = 0;
        foreach (var q in quantities)
        {
            any = true;
            if (q < 0) return -1;
            sum += q;
        }
        return any ? sum : 0;
    }

    /// <summary>Max of two stock figures, preserving unlimited (-1).</summary>
    public static int Max(int a, int b)
    {
        if (a < 0 || b < 0) return -1;
        return Math.Max(a, b);
    }
}
