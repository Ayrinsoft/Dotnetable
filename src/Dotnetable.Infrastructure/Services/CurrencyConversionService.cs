using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class CurrencyConversionService : ICurrencyConversionService
{
    private readonly AppDbContext _context;

    public CurrencyConversionService(AppDbContext context) => _context = context;

    public async Task<MoneyDto> ToDisplayAsync(int websiteId, decimal amountUsd, string? currencyCode = null, CancellationToken ct = default)
    {
        var rate = await ResolveRateAsync(websiteId, currencyCode, ct);
        var digits = await GetDecimalDigitsAsync(rate.CurrencyCode, ct);

        return new MoneyDto
        {
            AmountUsd = amountUsd,
            CurrencyCode = rate.CurrencyCode,
            Amount = Math.Round(amountUsd * rate.USDToCurrency, digits, MidpointRounding.AwayFromZero),
        };
    }

    public async Task<MoneyDto> ToDisplayFromLocalAsync(
        int websiteId,
        decimal amountLocal,
        string? fromCurrencyCode = null,
        string? toCurrencyCode = null,
        decimal? amountUsdHint = null,
        CancellationToken ct = default)
    {
        var fromRate = await ResolveRateAsync(websiteId, fromCurrencyCode, ct);
        var toRate = toCurrencyCode is null || string.Equals(toCurrencyCode, fromRate.CurrencyCode, StringComparison.OrdinalIgnoreCase)
            ? fromRate
            : await ResolveRateAsync(websiteId, toCurrencyCode, ct);

        if (string.Equals(fromRate.CurrencyCode, toRate.CurrencyCode, StringComparison.OrdinalIgnoreCase))
        {
            var digitsSame = await GetDecimalDigitsAsync(fromRate.CurrencyCode, ct);
            var usdSame = amountUsdHint is decimal hint && hint > 0
                ? hint
                : (fromRate.USDToCurrency == 0 ? 0 : amountLocal / fromRate.USDToCurrency);
            return new MoneyDto
            {
                Amount = Math.Round(amountLocal, digitsSame, MidpointRounding.AwayFromZero),
                CurrencyCode = fromRate.CurrencyCode,
                AmountUsd = usdSame,
            };
        }

        var amountUsd = amountUsdHint is decimal stored && stored > 0
            ? stored
            : (fromRate.USDToCurrency == 0 ? 0 : amountLocal / fromRate.USDToCurrency);

        var digits = await GetDecimalDigitsAsync(toRate.CurrencyCode, ct);
        return new MoneyDto
        {
            AmountUsd = amountUsd,
            CurrencyCode = toRate.CurrencyCode,
            Amount = Math.Round(amountUsd * toRate.USDToCurrency, digits, MidpointRounding.AwayFromZero),
        };
    }

    public async Task<(string CurrencyCode, decimal UsdToCurrency)> GetActiveRateAsync(int websiteId, string? currencyCode = null, CancellationToken ct = default)
    {
        var rate = await ResolveRateAsync(websiteId, currencyCode, ct);
        return (rate.CurrencyCode, rate.USDToCurrency);
    }

    public async Task<List<CurrencyRate>> GetActiveRatesAsync(int websiteId, CancellationToken ct = default) =>
        await _context.CurrencyRates.AsNoTracking()
            .Include(r => r.CurrencyCodeNavigation)
            .Where(r => r.WebsiteID == websiteId && r.CurrencyCodeNavigation.IsActive)
            .OrderByDescending(r => r.IsDefault)
            .ThenBy(r => r.CurrencyCode)
            .ToListAsync(ct);

    public async Task<decimal> ToUsdAsync(int websiteId, decimal amount, string? currencyCode = null, CancellationToken ct = default)
    {
        var rate = await ResolveRateAsync(websiteId, currencyCode, ct);
        return rate.USDToCurrency == 0 ? 0 : amount / rate.USDToCurrency;
    }

    public async Task<bool> GetStorePricesInUsdAsync(int websiteId, CancellationToken ct = default) =>
        await _context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId)
            .Select(w => w.StorePricesInUsd)
            .FirstOrDefaultAsync(ct);

    public async Task<decimal> ResolveCatalogUnitUsdAsync(
        int websiteId,
        decimal referencePriceLocal,
        decimal referencePriceUsd,
        decimal? overridePriceLocal = null,
        decimal? overridePriceUsd = null,
        CancellationToken ct = default)
    {
        // Prefer explicit local override → USD; else USD override; else local reference → USD; else stored USD.
        if (overridePriceLocal is decimal ol && ol > 0)
            return await ToUsdAsync(websiteId, ol, null, ct);

        if (overridePriceUsd is decimal ou && ou > 0)
            return ou;

        if (referencePriceLocal > 0)
            return await ToUsdAsync(websiteId, referencePriceLocal, null, ct);

        return referencePriceUsd;
    }

    public async Task<(decimal AmountLocal, string CurrencyCode)> ResolveCatalogUnitLocalAsync(
        int websiteId,
        decimal referencePriceLocal,
        decimal referencePriceUsd,
        decimal? overridePriceLocal = null,
        decimal? overridePriceUsd = null,
        CancellationToken ct = default)
    {
        var (code, rate) = await GetActiveRateAsync(websiteId, null, ct);

        if (overridePriceLocal is decimal ol && ol > 0)
            return (ol, code);

        if (overridePriceUsd is decimal ou && ou > 0)
            return (Math.Round(ou * rate, MidpointRounding.AwayFromZero), code);

        if (referencePriceLocal > 0)
            return (referencePriceLocal, code);

        return (Math.Round(referencePriceUsd * rate, MidpointRounding.AwayFromZero), code);
    }

    private async Task<byte> GetDecimalDigitsAsync(string currencyCode, CancellationToken ct) =>
        await _context.Currencies.AsNoTracking()
            .Where(c => c.CurrencyCode == currencyCode)
            .Select(c => (byte?)c.DecimalDigits)
            .FirstOrDefaultAsync(ct) ?? 2;

    private async Task<CurrencyRate> ResolveRateAsync(int websiteId, string? currencyCode, CancellationToken ct)
    {
        var query = _context.CurrencyRates.AsNoTracking().Where(r => r.WebsiteID == websiteId);

        var rate = currencyCode is null
            ? await query.FirstOrDefaultAsync(r => r.IsDefault, ct)
            : await query.FirstOrDefaultAsync(r => r.CurrencyCode == currencyCode, ct);

        rate ??= await query.FirstOrDefaultAsync(ct);

        return rate ?? throw new InvalidOperationException(
            $"Website {websiteId} has no configured currency rate.");
    }
}
