using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class CouponRedemption
{
    public int CouponRedemptionID { get; set; }

    public int CouponID { get; set; }

    public int OrderID { get; set; }

    public int WebsiteClientID { get; set; }

    public decimal DiscountAmountUsd { get; set; }

    public DateTime RedeemedAt { get; set; }

    public virtual Coupon Coupon { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual WebsiteClient WebsiteClient { get; set; } = null!;
}
