using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class VendorProduct
{
    public int VendorProductID { get; set; }

    public int WebsiteID { get; set; }

    public int VendorID { get; set; }

    public int ProductVariantID { get; set; }

    /// <summary>Listing reference price in the host website's operational currency.</summary>
    public decimal ReferencePrice { get; set; }

    /// <summary>Optional listing override in the host website's operational currency.</summary>
    public decimal? OverridePriceLocal { get; set; }

    /// <summary>USD equivalent of the reference price (dual storage / conversion bridge).</summary>
    public decimal ReferencePriceUsd { get; set; }

    /// <summary>Optional USD override for marketplace listings (when StorePricesInUsd or legacy rows).</summary>
    public decimal? OverridePrice { get; set; }

    public int StockQuantity { get; set; }

    public int DeliveryDays { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ProductVariant ProductVariant { get; set; } = null!;

    public virtual Vendor Vendor { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;
}
