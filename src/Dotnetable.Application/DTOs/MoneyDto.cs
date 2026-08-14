namespace Dotnetable.Application.DTOs;

/// <summary>
/// A monetary amount expressed both in the canonical USD storage value and in the currency it should
/// be displayed in. Every price-bearing DTO (product listing, cart line, order total, wallet balance,
/// payment amount) should embed one of these instead of a bare decimal, so callers never have to guess
/// which currency a number is in.
/// </summary>
public sealed class MoneyDto
{
    public decimal Amount { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public decimal AmountUsd { get; set; }
}

/// <summary>
/// One-shot FX via the USD rate bridge (source → USD → destination).
/// Used for vendor settlement even when the storefront is single-currency.
/// </summary>
public sealed class FxViaUsdQuote
{
    public string FromCurrency { get; init; } = "";
    public string ToCurrency { get; init; } = "";
    public decimal FromAmount { get; init; }
    public decimal UsdAmount { get; init; }
    public decimal ToAmount { get; init; }
    /// <summary>Source-currency units per 1 USD (e.g. IRR per dollar).</summary>
    public decimal RateFromPerUsd { get; init; }
    /// <summary>Destination-currency units per 1 USD (e.g. KRW per dollar).</summary>
    public decimal RateToPerUsd { get; init; }
}

/// <summary>One vendor settlement row for the FX / cost report.</summary>
public sealed class SettlementFxReportRow
{
    public int SettlementID { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? VendorName { get; init; }
    public string? PartyLabel { get; init; }
    public byte Status { get; init; }
    public string SourceCurrencyCode { get; init; } = "";
    public decimal SourceNetAmount { get; init; }
    public decimal SourceTaxAmount { get; init; }
    public decimal SourceTotalAmount { get; init; }
    public decimal BridgeUsdAmount { get; init; }
    public string SettleCurrencyCode { get; init; } = "";
    public decimal NetAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal? ExchangeRateToUsd { get; init; }
    public decimal? ExchangeRateUsdToSettle { get; init; }
    public string? Note { get; init; }
}
