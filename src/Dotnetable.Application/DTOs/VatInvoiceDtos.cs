namespace Dotnetable.Application.DTOs;

public sealed class VatInvoiceDto
{
    public int VatInvoiceId { get; init; }
    public int WebsiteId { get; init; }

    /// <summary>issued or received.</summary>
    public string Direction { get; init; } = "";

    public string InvoiceNumber { get; init; } = "";
    public DateTime InvoiceDate { get; init; }
    public int? OrderId { get; init; }
    public string? OrderNumber { get; init; }
    public string CurrencyCode { get; init; } = "";
    public InvoicePartyDto Seller { get; init; } = new();
    public InvoicePartyDto Buyer { get; init; } = new();
    public IReadOnlyList<VatInvoiceLineDto> Lines { get; init; } = Array.Empty<VatInvoiceLineDto>();
    public decimal NetTotal { get; init; }
    public decimal DiscountTotal { get; init; }
    public decimal TaxTotal { get; init; }
    public decimal PayableTotal { get; init; }
    public bool PricesIncludeTax { get; init; }
    public string? Note { get; init; }
}

public sealed class VatInvoiceLineDto
{
    public string Description { get; init; } = "";
    public decimal Quantity { get; init; }
    public string? Unit { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal NetAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal PayableAmount { get; init; }
}

public sealed class VatInvoiceSummaryDto
{
    public int VatInvoiceId { get; init; }
    public string Direction { get; init; } = "";
    public string InvoiceNumber { get; init; } = "";
    public DateTime InvoiceDate { get; init; }
    public int? OrderId { get; init; }
    public string OtherPartyName { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
    public decimal TaxTotal { get; init; }
    public decimal PayableTotal { get; init; }
}

public sealed class VatInvoiceListQuery
{
    public int WebsiteId { get; init; }
    public string? Direction { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
}

public sealed class VatInvoiceReceiveWrite
{
    public int WebsiteId { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierNationalId { get; set; }
    public string? SupplierEconomicCode { get; set; }
    public string? SupplierPhone { get; set; }
    public string? SupplierEmail { get; set; }
    public string? SupplierAddress { get; set; }
    public string? SupplierPostalCode { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Note { get; set; }
    public List<VatInvoiceLineWrite> Lines { get; set; } = [];
}

public sealed class VatInvoiceLineWrite
{
    public string? Description { get; set; }
    public decimal Quantity { get; set; } = 1;
    public string? Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
}

public sealed record VatInvoiceResult(bool Success, string? Error, VatInvoiceDto? Invoice);
