namespace Dotnetable.Application.DTOs;

/// <summary>
/// Storefront stock presentation: exact counts only when scarce (&lt; threshold);
/// otherwise the product/seller is simply "in stock" without a number.
/// </summary>
public static class StockDisplay
{
    /// <summary>Exact remaining count is shown only when available quantity is below this value.</summary>
    public const int ExactCountThreshold = 9;

    public static bool IsInStock(int available) => available > 0;

    /// <summary>
    /// Returns the quantity for public display when 1..8; null when out of stock or "plenty" (≥9).
    /// </summary>
    public static int? ExactCountOrNull(int available) =>
        available > 0 && available < ExactCountThreshold ? available : null;
}
