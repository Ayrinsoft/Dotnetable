using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class OrderItem
{
    public int OrderItemID { get; set; }

    public int OrderID { get; set; }

    public int WebsiteID { get; set; }

    public int SourceWebsiteID { get; set; }

    /// <summary>
    /// Catalog variant when the line maps to a product. Null for free-form admin lines
    /// (title/price only, no inventory).
    /// </summary>
    public int? ProductVariantID { get; set; }

    public int? VendorProductID { get; set; }

    public int? VendorID { get; set; }

    public string TitleSnapshot { get; set; } = null!;

    public string SkuSnapshot { get; set; } = null!;

    public int Quantity { get; set; }

    /// <summary>Unit price charged to the customer (order currency).</summary>
    public decimal UnitPrice { get; set; }

    public decimal UnitPriceUsd { get; set; }

    public decimal UnitCostUsd { get; set; }

    /// <summary>
    /// Catalog / list unit price at order time (order currency), before any admin instant markup.
    /// When the customer is charged more, the difference is <see cref="UnitMarkup"/>.
    /// </summary>
    public decimal CatalogUnitPrice { get; set; }

    /// <summary>
    /// Instant markup (روکشی لحظه‌ای) per unit in order currency: max(0, UnitPrice − CatalogUnitPrice).
    /// </summary>
    public decimal UnitMarkup { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalPrice { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual OrderDigitalAsset? OrderDigitalAsset { get; set; }

    public virtual ProductVariant? ProductVariant { get; set; }

    public virtual ICollection<SettlementItem> SettlementItems { get; set; } = new List<SettlementItem>();

    public virtual Website SourceWebsite { get; set; } = null!;

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public virtual ICollection<VendorCreditTransaction> VendorCreditTransactions { get; set; } = new List<VendorCreditTransaction>();

    public virtual Vendor? Vendor { get; set; }

    public virtual VendorProduct? VendorProduct { get; set; }

    public virtual Website Website { get; set; } = null!;
}
