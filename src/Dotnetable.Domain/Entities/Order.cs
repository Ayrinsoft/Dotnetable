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

    /// <summary>When true, line prices already included tax at checkout (tax extracted or zero-added per site settings).</summary>
    public bool PricesIncludeTax { get; set; }

    /// <summary>JSON breakdown of applied tax lines for filings (code, rate, amount).</summary>
    public string? TaxBreakdownJson { get; set; }

    public decimal GrandTotal { get; set; }

    public decimal GrandTotalUsd { get; set; }

    public int? WebsiteClientAddressID { get; set; }

    public string? AddressSnapshot { get; set; }

    public int? CouponID { get; set; }

    public int? ShippingMethodID { get; set; }

    /// <summary>
    /// Warehouse / pick-pack lifecycle for physical goods.
    /// See <c>OrderPreparationStatus</c> on the order service.
    /// </summary>
    public byte PreparationStatus { get; set; }

    /// <summary>
    /// Carrier shipping lifecycle (independent of payment <see cref="Status"/>).
    /// See <c>OrderShippingStatus</c> on the order service.
    /// </summary>
    public byte ShippingStatus { get; set; }

    /// <summary>Postal / courier tracking code (barcode, AWB, etc.).</summary>
    public string? ShippingTrackingCode { get; set; }

    /// <summary>When the order was marked shipped (UTC), if known.</summary>
    public DateTime? ShippedAt { get; set; }

    public string? Note { get; set; }

    /// <summary>
    /// Sales origin channel (storefront, Instagram, WhatsApp, …). See <c>OrderSalesChannel</c>.
    /// </summary>
    public byte SalesChannel { get; set; } = 1;

    /// <summary>
    /// When false, this order is excluded from VAT / tax filing reports (common for some offline channels).
    /// Storefront online checkout always sets this true when tax is computed.
    /// </summary>
    public bool ReportToTax { get; set; } = true;

    /// <summary>
    /// Sum of positive per-line instant markups (روکشی) in the order currency:
    /// charged unit price above catalog unit price × quantity.
    /// </summary>
    public decimal MarkupTotal { get; set; }

    public int? CreatedByMemberID { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>When an unpaid order's stock reservation lapses. The expiry job cancels the order and releases stock past this instant. Null once paid.</summary>
    public DateTime? ReservationExpiresAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public virtual Coupon? Coupon { get; set; }

    public virtual CouponRedemption? CouponRedemption { get; set; }

    public virtual Member? CreatedByMember { get; set; }

    public virtual Currency CurrencyCodeNavigation { get; set; } = null!;

    public virtual ICollection<OrderDigitalAsset> OrderDigitalAssets { get; set; } = new List<OrderDigitalAsset>();

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
