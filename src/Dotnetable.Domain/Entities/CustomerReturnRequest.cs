using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>Storefront / admin customer RMA. Pre-request is reviewed before the customer ships.</summary>
public partial class CustomerReturnRequest
{
    public int CustomerReturnRequestID { get; set; }
    public int WebsiteID { get; set; }
    public int OrderID { get; set; }
    public int WebsiteClientID { get; set; }
    public int? StockDocumentID { get; set; }

    /// <summary><see cref="Enums.CustomerReturnStatus"/>.</summary>
    public byte Status { get; set; }

    /// <summary><see cref="Enums.CustomerReturnReason"/>.</summary>
    public byte Reason { get; set; }
    public string? ReasonNote { get; set; }
    public string? Description { get; set; }

    public string? ShipMethod { get; set; }
    public string? TrackingCode { get; set; }

    /// <summary><see cref="Enums.ReturnShippingPayer"/> — required when admin approves.</summary>
    public byte ShippingPayer { get; set; }

    public string CurrencyCode { get; set; } = null!;
    public decimal RequestedRefundTotal { get; set; }
    public decimal ApprovedRefundTotal { get; set; }

    public string? ReviewNote { get; set; }
    public int? ReviewedByMemberID { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
    public virtual Order Order { get; set; } = null!;
    public virtual WebsiteClient WebsiteClient { get; set; } = null!;
    public virtual StockDocument? StockDocument { get; set; }
    public virtual Member? ReviewedByMember { get; set; }
    public virtual ICollection<CustomerReturnRequestLine> Lines { get; set; } = new List<CustomerReturnRequestLine>();
    public virtual ICollection<CustomerReturnRequestHistory> Histories { get; set; } = new List<CustomerReturnRequestHistory>();
}

public partial class CustomerReturnRequestLine
{
    public int CustomerReturnRequestLineID { get; set; }
    public int CustomerReturnRequestID { get; set; }
    public int OrderItemID { get; set; }
    public int? ProductVariantID { get; set; }
    public int Quantity { get; set; }

    /// <summary>Unit price the customer actually paid (order currency snapshot).</summary>
    public decimal UnitPricePaid { get; set; }

    /// <summary>Refund the customer asked for per unit (may differ from paid).</summary>
    public decimal UnitRefundRequested { get; set; }

    /// <summary>Refund admin approved per unit (set on approve; may differ from paid and requested).</summary>
    public decimal UnitRefundApproved { get; set; }

    public virtual CustomerReturnRequest ReturnRequest { get; set; } = null!;
    public virtual OrderItem OrderItem { get; set; } = null!;
}

public partial class CustomerReturnRequestHistory
{
    public int CustomerReturnRequestHistoryID { get; set; }
    public int CustomerReturnRequestID { get; set; }
    public byte FromStatus { get; set; }
    public byte ToStatus { get; set; }
    public string? Note { get; set; }
    public int? CreatedByMemberID { get; set; }
    public int? CreatedByClientID { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual CustomerReturnRequest ReturnRequest { get; set; } = null!;
    public virtual Member? CreatedByMember { get; set; }
}
