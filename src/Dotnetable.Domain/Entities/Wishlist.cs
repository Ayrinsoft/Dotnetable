using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Wishlist
{
    public int WishlistID { get; set; }

    public int WebsiteID { get; set; }

    public int WebsiteClientID { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual WebsiteClient WebsiteClient { get; set; } = null!;

    public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
}
