using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ProductVariantPriceHistory
{
    public long ProductVariantPriceHistoryID { get; set; }

    public int ProductVariantID { get; set; }

    public decimal ReferencePrice { get; set; }

    public decimal? CompareAtPrice { get; set; }

    public decimal ReferencePriceUsd { get; set; }

    public decimal? CompareAtPriceUsd { get; set; }

    public DateTime RecordedAt { get; set; }

    public int? ChangedByMemberId { get; set; }

    public virtual Member? ChangedByMember { get; set; }

    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
