using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Converts monetary amounts for a website.
/// Default mode (<see cref="Website.StorePricesInUsd"/> = false): single operational currency only —
/// no multi-currency switcher and no real FX required (e.g. KRW-only or IRR-only shops).
/// Dual mode (flag = true): site currency is still authority; USD dual columns + rates enable
/// multi-currency display (rates = local units per 1 USD).
/// </summary>
public interface ICurrencyConversionService
{
    /// <summary>Converts a USD amount into the given (or the website's default) display currency,
    /// rounded to that currency's <see cref="Currency.DecimalDigits"/>.</summary>
    Task<MoneyDto> ToDisplayAsync(int websiteId, decimal amountUsd, string? currencyCode = null, CancellationToken ct = default);

    /// <summary>
    /// Converts a site-currency (or <paramref name="fromCurrencyCode"/>) amount into the requested display currency
    /// via the USD rate bridge. When the target matches the source currency, returns the amount unchanged (no drift).
    /// </summary>
    Task<MoneyDto> ToDisplayFromLocalAsync(
        int websiteId,
        decimal amountLocal,
        string? fromCurrencyCode = null,
        string? toCurrencyCode = null,
        decimal? amountUsdHint = null,
        CancellationToken ct = default);

    /// <summary>Resolves the active rate for a website: the requested currency code, or the website's
    /// default <see cref="CurrencyRate"/> when none is specified.</summary>
    Task<(string CurrencyCode, decimal UsdToCurrency)> GetActiveRateAsync(int websiteId, string? currencyCode = null, CancellationToken ct = default);

    /// <summary>All active currencies configured for a website (for a currency switcher).</summary>
    Task<List<CurrencyRate>> GetActiveRatesAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Converts a display-currency amount back into USD using the given (or default) rate.</summary>
    Task<decimal> ToUsdAsync(int websiteId, decimal amount, string? currencyCode = null, CancellationToken ct = default);

    /// <summary>Whether the website dual-persists USD columns.</summary>
    Task<bool> GetStorePricesInUsdAsync(int websiteId, CancellationToken ct = default);

    /// <summary>
    /// Canonical catalog unit price in USD for conversion / checkout (flag-aware).
    /// Prefers stored USD when dual-store is on or when local is missing; otherwise derives from local.
    /// </summary>
    Task<decimal> ResolveCatalogUnitUsdAsync(
        int websiteId,
        decimal referencePriceLocal,
        decimal referencePriceUsd,
        decimal? overridePriceLocal = null,
        decimal? overridePriceUsd = null,
        CancellationToken ct = default);

    /// <summary>Canonical catalog unit price in the website operational currency.</summary>
    Task<(decimal AmountLocal, string CurrencyCode)> ResolveCatalogUnitLocalAsync(
        int websiteId,
        decimal referencePriceLocal,
        decimal referencePriceUsd,
        decimal? overridePriceLocal = null,
        decimal? overridePriceUsd = null,
        CancellationToken ct = default);
}
