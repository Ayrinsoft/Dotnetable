using System;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// Snapshot of a variant's selling / compare-at price when it changes.
/// Used for multi-month price history (admin charts and lowest-price checks).
/// </summary>
public partial class ProductVariantPriceHistory
{
    public long ProductVariantPriceHistoryID { get; set; }

    public int ProductVariantID { get; set; }

    public decimal ReferencePriceUsd { get; set; }

    public decimal? CompareAtPriceUsd { get; set; }

    public DateTime RecordedAt { get; set; }

    public int? ChangedByMemberId { get; set; }

    public virtual Member? ChangedByMember { get; set; }

    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
