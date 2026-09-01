using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class CurrencyConversionService : ICurrencyConversionService
{
    // Contexts come from DbLease per operation: it joins an ambient transaction when one is in
    // flight and otherwise opens a short-lived context, so nothing is shared across a circuit.
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public CurrencyConversionService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<MoneyDto> ToDisplayAsync(int websiteId, decimal amountUsd, string? currencyCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var storeUsd = await GetStorePricesInUsdAsync(websiteId, ct);
        var rate = await ResolveRateAsync(_context, websiteId, currencyCode, storeUsd, ct);
        var digits = await GetDecimalDigitsAsync(_context, rate.CurrencyCode, ct);

        // Single-currency sites: amountUsd may actually be "site units" when rate is synthetic 1.
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
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var storeUsd = await GetStorePricesInUsdAsync(websiteId, ct);
        var fromRate = await ResolveRateAsync(_context, websiteId, fromCurrencyCode, storeUsd, ct);

        // Multi-currency display only when the site opted into USD dual / FX. Otherwise always site currency.
        string? effectiveTo = toCurrencyCode;
        if (!storeUsd)
            effectiveTo = fromRate.CurrencyCode;

        var toRate = effectiveTo is null || string.Equals(effectiveTo, fromRate.CurrencyCode, StringComparison.OrdinalIgnoreCase)
            ? fromRate
            : await ResolveRateAsync(_context, websiteId, effectiveTo, storeUsd, ct);

        var digitsSame = await GetDecimalDigitsAsync(_context, fromRate.CurrencyCode, ct);

        if (string.Equals(fromRate.CurrencyCode, toRate.CurrencyCode, StringComparison.OrdinalIgnoreCase))
        {
            var usdSame = amountUsdHint is decimal hint && hint > 0
                ? hint
                : (fromRate.USDToCurrency == 0 ? amountLocal : amountLocal / fromRate.USDToCurrency);
            // Pure single-currency: keep AmountUsd equal to local so orders/snapshots stay consistent without real FX.
            if (!storeUsd)
                usdSame = amountLocal;

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

        var digits = await GetDecimalDigitsAsync(_context, toRate.CurrencyCode, ct);
        return new MoneyDto
        {
            AmountUsd = amountUsd,
            CurrencyCode = toRate.CurrencyCode,
            Amount = Math.Round(amountUsd * toRate.USDToCurrency, digits, MidpointRounding.AwayFromZero),
        };
    }

    public async Task<(string CurrencyCode, decimal UsdToCurrency)> GetActiveRateAsync(int websiteId, string? currencyCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var storeUsd = await GetStorePricesInUsdAsync(websiteId, ct);
        var rate = await ResolveRateAsync(_context, websiteId, currencyCode, storeUsd, ct);
        return (rate.CurrencyCode, rate.USDToCurrency);
    }

    public async Task<List<CurrencyRate>> GetActiveRatesAsync(int websiteId, CancellationToken ct = default)
    {
        await using var _lease = await DbLease.OpenAsync(_contextFactory, ct);
        var _context = _lease.Context;

        var storeUsd = await GetStorePricesInUsdAsync(websiteId, ct);
        var rates = await _context.CurrencyRates.AsNoTracking()
            .Include(r => r.CurrencyCodeNavigation)
            .Where(r => r.WebsiteID == websiteId && r.CurrencyCodeNavigation.IsActive)
            .OrderByDescending(r => r.IsDefault)
            .ThenBy(r => r.CurrencyCode)
            .ToListAsync(ct);

        if (rates.Count == 0)
        {
            // Synthetic default so admin/storefront never hard-fail without FX config.
            var synth = await BuildSyntheticDefaultRateAsync(_context, websiteId, ct);
            return [synth];
        }

        // Single-currency sites: only expose the default operational currency (no multi-currency switcher).
        if (!storeUsd)
        {
            var def = rates.FirstOrDefault(r => r.IsDefault) ?? rates[0];
            return [def];
        }

        return rates;
    }

    public async Task<IReadOnlyList<string>> FindMissingFxRatesAsync(
        int websiteId, IEnumerable<string> currencyCodes, CancellationToken ct = default)
    {
        await using var _lease = await DbLease.OpenAsync(_contextFactory, ct);
        var _context = _lease.Context;

        var needed = currencyCodes
            .Select(NormalizeCode)
            .Where(c => c is not null && !string.Equals(c, "USD", StringComparison.OrdinalIgnoreCase))
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (needed.Count == 0) return Array.Empty<string>();

        var present = await _context.CurrencyRates.AsNoTracking()
            .Where(r => r.WebsiteID == websiteId && needed.Contains(r.CurrencyCode))
            .Select(r => r.CurrencyCode)
            .ToListAsync(ct);
        var have = new HashSet<string>(present, StringComparer.OrdinalIgnoreCase);
        return needed.Where(c => !have.Contains(c)).ToList();
    }

    public async Task<FxViaUsdQuote> ConvertViaUsdAsync(
        int websiteId,
        decimal amount,
        string fromCurrencyCode,
        string toCurrencyCode,
        decimal? usdHint = null,
        CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var from = NormalizeCode(fromCurrencyCode) ?? throw new InvalidOperationException("Source currency is required.");
        var to = NormalizeCode(toCurrencyCode) ?? from;
        var digitsTo = await GetDecimalDigitsAsync(_context, to, ct);

        if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase))
        {
            var sameRate = await TryGetFxRateAsync(_context, websiteId, from, ct);
            var usdSame = usdHint is > 0
                ? usdHint.Value
                : (sameRate is { USDToCurrency: > 0 } r ? amount / r.USDToCurrency : 0);
            return new FxViaUsdQuote
            {
                FromCurrency = from,
                ToCurrency = to,
                FromAmount = amount,
                UsdAmount = Math.Round(usdSame, 4, MidpointRounding.AwayFromZero),
                ToAmount = Math.Round(amount, digitsTo, MidpointRounding.AwayFromZero),
                RateFromPerUsd = sameRate?.USDToCurrency ?? 0,
                RateToPerUsd = sameRate?.USDToCurrency ?? 0,
            };
        }

        var fromRate = await TryGetFxRateAsync(_context, websiteId, from, ct)
                       ?? throw new InvalidOperationException(MissingRateMessage(from));
        var toRate = await TryGetFxRateAsync(_context, websiteId, to, ct)
                     ?? throw new InvalidOperationException(MissingRateMessage(to));

        var usd = usdHint is > 0
            ? usdHint.Value
            : (fromRate.USDToCurrency == 0 ? 0 : amount / fromRate.USDToCurrency);
        var dest = Math.Round(usd * toRate.USDToCurrency, digitsTo, MidpointRounding.AwayFromZero);

        return new FxViaUsdQuote
        {
            FromCurrency = from,
            ToCurrency = to,
            FromAmount = amount,
            UsdAmount = Math.Round(usd, 4, MidpointRounding.AwayFromZero),
            ToAmount = dest,
            RateFromPerUsd = fromRate.USDToCurrency,
            RateToPerUsd = toRate.USDToCurrency,
        };
    }

    public async Task<decimal> ToUsdAsync(int websiteId, decimal amount, string? currencyCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var storeUsd = await GetStorePricesInUsdAsync(websiteId, ct);
        if (!storeUsd)
            return amount; // single-currency: USD dual columns mirror site amounts

        var rate = await ResolveRateAsync(_context, websiteId, currencyCode, storeUsd: true, ct);
        return rate.USDToCurrency == 0 ? 0 : amount / rate.USDToCurrency;
    }

    public async Task<bool> GetStorePricesInUsdAsync(int websiteId, CancellationToken ct = default)
    {
        await using var _lease = await DbLease.OpenAsync(_contextFactory, ct);
        var _context = _lease.Context;

        return await _context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId)
            .Select(w => w.StorePricesInUsd)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<decimal> ResolveCatalogUnitUsdAsync(
        int websiteId,
        decimal referencePriceLocal,
        decimal referencePriceUsd,
        decimal? overridePriceLocal = null,
        decimal? overridePriceUsd = null,
        CancellationToken ct = default)
    {
        var storeUsd = await GetStorePricesInUsdAsync(websiteId, ct);

        if (overridePriceLocal is decimal ol && ol > 0)
            return storeUsd ? await ToUsdAsync(websiteId, ol, null, ct) : ol;

        if (storeUsd && overridePriceUsd is decimal ou && ou > 0)
            return ou;

        if (referencePriceLocal > 0)
            return storeUsd ? await ToUsdAsync(websiteId, referencePriceLocal, null, ct) : referencePriceLocal;

        if (storeUsd && referencePriceUsd > 0)
            return referencePriceUsd;

        return referencePriceLocal > 0 ? referencePriceLocal : referencePriceUsd;
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
        var storeUsd = await GetStorePricesInUsdAsync(websiteId, ct);

        if (overridePriceLocal is decimal ol && ol > 0)
            return (ol, code);

        if (storeUsd && overridePriceUsd is decimal ou && ou > 0)
            return (Math.Round(ou * rate, MidpointRounding.AwayFromZero), code);

        if (referencePriceLocal > 0)
            return (referencePriceLocal, code);

        if (storeUsd)
            return (Math.Round(referencePriceUsd * rate, MidpointRounding.AwayFromZero), code);

        return (referencePriceUsd, code);
    }

    private async Task<byte> GetDecimalDigitsAsync(AppDbContext _context, string currencyCode, CancellationToken ct)
    {
        return await _context.Currencies.AsNoTracking()
            .Where(c => c.CurrencyCode == currencyCode)
            .Select(c => (byte?)c.DecimalDigits)
            .FirstOrDefaultAsync(ct) ?? 2;
    }

    private async Task<CurrencyRate> ResolveRateAsync(AppDbContext _context, int websiteId, string? currencyCode, bool storeUsd, CancellationToken ct)
    {
        var query = _context.CurrencyRates.AsNoTracking().Where(r => r.WebsiteID == websiteId);

        // Without multi-currency, always use the website default rate / synthetic default.
        if (!storeUsd)
            currencyCode = null;

        var rate = currencyCode is null
            ? await query.FirstOrDefaultAsync(r => r.IsDefault, ct)
            : await query.FirstOrDefaultAsync(r => r.CurrencyCode == currencyCode, ct);

        rate ??= await query.FirstOrDefaultAsync(ct);

        if (rate is not null)
            return rate;

        return await BuildSyntheticDefaultRateAsync(_context, websiteId, ct);
    }

    /// <summary>
    /// When no CurrencyRates rows exist, fall back to Website.DefaultCurrencyCode with rate 1.
    /// Single-currency shops (e.g. KRW-only) work without configuring FX against USD.
    /// </summary>
    private async Task<CurrencyRate?> TryGetFxRateAsync(AppDbContext _context, int websiteId, string code, CancellationToken ct)
    {
        var rate = await _context.CurrencyRates.AsNoTracking()
            .FirstOrDefaultAsync(r => r.WebsiteID == websiteId && r.CurrencyCode == code, ct);
        if (rate is not null) return rate;

        if (string.Equals(code, "USD", StringComparison.OrdinalIgnoreCase))
        {
            return new CurrencyRate
            {
                WebsiteID = websiteId,
                CurrencyCode = "USD",
                USDToCurrency = 1m,
                IsDefault = false,
                LastUpdate = DateTime.UtcNow,
            };
        }

        return null;
    }

    internal static string MissingRateMessage(string currencyCode) =>
        $"Exchange rate for {currencyCode} is not configured. Open Finance → Exchange Rates and add {currencyCode} (how many {currencyCode} per 1 USD), then try again.";

    private static string? NormalizeCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var c = code.Trim().ToUpperInvariant();
        return c.Length == 0 ? null : (c.Length > 3 ? c[..3] : c);
    }

    private async Task<CurrencyRate> BuildSyntheticDefaultRateAsync(AppDbContext _context, int websiteId, CancellationToken ct)
    {
        var site = await _context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId)
            .Select(w => new { w.DefaultCurrencyCode })
            .FirstOrDefaultAsync(ct);

        var code = string.IsNullOrWhiteSpace(site?.DefaultCurrencyCode) ? "USD" : site!.DefaultCurrencyCode;
        var currency = await _context.Currencies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CurrencyCode == code, ct);

        return new CurrencyRate
        {
            WebsiteID = websiteId,
            CurrencyCode = code,
            USDToCurrency = 1m,
            IsDefault = true,
            LastUpdate = DateTime.UtcNow,
            CurrencyCodeNavigation = currency ?? new Currency
            {
                CurrencyCode = code,
                Name = code,
                Symbol = code,
                DecimalDigits = 2,
                IsActive = true,
            },
        };
    }
}
