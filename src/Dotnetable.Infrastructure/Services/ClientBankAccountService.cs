using Dotnetable.Application;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ClientBankAccountService : IClientBankAccountService
{
    private readonly AppDbContext _context;

    public ClientBankAccountService(AppDbContext context) => _context = context;

    public async Task<List<ClientBankAccount>> GetByClientIdAsync(int clientId, CancellationToken ct = default) =>
        await _context.ClientBankAccounts.AsNoTracking()
            .Include(a => a.Bank)
            .Where(a => a.WebsiteClientID == clientId && a.IsActive)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.ClientBankAccountID)
            .ToListAsync(ct);

    public async Task<ClientBankAccount?> GetByIdAsync(int bankAccountId, int clientId, CancellationToken ct = default) =>
        await _context.ClientBankAccounts.AsNoTracking()
            .Include(a => a.Bank)
            .FirstOrDefaultAsync(a => a.ClientBankAccountID == bankAccountId && a.WebsiteClientID == clientId && a.IsActive, ct);

    public async Task<BankAccountSaveResult> CreateAsync(ClientBankAccount account, CancellationToken ct = default)
    {
        var count = await _context.ClientBankAccounts
            .CountAsync(a => a.WebsiteClientID == account.WebsiteClientID && a.IsActive, ct);
        if (count >= AppConstants.MaxClientBankAccounts)
            return BankAccountSaveResult.LimitReached;

        account.IsActive = true;
        account.CreatedAt = DateTime.UtcNow;
        if (account.IsDefault)
            await ClearDefaultAsync(account.WebsiteClientID, ct);
        else if (count == 0)
            account.IsDefault = true; // first bank account is always the default

        _context.ClientBankAccounts.Add(account);
        await _context.SaveChangesAsync(ct);
        return BankAccountSaveResult.Success;
    }

    public async Task<BankAccountSaveResult> UpdateAsync(ClientBankAccount account, CancellationToken ct = default)
    {
        var existing = await _context.ClientBankAccounts
            .FirstOrDefaultAsync(a => a.ClientBankAccountID == account.ClientBankAccountID
                                       && a.WebsiteClientID == account.WebsiteClientID && a.IsActive, ct);
        if (existing is null)
            return BankAccountSaveResult.NotFound;

        existing.BankID = account.BankID;
        existing.OwnerName = account.OwnerName;
        existing.AccountNumber = account.AccountNumber;
        existing.IBAN = account.IBAN;
        existing.CardNumber = account.CardNumber;

        if (account.IsDefault && !existing.IsDefault)
            await ClearDefaultAsync(account.WebsiteClientID, ct);
        existing.IsDefault = account.IsDefault;

        await _context.SaveChangesAsync(ct);
        return BankAccountSaveResult.Success;
    }

    public async Task<bool> DeleteAsync(int bankAccountId, int clientId, CancellationToken ct = default)
    {
        var account = await _context.ClientBankAccounts
            .FirstOrDefaultAsync(a => a.ClientBankAccountID == bankAccountId && a.WebsiteClientID == clientId && a.IsActive, ct);
        if (account is null) return false;

        // Soft-delete: a bank account may already be referenced by a ClientWalletWithdrawal
        // row (FK, no cascade), so it can never be hard-removed once used.
        var wasDefault = account.IsDefault;
        account.IsActive = false;
        account.IsDefault = false;
        await _context.SaveChangesAsync(ct);

        // Promote another bank account to default when the deleted one was it.
        if (wasDefault)
        {
            var next = await _context.ClientBankAccounts
                .Where(a => a.WebsiteClientID == clientId && a.IsActive)
                .OrderBy(a => a.ClientBankAccountID)
                .FirstOrDefaultAsync(ct);
            if (next is not null)
            {
                next.IsDefault = true;
                await _context.SaveChangesAsync(ct);
            }
        }

        return true;
    }

    public async Task<bool> SetDefaultAsync(int bankAccountId, int clientId, CancellationToken ct = default)
    {
        var account = await _context.ClientBankAccounts
            .FirstOrDefaultAsync(a => a.ClientBankAccountID == bankAccountId && a.WebsiteClientID == clientId && a.IsActive, ct);
        if (account is null) return false;

        await ClearDefaultAsync(clientId, ct);
        account.IsDefault = true;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    private async Task ClearDefaultAsync(int clientId, CancellationToken ct) =>
        await _context.ClientBankAccounts
            .Where(a => a.WebsiteClientID == clientId && a.IsDefault)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false), ct);
}
