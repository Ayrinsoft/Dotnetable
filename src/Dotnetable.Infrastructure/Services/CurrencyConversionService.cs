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
        var digits = await _context.Currencies.AsNoTracking()
            .Where(c => c.CurrencyCode == rate.CurrencyCode)
            .Select(c => (byte?)c.DecimalDigits)
            .FirstOrDefaultAsync(ct) ?? 2;

        return new MoneyDto
        {
            AmountUsd = amountUsd,
            CurrencyCode = rate.CurrencyCode,
            Amount = Math.Round(amountUsd * rate.USDToCurrency, digits, MidpointRounding.AwayFromZero),
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
