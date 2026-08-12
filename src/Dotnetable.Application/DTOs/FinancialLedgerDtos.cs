namespace Dotnetable.Application.DTOs;

public sealed class FinancialLedgerFilter
{
    public int? WebsiteId { get; set; }
    public string? TransactionType { get; set; }
    public byte? Flow { get; set; }
    public int? OrderId { get; set; }
    public int? VendorId { get; set; }
    public int? WebsiteClientId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public bool? ReportToTax { get; set; }
    public bool CurrentOnly { get; set; } = true;
    /// <summary>When true, only rows vendors are allowed to see.</summary>
    public bool VendorVisibleOnly { get; set; }
    public string? Search { get; set; }
}

public sealed class FinancialLedgerEntryDto
{
    public long FinancialLedgerEntryID { get; init; }
    public int WebsiteID { get; init; }
    public string TransactionType { get; init; } = "";
    public byte Flow { get; init; }
    public decimal Amount { get; init; }
    public decimal AmountUsd { get; init; }
    public string CurrencyCode { get; init; } = "";
    public DateOnly OccurredDate { get; init; }
    public TimeOnly OccurredTime { get; init; }
    public DateTime OccurredAtUtc { get; init; }
    public string Title { get; init; } = "";
    public string? Description { get; init; }
    public bool ReportToTax { get; init; }
    public bool VendorVisible { get; init; }
    public int? VendorID { get; init; }
    public string? VendorName { get; init; }
    public int? OrderID { get; init; }
    public string? OrderNumber { get; init; }
    public int? OrderItemID { get; init; }
    public int? PaymentID { get; init; }
    public int? SettlementID { get; init; }
    public int? WebsiteClientID { get; init; }
    public Guid EventGroupId { get; init; }
    public int Version { get; init; }
    public long? SupersedesEntryID { get; init; }
    public bool IsCurrent { get; init; }
    public string? ChangeNote { get; init; }
    public int? CreatedByMemberID { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? MetaJson { get; init; }
}

/// <summary>Invoice-level financial rollup for admin order/invoice screens.</summary>
public sealed class OrderFinancialSummaryDto
{
    public int OrderId { get; init; }
    public string OrderNumber { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
    public bool ReportToTax { get; init; }

    public decimal CustomerPaidTotal { get; init; }
    public decimal AdditionalChargesTotal { get; init; }
    public decimal RefundsTotal { get; init; }
    public decimal NetReceived { get; init; }

    public decimal ShippingTotal { get; init; }
    public decimal MarkupTotal { get; init; }
    public decimal ProductRevenueTotal { get; init; }
    public decimal ProductCostTotal { get; init; }
    public decimal ProductProfitTotal { get; init; }
    public decimal DiscountTotal { get; init; }
    public decimal TaxTotal { get; init; }
    public decimal VendorSettlementTotal { get; init; }

    public decimal OrderGrandTotalSnapshot { get; init; }
    public IReadOnlyList<FinancialLedgerEntryDto> Lines { get; init; } = Array.Empty<FinancialLedgerEntryDto>();
    public IReadOnlyList<FinancialLedgerEntryDto> History { get; init; } = Array.Empty<FinancialLedgerEntryDto>();
}

public sealed class PostFinancialEntryRequest
{
    public int WebsiteId { get; set; }
    public string TransactionType { get; set; } = "";
    public byte Flow { get; set; }
    public decimal Amount { get; set; }
    public decimal? AmountUsd { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime? OccurredAtLocal { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public bool ReportToTax { get; set; }
    public bool VendorVisible { get; set; }
    public int? VendorId { get; set; }
    public int? OrderId { get; set; }
    public int? OrderItemId { get; set; }
    public int? PaymentId { get; set; }
    public int? SettlementId { get; set; }
    public int? WebsiteClientId { get; set; }
    public Guid? EventGroupId { get; set; }
    public string? MetaJson { get; set; }
    public int? MemberId { get; set; }
    /// <summary>When true, skip automatic GL projection (caller will project the event group once).</summary>
    public bool SkipGlProjection { get; set; }
}

public sealed class PostAdditionalChargeRequest
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public bool ReportToTax { get; set; } = true;
    public bool RecordPayment { get; set; }
    public byte? PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    public int? MemberId { get; set; }
}
