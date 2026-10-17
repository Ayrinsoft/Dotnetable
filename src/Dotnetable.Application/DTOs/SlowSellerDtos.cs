namespace Dotnetable.Application.DTOs;

/// <summary>One published product whose paid sales in the last 30 days are at or under the admin's cutoff.</summary>
public sealed class SlowSellerRow
{
    public int ProductId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    /// <summary>Units on paid, processing, shipped, or completed orders in the window.</summary>
    public int UnitsSold { get; init; }
    public int OrderCount { get; init; }
    /// <summary>Selling price of the default active variant, in the website currency.</summary>
    public decimal? Price { get; init; }
    /// <summary>True when an active variant already has a compare-at price above its selling price.</summary>
    public bool HasDiscount { get; init; }
    public int StockOnHand { get; init; }
}

/// <summary>The slow-seller list for one website. Nothing here changes a price.</summary>
public sealed class SlowSellerReport
{
    public DateTime FromUtc { get; init; }
    public DateTime ToUtc { get; init; }
    public string? CurrencyCode { get; init; }
    public IReadOnlyList<SlowSellerRow> Items { get; init; } = Array.Empty<SlowSellerRow>();
    public int TotalCount { get; init; }
}
