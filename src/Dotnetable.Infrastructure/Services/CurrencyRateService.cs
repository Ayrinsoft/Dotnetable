using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class CurrencyRateService : ICurrencyRateService
{
    private readonly AppDbContext _context;

    public CurrencyRateService(AppDbContext context) => _context = context;

    public async Task<List<CurrencyRate>> GetByWebsiteAsync(int websiteId, CancellationToken ct = default) =>
        await _context.CurrencyRates.AsNoTracking()
            .Include(r => r.CurrencyCodeNavigation)
            .Where(r => r.WebsiteID == websiteId)
            .OrderByDescending(r => r.IsDefault)
            .ThenBy(r => r.CurrencyCode)
            .ToListAsync(ct);

    public async Task<PagedResult<CurrencyRate>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
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
        rate.LastUpdate = DateTime.UtcNow;
        if (rate.IsDefault)
            await ClearDefaultAsync(rate.WebsiteID, ct);
        else if (!await _context.CurrencyRates.AnyAsync(r => r.WebsiteID == rate.WebsiteID, ct))
            rate.IsDefault = true; // first rate for a website is always the default

        _context.CurrencyRates.Add(rate);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> UpdateAsync(CurrencyRate rate, CancellationToken ct = default)
    {
        var existing = await _context.CurrencyRates
            .FirstOrDefaultAsync(r => r.CurrencyRateID == rate.CurrencyRateID && r.WebsiteID == rate.WebsiteID, ct);
        if (existing is null) return false;

        existing.CurrencyCode = rate.CurrencyCode;
        existing.USDToCurrency = rate.USDToCurrency;
        existing.LastUpdate = DateTime.UtcNow;

        if (rate.IsDefault && !existing.IsDefault)
            await ClearDefaultAsync(rate.WebsiteID, ct);
        existing.IsDefault = rate.IsDefault;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int currencyRateId, int websiteId, CancellationToken ct = default)
    {
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
        var existing = await _context.CurrencyRates
            .FirstOrDefaultAsync(r => r.CurrencyRateID == currencyRateId && r.WebsiteID == websiteId, ct);
        if (existing is null) return false;

        await ClearDefaultAsync(websiteId, ct);
        existing.IsDefault = true;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    private async Task ClearDefaultAsync(int websiteId, CancellationToken ct) =>
        await _context.CurrencyRates
            .Where(r => r.WebsiteID == websiteId && r.IsDefault)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsDefault, false), ct);
}
