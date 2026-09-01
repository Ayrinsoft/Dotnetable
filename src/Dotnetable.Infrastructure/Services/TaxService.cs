using System.Text.Json;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class TaxService : ITaxService
{
    // Contexts come from DbLease per operation: it joins an ambient transaction when one is in
    // flight and otherwise opens a short-lived context, so nothing is shared across a circuit.
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public TaxService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<List<TaxRate>> GetAllAsync(int websiteId, CancellationToken ct = default)
    {
        await using var _lease = await DbLease.OpenAsync(_contextFactory, ct);
        var _context = _lease.Context;

        return await _context.TaxRates.AsNoTracking()
            .Include(r => r.Country).Include(r => r.State)
            .Where(r => r.WebsiteID == websiteId)
            .OrderBy(r => r.Priority).ThenBy(r => r.Title)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<TaxRate>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
        await using var _lease = await DbLease.OpenAsync(_contextFactory, ct);
        var _context = _lease.Context;

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

    public async Task<TaxRate?> GetByIdAsync(int taxRateId, CancellationToken ct = default)
    {
        await using var _lease = await DbLease.OpenAsync(_contextFactory, ct);
        var _context = _lease.Context;

        return await _context.TaxRates.FindAsync([taxRateId], ct);
    }

    public async Task<TaxRate> CreateAsync(TaxRate rate, CancellationToken ct = default)
    {
        await using var _lease = await DbLease.OpenAsync(_contextFactory, ct);
        var _context = _lease.Context;

        _context.TaxRates.Add(rate);
        await _context.SaveChangesAsync(ct);
        return rate;
    }

    public async Task<bool> UpdateAsync(TaxRate rate, CancellationToken ct = default)
    {
        await using var _lease = await DbLease.OpenAsync(_contextFactory, ct);
        var _context = _lease.Context;

        var existing = await _context.TaxRates.FirstOrDefaultAsync(r => r.TaxRateID == rate.TaxRateID, ct);
        if (existing is null) return false;

        existing.Title = rate.Title;
        existing.TaxCode = rate.TaxCode;
        existing.TaxKind = rate.TaxKind;
        existing.Rate = rate.Rate;
        existing.CountryID = rate.CountryID;
        existing.StateID = rate.StateID;
        existing.Priority = rate.Priority;
        existing.ApplyToShipping = rate.ApplyToShipping;
        existing.IsActive = rate.IsActive;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int taxRateId, CancellationToken ct = default)
    {
        await using var _lease = await DbLease.OpenAsync(_contextFactory, ct);
        var _context = _lease.Context;

        var existing = await _context.TaxRates.FindAsync([taxRateId], ct);
        if (existing is null) return false;
        _context.TaxRates.Remove(existing);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<decimal> ComputeTaxAsync(int websiteId, int? countryId, int? stateId, decimal subtotalUsd, CancellationToken ct = default)
    {
        var result = await ComputeTaxDetailedAsync(websiteId, countryId, stateId, subtotalUsd, 0, ct);
        return result.TaxAmount;
    }

    public async Task<TaxComputationResult> ComputeTaxDetailedAsync(
        int websiteId,
        int? countryId,
        int? stateId,
        decimal merchandiseSubtotal,
        decimal shippingAmount = 0,
        CancellationToken ct = default)
    {
        await using var _lease = await DbLease.OpenAsync(_contextFactory, ct);
        var _context = _lease.Context;

        var website = await _context.Websites.AsNoTracking()
            .FirstOrDefaultAsync(w => w.WebsiteID == websiteId, ct);

        if (website is null || !website.TaxEnabled)
        {
            return new TaxComputationResult
            {
                TaxAmount = 0,
                TaxEnabled = website?.TaxEnabled ?? false,
                PricesIncludeTax = website?.PricesIncludeTax ?? false,
            };
        }

        var matchCountryId = countryId ?? website.TaxCountryID;

        var matching = await _context.TaxRates.AsNoTracking()
            .Where(r => r.WebsiteID == websiteId && r.IsActive
                && (r.CountryID == null || r.CountryID == matchCountryId)
                && (r.StateID == null || r.StateID == stateId))
            .OrderBy(r => r.Priority)
            .ToListAsync(ct);

        if (matching.Count == 0)
        {
            return new TaxComputationResult
            {
                TaxAmount = 0,
                TaxEnabled = true,
                PricesIncludeTax = website.PricesIncludeTax,
            };
        }

        var lines = new List<TaxLineDto>();
        decimal totalTax = 0;

        foreach (var rate in matching)
        {
            var baseMerch = merchandiseSubtotal;
            var baseShip = website.TaxOnShipping && rate.ApplyToShipping ? shippingAmount : 0m;
            var taxable = baseMerch + baseShip;
            if (taxable <= 0 || rate.Rate <= 0) continue;

            decimal amount;
            if (website.PricesIncludeTax)
            {
                // Extract tax already embedded in prices: tax = gross * r / (1 + r) for a single rate;
                // for stacked rates use sequential extraction on remaining gross.
                amount = taxable * rate.Rate / (1m + rate.Rate);
            }
            else
            {
                amount = taxable * rate.Rate;
            }

            amount = Math.Round(amount, 4, MidpointRounding.AwayFromZero);
            totalTax += amount;
            lines.Add(new TaxLineDto
            {
                Code = rate.TaxCode,
                Title = rate.Title,
                TaxKind = rate.TaxKind,
                Rate = rate.Rate,
                Amount = amount,
                AppliedToShipping = baseShip > 0,
            });
        }

        var breakdown = lines.Count == 0
            ? null
            : JsonSerializer.Serialize(lines.Select(l => new
            {
                code = l.Code,
                title = l.Title,
                kind = l.TaxKind,
                rate = l.Rate,
                amount = l.Amount,
                onShipping = l.AppliedToShipping,
            }));

        return new TaxComputationResult
        {
            TaxAmount = totalTax,
            TaxEnabled = true,
            PricesIncludeTax = website.PricesIncludeTax,
            BreakdownJson = breakdown,
            Lines = lines,
        };
    }
}
