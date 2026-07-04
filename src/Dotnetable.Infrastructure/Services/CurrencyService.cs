using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class CurrencyService : ICurrencyService
{
    private readonly AppDbContext _context;

    public CurrencyService(AppDbContext context) => _context = context;

    public async Task<List<Currency>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Currencies.AsNoTracking().OrderBy(c => c.CurrencyCode).ToListAsync(ct);

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
