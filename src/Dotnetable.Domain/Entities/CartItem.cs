using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class CartItem
{
    public int CartItemID { get; set; }

    public int CartID { get; set; }

    public int ProductVariantID { get; set; }

    public int? VendorProductID { get; set; }

    public int Quantity { get; set; }

    public DateTime AddedAt { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual ProductVariant ProductVariant { get; set; } = null!;

    public virtual VendorProduct? VendorProduct { get; set; }
}
