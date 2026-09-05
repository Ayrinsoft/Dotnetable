using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Manages a website's own set of <see cref="CurrencyRate"/> rows (USD-to-currency, one flagged default).</summary>
public interface ICurrencyRateService
{
    Task<List<CurrencyRate>> GetByWebsiteAsync(int websiteId, CancellationToken ct = default);
    Task<PagedResult<CurrencyRate>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default);

    Task CreateAsync(CurrencyRate rate, CancellationToken ct = default);

    Task<bool> UpdateAsync(CurrencyRate rate, CancellationToken ct = default);

    Task<bool> DeleteAsync(int currencyRateId, int websiteId, CancellationToken ct = default);

    /// <summary>Marks one rate as the website's default and clears the flag on all others.</summary>
    Task<bool> SetDefaultAsync(int currencyRateId, int websiteId, CancellationToken ct = default);

    /// <summary>
    /// Updates the default (or first) USD→local rate for a website. Used by catalog price lists
    /// so operators can change today's dollar without opening the rate grid.
    /// </summary>
    Task<CurrencyRate?> UpdateDefaultUsdToCurrencyAsync(int websiteId, decimal usdToCurrency, CancellationToken ct = default);
}
