using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Coupon
{
    public int CouponID { get; set; }

    public int WebsiteID { get; set; }

    public string Code { get; set; } = null!;

    public byte DiscountType { get; set; }

    public decimal DiscountValue { get; set; }

    public decimal MinOrderAmountUsd { get; set; }

    public decimal? MaxDiscountAmountUsd { get; set; }

    public int? UsageLimitTotal { get; set; }

    public int? UsageLimitPerClient { get; set; }

    public int TimesUsed { get; set; }

    public DateTime? StartsAt { get; set; }

    public DateTime? EndsAt { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedByMemberID { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<CouponRedemption> CouponRedemptions { get; set; } = new List<CouponRedemption>();

    public virtual Member? CreatedByMember { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Website Website { get; set; } = null!;
}
