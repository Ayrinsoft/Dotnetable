using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class BankService : IBankService
{
    private readonly AppDbContext _context;

    public BankService(AppDbContext context) => _context = context;

    public async Task<List<Bank>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Banks.AsNoTracking().OrderBy(b => b.Name).ToListAsync(ct);

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
