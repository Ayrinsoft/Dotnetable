using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.DTOs;

public sealed class CustomerReturnLineInput
{
    public int OrderItemID { get; set; }
    public int Quantity { get; set; }
    /// <summary>Requested refund per unit. When 0, the paid unit price is used.</summary>
    public decimal UnitRefundRequested { get; set; }
}

public sealed class CustomerReturnLineApproval
{
    public int CustomerReturnRequestLineID { get; set; }
    public decimal UnitRefundApproved { get; set; }
}

public sealed class CustomerReturnLineReceive
{
    public int CustomerReturnRequestLineID { get; set; }
    public byte ReceivedCondition { get; set; }
    public byte HealthGrade { get; set; }
}

public sealed class CustomerReturnLineDto
{
    public int CustomerReturnRequestLineID { get; set; }
    public int OrderItemID { get; set; }
    public int? ProductVariantID { get; set; }
    public string Title { get; set; } = "";
    public string Sku { get; set; } = "";
    public int Quantity { get; set; }
    public int OrderedQty { get; set; }
    public int RemainingQty { get; set; }
    public decimal UnitPricePaid { get; set; }
    public decimal UnitCost { get; set; }
    public decimal UnitRefundRequested { get; set; }
    public decimal UnitRefundApproved { get; set; }
    public byte ReceivedCondition { get; set; }
    public byte HealthGrade { get; set; }
}

public sealed class CustomerReturnHistoryDto
{
    public byte FromStatus { get; set; }
    public byte ToStatus { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class CustomerReturnDto
{
    public int CustomerReturnRequestID { get; set; }
    public int WebsiteID { get; set; }
    public int OrderID { get; set; }
    public string OrderNumber { get; set; } = "";
    public int WebsiteClientID { get; set; }
    public string? ClientName { get; set; }
    public int? StockDocumentID { get; set; }
    public byte Status { get; set; }
    public byte Reason { get; set; }
    public string? ReasonNote { get; set; }
    public string? Description { get; set; }
    public string? ShipMethod { get; set; }
    public string? TrackingCode { get; set; }
    public byte ShippingPayer { get; set; }
    public decimal ReturnShippingCost { get; set; }
    public decimal SiteShippingShare { get; set; }
    public decimal RecoveredInventoryValue { get; set; }
    /// <summary>Recovered inventory − refund − site shipping. Negative = site loss.</summary>
    public decimal SiteImpactAmount { get; set; }
    public bool AcceptedAfterWindowExpired { get; set; }
    public int? ReceivedWarehouseID { get; set; }
    public string? ReceivedWarehouseName { get; set; }
    public string CurrencyCode { get; set; } = "";
    public decimal RequestedRefundTotal { get; set; }
    public decimal ApprovedRefundTotal { get; set; }
    public string? ReviewNote { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? ImpactPostedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<CustomerReturnLineDto> Lines { get; set; } = Array.Empty<CustomerReturnLineDto>();
    public IReadOnlyList<CustomerReturnHistoryDto> Histories { get; set; } = Array.Empty<CustomerReturnHistoryDto>();
    public IReadOnlyList<RecordAttachmentDto> Photos { get; set; } = Array.Empty<RecordAttachmentDto>();
}

public sealed class ReturnableOrderLineDto
{
    public int OrderItemID { get; set; }
    public int? ProductVariantID { get; set; }
    public string Title { get; set; } = "";
    public string Sku { get; set; } = "";
    public int OrderedQty { get; set; }
    public int AlreadyRequestedQty { get; set; }
    public int RemainingQty { get; set; }
    public decimal UnitPricePaid { get; set; }
}

public sealed class ReturnEligibilityDto
{
    public int OrderID { get; set; }
    public string OrderNumber { get; set; } = "";
    public bool Eligible { get; set; }
    public string? BlockReason { get; set; }
    public bool WindowExpired { get; set; }
    /// <summary>True when the window has ended but an operator can still accept with acknowledgment.</summary>
    public bool CanAcceptAfterWindow { get; set; }
    public DateTime? WindowStartUtc { get; set; }
    public DateTime? WindowEndUtc { get; set; }
    public int ReturnWindowDays { get; set; }
    public byte ReturnWindowFrom { get; set; }
    public string CurrencyCode { get; set; } = "";
    public IReadOnlyList<ReturnableOrderLineDto> Lines { get; set; } = Array.Empty<ReturnableOrderLineDto>();
}
