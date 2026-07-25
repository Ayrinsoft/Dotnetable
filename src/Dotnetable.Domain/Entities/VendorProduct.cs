using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class VendorProduct
{
    public int VendorProductID { get; set; }

    public int WebsiteID { get; set; }

    public int VendorID { get; set; }

    public int ProductVariantID { get; set; }

    public decimal ReferencePriceUsd { get; set; }

    public decimal? OverridePrice { get; set; }

    public int StockQuantity { get; set; }

    public int DeliveryDays { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ProductVariant ProductVariant { get; set; } = null!;

    public virtual Vendor Vendor { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;
}
