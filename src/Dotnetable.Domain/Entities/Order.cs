using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Order
{
    public int OrderID { get; set; }

    public int WebsiteID { get; set; }

    public string OrderNumber { get; set; } = null!;

    public int WebsiteClientID { get; set; }

    public byte Status { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public decimal ExchangeRateToUsd { get; set; }

    public decimal SubTotal { get; set; }

    public decimal DiscountTotal { get; set; }

    public decimal ShippingTotal { get; set; }

    public decimal TaxTotal { get; set; }

    public decimal GrandTotal { get; set; }

    public decimal GrandTotalUsd { get; set; }

    public int? WebsiteClientAddressID { get; set; }

    public string? AddressSnapshot { get; set; }

    public int? CouponID { get; set; }

    public int? ShippingMethodID { get; set; }

    public string? Note { get; set; }

    public int? CreatedByMemberID { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public virtual Coupon? Coupon { get; set; }

    public virtual CouponRedemption? CouponRedemption { get; set; }

    public virtual Member? CreatedByMember { get; set; }

    public virtual Currency CurrencyCodeNavigation { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<SupportInteraction> SupportInteractions { get; set; } = new List<SupportInteraction>();

    public virtual ICollection<SupportSession> SupportSessions { get; set; } = new List<SupportSession>();

    public virtual ShippingMethod? ShippingMethod { get; set; }

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public virtual ICollection<VendorCreditTransaction> VendorCreditTransactionMirrorOrders { get; set; } = new List<VendorCreditTransaction>();

    public virtual Website Website { get; set; } = null!;

    public virtual WebsiteClient WebsiteClient { get; set; } = null!;

    public virtual WebsiteClientAddress? WebsiteClientAddress { get; set; }
}
