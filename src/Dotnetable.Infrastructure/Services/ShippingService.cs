using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ShippingService : IShippingService
{
    private readonly AppDbContext _context;

    public ShippingService(AppDbContext context) => _context = context;

    // ── Methods ─────────────────────────────────────────────────────

    public async Task<List<ShippingMethod>> GetAllAsync(int websiteId, CancellationToken ct = default) =>
        await _context.ShippingMethods.AsNoTracking()
            .Where(m => m.WebsiteID == websiteId)
            .OrderBy(m => m.SortOrder).ThenBy(m => m.Title)
            .ToListAsync(ct);

    public async Task<PagedResult<ShippingMethod>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.ShippingMethods.AsNoTracking().Where(m => m.WebsiteID == websiteId);

        if (query.GetSearch(nameof(ShippingMethod.Title)) is string title)
            q = q.Where(m => m.Title.Contains(title));
        if (query.GetSearch(nameof(ShippingMethod.CarrierName)) is string carrier)
            q = q.Where(m => m.CarrierName != null && m.CarrierName.Contains(carrier));
        if (query.GetSearch(nameof(ShippingMethod.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(m => m.IsActive == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(ShippingMethod.SortOrder))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<ShippingMethod> { Items = items, TotalCount = total };
    }

    public async Task<ShippingMethod?> GetByIdAsync(int shippingMethodId, CancellationToken ct = default) =>
        await _context.ShippingMethods.FindAsync([shippingMethodId], ct);

    public async Task<ShippingMethod> CreateAsync(ShippingMethod method, CancellationToken ct = default)
    {
        _context.ShippingMethods.Add(method);
        await _context.SaveChangesAsync(ct);
        return method;
    }

    public async Task<bool> UpdateAsync(ShippingMethod method, CancellationToken ct = default)
    {
        var existing = await _context.ShippingMethods.FirstOrDefaultAsync(m => m.ShippingMethodID == method.ShippingMethodID, ct);
        if (existing is null) return false;

        existing.Title = method.Title;
        existing.CarrierName = method.CarrierName;
        existing.IsActive = method.IsActive;
        existing.SortOrder = method.SortOrder;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int shippingMethodId, CancellationToken ct = default)
    {
        var existing = await _context.ShippingMethods.FindAsync([shippingMethodId], ct);
        if (existing is null) return false;
        _context.ShippingMethods.Remove(existing);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    // ── Rates ───────────────────────────────────────────────────────

    public async Task<List<ShippingRate>> GetRatesAsync(int shippingMethodId, CancellationToken ct = default) =>
        await _context.ShippingRates.AsNoTracking()
            .Include(r => r.Country).Include(r => r.State).Include(r => r.City)
            .Where(r => r.ShippingMethodID == shippingMethodId)
            .OrderBy(r => r.PriceUsd)
            .ToListAsync(ct);

    public async Task<PagedResult<ShippingRate>> GetRatesPagedAsync(int shippingMethodId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.ShippingRates.AsNoTracking()
            .Include(r => r.Country).Include(r => r.State).Include(r => r.City)
            .Where(r => r.ShippingMethodID == shippingMethodId);

        if (query.GetSearch(nameof(ShippingRate.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(r => r.IsActive == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(ShippingRate.PriceUsd))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<ShippingRate> { Items = items, TotalCount = total };
    }

    public async Task<ShippingRate> CreateRateAsync(ShippingRate rate, CancellationToken ct = default)
    {
        _context.ShippingRates.Add(rate);
        await _context.SaveChangesAsync(ct);
        return rate;
    }

    public async Task<bool> UpdateRateAsync(ShippingRate rate, CancellationToken ct = default)
    {
        var existing = await _context.ShippingRates.FirstOrDefaultAsync(r => r.ShippingRateID == rate.ShippingRateID, ct);
        if (existing is null) return false;

        existing.CountryID = rate.CountryID;
        existing.StateID = rate.StateID;
        existing.CityID = rate.CityID;
        existing.MinWeightKg = rate.MinWeightKg;
        existing.MaxWeightKg = rate.MaxWeightKg;
        existing.PriceUsd = rate.PriceUsd;
        existing.IsActive = rate.IsActive;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteRateAsync(int shippingRateId, CancellationToken ct = default)
    {
        var existing = await _context.ShippingRates.FindAsync([shippingRateId], ct);
        if (existing is null) return false;
        _context.ShippingRates.Remove(existing);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    // ── Storefront quote ───────────────────────────────────────────

    public async Task<List<(ShippingMethod Method, decimal PriceUsd)>> GetAvailableWithPricesAsync(
        int websiteId, int? countryId, int? stateId, int? cityId, decimal totalWeightKg, CancellationToken ct = default)
    {
        var methods = await _context.ShippingMethods.AsNoTracking()
            .Where(m => m.WebsiteID == websiteId && m.IsActive)
            .OrderBy(m => m.SortOrder).ThenBy(m => m.Title)
            .ToListAsync(ct);

        if (methods.Count == 0) return new List<(ShippingMethod, decimal)>();

        var methodIds = methods.Select(m => m.ShippingMethodID).ToList();
        var rates = await _context.ShippingRates.AsNoTracking()
            .Where(r => methodIds.Contains(r.ShippingMethodID) && r.IsActive
                && (r.MinWeightKg == null || r.MinWeightKg <= totalWeightKg)
                && (r.MaxWeightKg == null || r.MaxWeightKg >= totalWeightKg))
            .ToListAsync(ct);

        var result = new List<(ShippingMethod, decimal)>();
        foreach (var method in methods)
        {
            var candidates = rates.Where(r => r.ShippingMethodID == method.ShippingMethodID).ToList();

            // Most-specific location match wins: city > state > country > full wildcard.
            var best =
                candidates.FirstOrDefault(r => cityId is not null && r.CityID == cityId) ??
                candidates.FirstOrDefault(r => stateId is not null && r.StateID == stateId && r.CityID is null) ??
                candidates.FirstOrDefault(r => countryId is not null && r.CountryID == countryId && r.StateID is null && r.CityID is null) ??
                candidates.FirstOrDefault(r => r.CountryID is null && r.StateID is null && r.CityID is null);

            if (best is not null)
                result.Add((method, best.PriceUsd));
        }

        return result;
    }
}
