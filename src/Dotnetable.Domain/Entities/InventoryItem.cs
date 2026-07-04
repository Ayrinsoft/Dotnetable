using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class InventoryItem
{
    public int InventoryItemID { get; set; }

    public int WebsiteID { get; set; }

    public int ProductVariantID { get; set; }

    public int QuantityOnHand { get; set; }

    public int QuantityReserved { get; set; }

    public int ReorderLevel { get; set; }

    public decimal AvgCostUsd { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ProductVariant ProductVariant { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;
}
