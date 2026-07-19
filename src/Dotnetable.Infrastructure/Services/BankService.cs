using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class BankService : IBankService
{
    private readonly AppDbContext _context;

    public BankService(AppDbContext context) => _context = context;

    public async Task<List<Bank>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Banks.AsNoTracking().OrderBy(b => b.Name).ToListAsync(ct);

    public async Task<PagedResult<Bank>> GetPagedAsync(GridQuery query, CancellationToken ct = default)
    {
        var q = _context.Banks.AsNoTracking();

        if (query.GetSearch(nameof(Bank.Name)) is string name)
            q = q.Where(b => b.Name.Contains(name));
        if (query.GetSearch(nameof(Bank.BankCode)) is string code)
            q = q.Where(b => b.BankCode.Contains(code));
        if (query.GetSearch(nameof(Bank.Active)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(b => b.Active == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Bank.Name))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Bank> { Items = items, TotalCount = total };
    }

    public async Task<Bank?> GetByIdAsync(int bankId, CancellationToken ct = default) =>
        await _context.Banks.FindAsync([bankId], ct);

    public async Task<Bank> CreateAsync(Bank bank, CancellationToken ct = default)
    {
        _context.Banks.Add(bank);
        await _context.SaveChangesAsync(ct);
        return bank;
    }

    public async Task<bool> UpdateAsync(Bank bank, CancellationToken ct = default)
    {
        var existing = await _context.Banks.FirstOrDefaultAsync(b => b.BankID == bank.BankID, ct);
        if (existing is null) return false;

        existing.Name = bank.Name;
        existing.BankCode = bank.BankCode;
        existing.LogoFileID = bank.LogoFileID;
        existing.Active = bank.Active;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int bankId, CancellationToken ct = default)
    {
        var existing = await _context.Banks.FirstOrDefaultAsync(b => b.BankID == bankId, ct);
        if (existing is null) return false;

        _context.Banks.Remove(existing);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
