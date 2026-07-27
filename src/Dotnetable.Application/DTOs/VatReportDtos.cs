namespace Dotnetable.Application.DTOs;

public sealed class VatReportRequest
{
    public int WebsiteId { get; init; }
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
}

public sealed class VatReportDto
{
    public int WebsiteId { get; init; }
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public string? SellerLegalName { get; init; }
    public string? SellerTaxId { get; init; }
    public string? SellerVatNumber { get; init; }
    public string? SellerEconomicCode { get; init; }
    public string DefaultCurrencyCode { get; init; } = "";

    /// <summary>Customer sales (output VAT) from paid/fulfilled orders.</summary>
    public VatBucketDto OutputSales { get; init; } = new();

    /// <summary>B2B settlement tax on host payables / receivables (for dual-site books).</summary>
    public VatBucketDto SettlementTax { get; init; } = new();

    public IReadOnlyList<VatReportLineDto> OrderLines { get; init; } = Array.Empty<VatReportLineDto>();
    public IReadOnlyList<VatReportLineDto> SettlementLines { get; init; } = Array.Empty<VatReportLineDto>();
}

public sealed class VatBucketDto
{
    public decimal Net { get; init; }
    public decimal Tax { get; init; }
    public decimal Gross { get; init; }
    public int DocumentCount { get; init; }
}

public sealed class VatReportLineDto
{
    public string Source { get; init; } = ""; // Order | Settlement
    public int DocumentId { get; init; }
    public string DocumentNumber { get; init; } = "";
    public DateTime Date { get; init; }
    public string CurrencyCode { get; init; } = "";
    public decimal Net { get; init; }
    public decimal Tax { get; init; }
    public decimal Gross { get; init; }
    public bool PricesIncludeTax { get; init; }
    public string? Counterparty { get; init; }
    public string? BreakdownJson { get; init; }
}
