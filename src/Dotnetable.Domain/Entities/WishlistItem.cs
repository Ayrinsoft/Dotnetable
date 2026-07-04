using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class WishlistItem
{
    public int WishlistItemID { get; set; }

    public int WishlistID { get; set; }

    public int ProductVariantID { get; set; }

    public DateTime AddedAt { get; set; }

    public virtual ProductVariant ProductVariant { get; set; } = null!;

    public virtual Wishlist Wishlist { get; set; } = null!;
}
