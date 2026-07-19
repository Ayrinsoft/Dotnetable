using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class CurrencyService : ICurrencyService
{
    private readonly AppDbContext _context;

    public CurrencyService(AppDbContext context) => _context = context;

    public async Task<List<Currency>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Currencies.AsNoTracking().OrderBy(c => c.CurrencyCode).ToListAsync(ct);

    public async Task<PagedResult<Currency>> GetPagedAsync(GridQuery query, CancellationToken ct = default)
    {
        var q = _context.Currencies.AsNoTracking();

        if (query.GetSearch(nameof(Currency.CurrencyCode)) is string code)
            q = q.Where(c => c.CurrencyCode.Contains(code));
        if (query.GetSearch(nameof(Currency.Name)) is string name)
            q = q.Where(c => c.Name.Contains(name));
        if (query.GetSearch(nameof(Currency.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(c => c.IsActive == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Currency.CurrencyCode))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Currency> { Items = items, TotalCount = total };
    }

    public async Task<Currency?> GetByCodeAsync(string currencyCode, CancellationToken ct = default) =>
        await _context.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.CurrencyCode == currencyCode, ct);

    public async Task CreateAsync(Currency currency, CancellationToken ct = default)
    {
        _context.Currencies.Add(currency);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> UpdateAsync(Currency currency, CancellationToken ct = default)
    {
        var existing = await _context.Currencies.FirstOrDefaultAsync(c => c.CurrencyCode == currency.CurrencyCode, ct);
        if (existing is null) return false;

        existing.Name = currency.Name;
        existing.Symbol = currency.Symbol;
        existing.DecimalDigits = currency.DecimalDigits;
        existing.IsActive = currency.IsActive;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(string currencyCode, CancellationToken ct = default)
    {
        var existing = await _context.Currencies.FirstOrDefaultAsync(c => c.CurrencyCode == currencyCode, ct);
        if (existing is null) return false;

        _context.Currencies.Remove(existing);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
