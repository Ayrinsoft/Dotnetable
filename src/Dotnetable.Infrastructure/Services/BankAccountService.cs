using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class BankAccountService : IBankAccountService
{
    private readonly AppDbContext _context;

    public BankAccountService(AppDbContext context) => _context = context;

    public async Task<List<BankAccount>> GetByWebsiteAsync(int websiteId, CancellationToken ct = default) =>
        await _context.BankAccounts.AsNoTracking()
            .Include(a => a.Bank)
            .Where(a => a.WebsiteID == websiteId)
            .OrderBy(a => a.Title)
            .ToListAsync(ct);

    public async Task<BankAccount?> GetByIdAsync(int bankAccountId, CancellationToken ct = default) =>
        await _context.BankAccounts.AsNoTracking()
            .Include(a => a.Bank)
            .FirstOrDefaultAsync(a => a.BankAccountID == bankAccountId, ct);

    public async Task<BankAccount> CreateAsync(BankAccount account, CancellationToken ct = default)
    {
        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync(ct);
        return account;
    }

    public async Task<bool> UpdateAsync(BankAccount account, CancellationToken ct = default)
    {
        var existing = await _context.BankAccounts
            .FirstOrDefaultAsync(a => a.BankAccountID == account.BankAccountID && a.WebsiteID == account.WebsiteID, ct);
        if (existing is null) return false;

        existing.BankID = account.BankID;
        existing.Title = account.Title;
        existing.OwnerName = account.OwnerName;
        existing.AccountNumber = account.AccountNumber;
        existing.IBAN = account.IBAN;
        existing.CardNumber = account.CardNumber;
        existing.IsForOfflinePayment = account.IsForOfflinePayment;
        existing.IsActive = account.IsActive;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int bankAccountId, int websiteId, CancellationToken ct = default)
    {
        var existing = await _context.BankAccounts
            .FirstOrDefaultAsync(a => a.BankAccountID == bankAccountId && a.WebsiteID == websiteId, ct);
        if (existing is null) return false;

        _context.BankAccounts.Remove(existing);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<BankAccount>> GetOfflinePaymentAccountsAsync(int websiteId, CancellationToken ct = default) =>
        await _context.BankAccounts.AsNoTracking()
            .Include(a => a.Bank)
            .Where(a => a.WebsiteID == websiteId && a.IsActive && a.IsForOfflinePayment)
            .OrderBy(a => a.Title)
            .ToListAsync(ct);
}
