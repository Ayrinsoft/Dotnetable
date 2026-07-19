using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class TaxService : ITaxService
{
    private readonly AppDbContext _context;

    public TaxService(AppDbContext context) => _context = context;

    public async Task<List<TaxRate>> GetAllAsync(int websiteId, CancellationToken ct = default) =>
        await _context.TaxRates.AsNoTracking()
            .Include(r => r.Country).Include(r => r.State)
            .Where(r => r.WebsiteID == websiteId)
            .OrderBy(r => r.Priority).ThenBy(r => r.Title)
            .ToListAsync(ct);

    public async Task<PagedResult<TaxRate>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.TaxRates.AsNoTracking()
            .Include(r => r.Country).Include(r => r.State)
            .Where(r => r.WebsiteID == websiteId);

        if (query.GetSearch(nameof(TaxRate.Title)) is string title)
            q = q.Where(r => r.Title.Contains(title));
        if (query.GetSearch(nameof(TaxRate.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(r => r.IsActive == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(TaxRate.Priority))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<TaxRate> { Items = items, TotalCount = total };
    }

    public async Task<TaxRate?> GetByIdAsync(int taxRateId, CancellationToken ct = default) =>
        await _context.TaxRates.FindAsync([taxRateId], ct);

    public async Task<TaxRate> CreateAsync(TaxRate rate, CancellationToken ct = default)
    {
        _context.TaxRates.Add(rate);
        await _context.SaveChangesAsync(ct);
        return rate;
    }

    public async Task<bool> UpdateAsync(TaxRate rate, CancellationToken ct = default)
    {
        var existing = await _context.TaxRates.FirstOrDefaultAsync(r => r.TaxRateID == rate.TaxRateID, ct);
        if (existing is null) return false;

        existing.Title = rate.Title;
        existing.Rate = rate.Rate;
        existing.CountryID = rate.CountryID;
        existing.StateID = rate.StateID;
        existing.Priority = rate.Priority;
        existing.IsActive = rate.IsActive;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int taxRateId, CancellationToken ct = default)
    {
        var existing = await _context.TaxRates.FindAsync([taxRateId], ct);
        if (existing is null) return false;
        _context.TaxRates.Remove(existing);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<decimal> ComputeTaxAsync(int websiteId, int? countryId, int? stateId, decimal subtotalUsd, CancellationToken ct = default)
    {
        var matching = await _context.TaxRates.AsNoTracking()
            .Where(r => r.WebsiteID == websiteId && r.IsActive
                && (r.CountryID == null || r.CountryID == countryId)
                && (r.StateID == null || r.StateID == stateId))
            .OrderBy(r => r.Priority)
            .ToListAsync(ct);

        return matching.Sum(r => subtotalUsd * r.Rate);
    }
}
