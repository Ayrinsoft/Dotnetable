using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ShippingService : IShippingService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ICurrencyConversionService _currency;

    public ShippingService(IDbContextFactory<AppDbContext> contextFactory, ICurrencyConversionService currency)
    {
        _contextFactory = contextFactory;
        _currency = currency;
    }

    // ── Methods ─────────────────────────────────────────────────────

    public async Task<List<ShippingMethod>> GetAllAsync(int websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.ShippingMethods.AsNoTracking()
            .Include(m => m.LogoFile)
            .Where(m => m.WebsiteID == websiteId)
            .OrderBy(m => m.SortOrder).ThenBy(m => m.Title)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<ShippingMethod>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.ShippingMethods.AsNoTracking()
            .Include(m => m.LogoFile)
            .Where(m => m.WebsiteID == websiteId);

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

    public async Task<ShippingMethod?> GetByIdAsync(int shippingMethodId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.ShippingMethods
            .Include(m => m.LogoFile)
            .FirstOrDefaultAsync(m => m.ShippingMethodID == shippingMethodId, ct);
    }

    public async Task<ShippingMethod> CreateAsync(ShippingMethod method, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        await NormalizeMethodPricesAsync(method, ct);
        _context.ShippingMethods.Add(method);
        await _context.SaveChangesAsync(ct);
        return method;
    }

    public async Task<bool> UpdateAsync(ShippingMethod method, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.ShippingMethods.FirstOrDefaultAsync(m => m.ShippingMethodID == method.ShippingMethodID, ct);
        if (existing is null) return false;

        await NormalizeMethodPricesAsync(method, ct);

        existing.Title = method.Title;
        existing.CarrierName = method.CarrierName;
        existing.LogoFileID = method.LogoFileID;
        existing.SupportsPrepaid = method.SupportsPrepaid;
        existing.SupportsCod = method.SupportsCod;
        existing.PrepaidMinPrice = method.PrepaidMinPrice;
        existing.PrepaidMinPriceUsd = method.PrepaidMinPriceUsd;
        existing.CodMinPrice = method.CodMinPrice;
        existing.CodMinPriceUsd = method.CodMinPriceUsd;
        existing.FreeShippingMinOrderAmount = method.FreeShippingMinOrderAmount;
        existing.FreeShippingMinOrderAmountUsd = method.FreeShippingMinOrderAmountUsd;
        existing.IsActive = method.IsActive;
        existing.SortOrder = method.SortOrder;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int shippingMethodId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.ShippingMethods.FindAsync([shippingMethodId], ct);
        if (existing is null) return false;
        _context.ShippingMethods.Remove(existing);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    // ── Rates ───────────────────────────────────────────────────────

    public async Task<List<ShippingRate>> GetRatesAsync(int shippingMethodId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.ShippingRates.AsNoTracking()
            .Include(r => r.Country).Include(r => r.State).Include(r => r.City)
            .Where(r => r.ShippingMethodID == shippingMethodId)
            .OrderBy(r => r.PriceUsd)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<ShippingRate>> GetRatesPagedAsync(int shippingMethodId, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

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
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        await NormalizeRatePriceAsync(_context, rate, ct);
        _context.ShippingRates.Add(rate);
        await _context.SaveChangesAsync(ct);
        return rate;
    }

    public async Task<bool> UpdateRateAsync(ShippingRate rate, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.ShippingRates.FirstOrDefaultAsync(r => r.ShippingRateID == rate.ShippingRateID, ct);
        if (existing is null) return false;

        await NormalizeRatePriceAsync(_context, rate, ct);

        existing.CountryID = rate.CountryID;
        existing.StateID = rate.StateID;
        existing.CityID = rate.CityID;
        existing.MinWeightKg = rate.MinWeightKg;
        existing.MaxWeightKg = rate.MaxWeightKg;
        existing.Price = rate.Price;
        existing.PriceUsd = rate.PriceUsd;
        existing.IsActive = rate.IsActive;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    private async Task NormalizeMethodPricesAsync(ShippingMethod method, CancellationToken ct)
    {
        if (method.FreeShippingMinOrderAmount <= 0)
        {
            method.FreeShippingMinOrderAmount = 0;
            method.FreeShippingMinOrderAmountUsd = 0;
        }

        try
        {
            if (method.PrepaidMinPrice > 0)
                method.PrepaidMinPriceUsd = await _currency.ToUsdAsync(method.WebsiteID, method.PrepaidMinPrice, null, ct);
            else if (method.PrepaidMinPriceUsd > 0)
            {
                var m = await _currency.ToDisplayAsync(method.WebsiteID, method.PrepaidMinPriceUsd, null, ct);
                method.PrepaidMinPrice = m.Amount;
            }

            if (method.CodMinPrice > 0)
                method.CodMinPriceUsd = await _currency.ToUsdAsync(method.WebsiteID, method.CodMinPrice, null, ct);
            else if (method.CodMinPriceUsd > 0)
            {
                var m = await _currency.ToDisplayAsync(method.WebsiteID, method.CodMinPriceUsd, null, ct);
                method.CodMinPrice = m.Amount;
            }

            if (method.FreeShippingMinOrderAmount > 0)
                method.FreeShippingMinOrderAmountUsd = await _currency.ToUsdAsync(method.WebsiteID, method.FreeShippingMinOrderAmount, null, ct);
        }
        catch (InvalidOperationException)
        {
            if (method.PrepaidMinPrice <= 0) method.PrepaidMinPrice = method.PrepaidMinPriceUsd;
            if (method.PrepaidMinPriceUsd <= 0) method.PrepaidMinPriceUsd = method.PrepaidMinPrice;
            if (method.CodMinPrice <= 0) method.CodMinPrice = method.CodMinPriceUsd;
            if (method.CodMinPriceUsd <= 0) method.CodMinPriceUsd = method.CodMinPrice;
            if (method.FreeShippingMinOrderAmount > 0 && method.FreeShippingMinOrderAmountUsd <= 0)
                method.FreeShippingMinOrderAmountUsd = method.FreeShippingMinOrderAmount;
        }
    }

    private async Task NormalizeRatePriceAsync(AppDbContext _context, ShippingRate rate, CancellationToken ct)
    {
        var method = await _context.ShippingMethods.AsNoTracking()
            .FirstOrDefaultAsync(m => m.ShippingMethodID == rate.ShippingMethodID, ct);
        if (method is null) return;

        try
        {
            if (rate.Price > 0)
                rate.PriceUsd = await _currency.ToUsdAsync(method.WebsiteID, rate.Price, null, ct);
            else if (rate.PriceUsd > 0)
            {
                var m = await _currency.ToDisplayAsync(method.WebsiteID, rate.PriceUsd, null, ct);
                rate.Price = m.Amount;
            }
        }
        catch (InvalidOperationException)
        {
            if (rate.Price <= 0) rate.Price = rate.PriceUsd;
            if (rate.PriceUsd <= 0) rate.PriceUsd = rate.Price;
        }
    }

    public async Task<bool> DeleteRateAsync(int shippingRateId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.ShippingRates.FindAsync([shippingRateId], ct);
        if (existing is null) return false;
        _context.ShippingRates.Remove(existing);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    // ── Storefront quote ───────────────────────────────────────────

    public async Task<List<ShippingQuoteDto>> GetAvailableWithPricesAsync(
        int websiteId,
        int? countryId,
        int? stateId,
        int? cityId,
        decimal totalWeightKg,
        decimal cartSubtotalLocal = 0,
        CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var website = await _context.Websites.AsNoTracking()
            .FirstOrDefaultAsync(w => w.WebsiteID == websiteId, ct);
        var allowCod = website?.AllowCashOnDelivery ?? true;
        var siteFreeLocal = website?.FreeShippingMinOrderAmount ?? 0m;

        var methods = await _context.ShippingMethods.AsNoTracking()
            .Include(m => m.LogoFile)
            .Where(m => m.WebsiteID == websiteId && m.IsActive && (m.SupportsPrepaid || m.SupportsCod))
            .OrderBy(m => m.SortOrder).ThenBy(m => m.Title)
            .ToListAsync(ct);

        if (methods.Count == 0) return new List<ShippingQuoteDto>();

        var methodIds = methods.Select(m => m.ShippingMethodID).ToList();
        var rates = await _context.ShippingRates.AsNoTracking()
            .Where(r => methodIds.Contains(r.ShippingMethodID) && r.IsActive
                && (r.MinWeightKg == null || r.MinWeightKg <= totalWeightKg)
                && (r.MaxWeightKg == null || r.MaxWeightKg >= totalWeightKg))
            .ToListAsync(ct);

        var siteFreeShipping = siteFreeLocal > 0 && cartSubtotalLocal >= siteFreeLocal;

        var result = new List<ShippingQuoteDto>();
        foreach (var method in methods)
        {
            var candidates = rates.Where(r => r.ShippingMethodID == method.ShippingMethodID).ToList();

            // Most-specific location match wins: city > state > country > full wildcard.
            var best =
                candidates.FirstOrDefault(r => cityId is not null && r.CityID == cityId) ??
                candidates.FirstOrDefault(r => stateId is not null && r.StateID == stateId && r.CityID is null) ??
                candidates.FirstOrDefault(r => countryId is not null && r.CountryID == countryId && r.StateID is null && r.CityID is null) ??
                candidates.FirstOrDefault(r => r.CountryID is null && r.StateID is null && r.CityID is null);

            decimal? zoneUsd = null;
            if (best is not null)
            {
                var local = best.Price > 0 ? best.Price : best.PriceUsd;
                zoneUsd = best.PriceUsd > 0 ? best.PriceUsd : local;
                try
                {
                    if (best.Price > 0)
                        zoneUsd = await _currency.ToUsdAsync(websiteId, best.Price, null, ct);
                }
                catch (InvalidOperationException)
                {
                    zoneUsd = local;
                }
            }

            // When no zone rate exists, method min prices alone still allow quoting
            // (so carriers with only prepaid/COD floors work without rate rows).
            var baseUsd = zoneUsd ?? 0m;

            var methodFreeLocal = method.FreeShippingMinOrderAmount > 0
                ? method.FreeShippingMinOrderAmount
                : 0m;
            var methodFreeShipping = methodFreeLocal > 0 && cartSubtotalLocal >= methodFreeLocal;
            var isFreeShipping = siteFreeShipping || methodFreeShipping;

            decimal? prepaidUsd = null;
            if (method.SupportsPrepaid)
            {
                if (isFreeShipping)
                {
                    prepaidUsd = 0m;
                }
                else
                {
                    var floor = method.PrepaidMinPriceUsd > 0
                        ? method.PrepaidMinPriceUsd
                        : method.PrepaidMinPrice;
                    try
                    {
                        if (method.PrepaidMinPrice > 0)
                            floor = await _currency.ToUsdAsync(websiteId, method.PrepaidMinPrice, null, ct);
                    }
                    catch (InvalidOperationException)
                    {
                        floor = method.PrepaidMinPrice > 0 ? method.PrepaidMinPrice : method.PrepaidMinPriceUsd;
                    }
                    prepaidUsd = Math.Max(baseUsd, floor);
                }
            }

            decimal? codUsd = null;
            if (method.SupportsCod && allowCod)
            {
                if (isFreeShipping)
                {
                    codUsd = 0m;
                }
                else
                {
                    var floor = method.CodMinPriceUsd > 0
                        ? method.CodMinPriceUsd
                        : method.CodMinPrice;
                    try
                    {
                        if (method.CodMinPrice > 0)
                            floor = await _currency.ToUsdAsync(websiteId, method.CodMinPrice, null, ct);
                    }
                    catch (InvalidOperationException)
                    {
                        floor = method.CodMinPrice > 0 ? method.CodMinPrice : method.CodMinPriceUsd;
                    }
                    codUsd = Math.Max(baseUsd, floor);
                }
            }

            if (prepaidUsd is null && codUsd is null)
                continue;

            result.Add(new ShippingQuoteDto
            {
                Method = method,
                ZoneRateUsd = zoneUsd,
                PrepaidPriceUsd = prepaidUsd,
                CodPriceUsd = codUsd,
                IsFreeShipping = isFreeShipping,
            });
        }

        return result;
    }
}
