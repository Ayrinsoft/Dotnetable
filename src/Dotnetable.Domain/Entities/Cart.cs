using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Cart
{
    public int CartID { get; set; }

    public int WebsiteID { get; set; }

    public int? WebsiteClientID { get; set; }

    public string? SessionKey { get; set; }

    public int? CouponID { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Coupon? Coupon { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual WebsiteClient? WebsiteClient { get; set; }
}
