using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class StockMovement
{
    public int StockMovementID { get; set; }

    public int WebsiteID { get; set; }

    public int ProductVariantID { get; set; }

    public byte Type { get; set; }

    public int Quantity { get; set; }

    public decimal UnitCostUsd { get; set; }

    public decimal? UnitSalePriceUsd { get; set; }

    public string CurrencyCode { get; set; } = null!;

    /// <summary>
    /// website&apos;s local currency rate snapshotted at the moment of this stock entry/exit, so accounting and reports can be reconstructed in local currency at that point in time
    /// </summary>
    public decimal ExchangeRateToUsd { get; set; } = 1m;

    public int? SupplierID { get; set; }

    public int? OrderID { get; set; }

    public int? OrderItemID { get; set; }

    public string? Note { get; set; }

    public int? CreatedByMemberID { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Member? CreatedByMember { get; set; }

    public virtual Currency CurrencyCodeNavigation { get; set; } = null!;

    public virtual Order? Order { get; set; }

    public virtual OrderItem? OrderItem { get; set; }

    public virtual ProductVariant ProductVariant { get; set; } = null!;

    public virtual ICollection<SettlementItem> SettlementItems { get; set; } = new List<SettlementItem>();

    public virtual Supplier? Supplier { get; set; }

    public virtual Website Website { get; set; } = null!;
}
