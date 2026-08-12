namespace Dotnetable.Application.DTOs;

/// <summary>Printable tax invoice for a single order, with seller identity from website settings.</summary>
public sealed class OrderInvoiceDto
{
    public int OrderId { get; init; }
    public int WebsiteId { get; init; }
    public string OrderNumber { get; init; } = "";
    public DateTime CreatedAt { get; init; }
    public DateTime? PaidAt { get; init; }
    public byte Status { get; init; }
    public string CurrencyCode { get; init; } = "";

    public InvoicePartyDto Seller { get; init; } = new();
    public InvoicePartyDto Buyer { get; init; } = new();

    public IReadOnlyList<InvoiceLineDto> Lines { get; init; } = Array.Empty<InvoiceLineDto>();

    public decimal SubTotal { get; init; }
    public decimal DiscountTotal { get; init; }
    public decimal ShippingTotal { get; init; }
    public decimal TaxTotal { get; init; }
    public decimal GrandTotal { get; init; }
    /// <summary>
    /// Internal sum of instant markups (روکشی). Already included in line UnitPrice, SubTotal, and GrandTotal.
    /// Do not render as a separate add-on on customer-facing invoices/print — keep for admin/accounting only.
    /// </summary>
    public decimal MarkupTotal { get; init; }
    public bool PricesIncludeTax { get; init; }
    public bool ReportToTax { get; init; } = true;
    public byte SalesChannel { get; init; }
    public string? TaxBreakdownJson { get; init; }
    public string? AddressSnapshot { get; init; }
    public string? Note { get; init; }
}

public sealed class InvoicePartyDto
{
    public string Name { get; init; } = "";
    public string? LegalName { get; init; }
    public string? TaxId { get; init; }
    public string? EconomicCode { get; init; }
    public string? VatNumber { get; init; }
    public string? RegistrationNumber { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
}

public sealed class InvoiceLineDto
{
    public string Title { get; init; } = "";
    /// <summary>Site product code (<c>DN-42</c>), when the line maps to a catalog product.</summary>
    public string? ProductCode { get; init; }
    public string? Sku { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal LineTotal { get; init; }
}
