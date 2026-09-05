using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class CurrencyRateService : ICurrencyRateService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public CurrencyRateService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<List<CurrencyRate>> GetByWebsiteAsync(int websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.CurrencyRates.AsNoTracking()
            .Include(r => r.CurrencyCodeNavigation)
            .Where(r => r.WebsiteID == websiteId)
            .OrderByDescending(r => r.IsDefault)
            .ThenBy(r => r.CurrencyCode)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<CurrencyRate>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.CurrencyRates.AsNoTracking()
            .Include(r => r.CurrencyCodeNavigation)
            .Where(r => r.WebsiteID == websiteId);

        if (query.GetSearch(nameof(CurrencyRate.CurrencyCode)) is string code)
            q = q.Where(r => r.CurrencyCode.Contains(code));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(CurrencyRate.CurrencyCode))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<CurrencyRate> { Items = items, TotalCount = total };
    }

    public async Task CreateAsync(CurrencyRate rate, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        ValidateRate(rate);
        rate.LastUpdate = DateTime.UtcNow;
        if (rate.IsDefault)
            await ClearDefaultAsync(_context, rate.WebsiteID, ct);
        else if (!await _context.CurrencyRates.AnyAsync(r => r.WebsiteID == rate.WebsiteID, ct))
            rate.IsDefault = true; // first rate for a website is always the default

        _context.CurrencyRates.Add(rate);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> UpdateAsync(CurrencyRate rate, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        ValidateRate(rate);
        var existing = await _context.CurrencyRates
            .FirstOrDefaultAsync(r => r.CurrencyRateID == rate.CurrencyRateID && r.WebsiteID == rate.WebsiteID, ct);
        if (existing is null) return false;

        existing.CurrencyCode = rate.CurrencyCode;
        existing.USDToCurrency = rate.USDToCurrency;
        existing.LastUpdate = DateTime.UtcNow;

        if (rate.IsDefault && !existing.IsDefault)
            await ClearDefaultAsync(_context, rate.WebsiteID, ct);
        existing.IsDefault = rate.IsDefault;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int currencyRateId, int websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.CurrencyRates
            .FirstOrDefaultAsync(r => r.CurrencyRateID == currencyRateId && r.WebsiteID == websiteId, ct);
        if (existing is null) return false;

        _context.CurrencyRates.Remove(existing);
        await _context.SaveChangesAsync(ct);

        if (existing.IsDefault)
        {
            var next = await _context.CurrencyRates
                .Where(r => r.WebsiteID == websiteId)
                .OrderBy(r => r.CurrencyRateID)
                .FirstOrDefaultAsync(ct);
            if (next is not null)
            {
                next.IsDefault = true;
                await _context.SaveChangesAsync(ct);
            }
        }

        return true;
    }

    public async Task<bool> SetDefaultAsync(int currencyRateId, int websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.CurrencyRates
            .FirstOrDefaultAsync(r => r.CurrencyRateID == currencyRateId && r.WebsiteID == websiteId, ct);
        if (existing is null) return false;

        await ClearDefaultAsync(_context, websiteId, ct);
        existing.IsDefault = true;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<CurrencyRate?> UpdateDefaultUsdToCurrencyAsync(int websiteId, decimal usdToCurrency, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.CurrencyRates
            .Where(r => r.WebsiteID == websiteId)
            .OrderByDescending(r => r.IsDefault)
            .ThenBy(r => r.CurrencyRateID)
            .FirstOrDefaultAsync(ct);
        if (existing is null) return null;

        existing.USDToCurrency = usdToCurrency;
        ValidateRate(existing);
        existing.LastUpdate = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return existing;
    }

    private async Task ClearDefaultAsync(AppDbContext _context, int websiteId, CancellationToken ct)
    {
        await _context.CurrencyRates
            .Where(r => r.WebsiteID == websiteId && r.IsDefault)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsDefault, false), ct);
    }

    /// <summary>
    /// Rates are stored as "how many units of local currency equal 1 USD" (USDToCurrency).
    /// Column is decimal(18,6); values that round to zero are rejected.
    /// </summary>
    private static void ValidateRate(CurrencyRate rate)
    {
        if (string.IsNullOrWhiteSpace(rate.CurrencyCode))
            throw new ArgumentException("Currency code is required.");

        if (rate.USDToCurrency <= 0)
            throw new ArgumentException(
                "Exchange rate must be greater than zero. Enter how many units of the local currency equal 1 USD (e.g. 2000000 for IRR).");

        // Persist with the same scale as the column so tiny inverses don't silently become 0.
        rate.USDToCurrency = decimal.Round(rate.USDToCurrency, 6, MidpointRounding.AwayFromZero);
        if (rate.USDToCurrency <= 0)
            throw new ArgumentException(
                "Exchange rate is too small to store. Prefer entering how many local units equal 1 USD.");
    }
}
