using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Converts amounts between the canonical USD storage value and a website's display currency. Every
/// domain that stores a "*Usd" column (Product pricing, Cart, Order, Payment, StockMovement, Wallet)
/// should resolve its display amount through this service instead of reading <see cref="Domain.Entities.CurrencyRate"/>
/// directly, so the "USD is the base currency" convention stays in one place.
/// </summary>
public interface ICurrencyConversionService
{
    /// <summary>Converts a USD amount into the given (or the website's default) display currency,
    /// rounded to that currency's <see cref="Domain.Entities.Currency.DecimalDigits"/>.</summary>
    Task<MoneyDto> ToDisplayAsync(int websiteId, decimal amountUsd, string? currencyCode = null, CancellationToken ct = default);

    /// <summary>Resolves the active rate for a website: the requested currency code, or the website's
    /// default <see cref="Domain.Entities.CurrencyRate"/> when none is specified.</summary>
    Task<(string CurrencyCode, decimal UsdToCurrency)> GetActiveRateAsync(int websiteId, string? currencyCode = null, CancellationToken ct = default);

    /// <summary>All active currencies configured for a website (for a currency switcher).</summary>
    Task<List<Domain.Entities.CurrencyRate>> GetActiveRatesAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Converts a display-currency amount back into USD using the given (or default) rate.</summary>
    Task<decimal> ToUsdAsync(int websiteId, decimal amount, string? currencyCode = null, CancellationToken ct = default);
}
