using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class BankAccountService : IBankAccountService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public BankAccountService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<List<BankAccount>> GetByWebsiteAsync(int websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.BankAccounts.AsNoTracking()
            .Include(a => a.Bank)
            .Where(a => a.WebsiteID == websiteId)
            .OrderBy(a => a.Title)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<BankAccount>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.BankAccounts.AsNoTracking()
            .Include(a => a.Bank)
            .Where(a => a.WebsiteID == websiteId);

        if (query.GetSearch(nameof(BankAccount.Title)) is string title)
            q = q.Where(a => a.Title.Contains(title));
        if (query.GetSearch(nameof(BankAccount.IBAN)) is string iban)
            q = q.Where(a => a.IBAN != null && a.IBAN.Contains(iban));
        if (query.GetSearch(nameof(BankAccount.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(a => a.IsActive == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(BankAccount.Title))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<BankAccount> { Items = items, TotalCount = total };
    }

    public async Task<BankAccount?> GetByIdAsync(int bankAccountId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.BankAccounts.AsNoTracking()
            .Include(a => a.Bank)
            .FirstOrDefaultAsync(a => a.BankAccountID == bankAccountId, ct);
    }

    public async Task<BankAccount> CreateAsync(BankAccount account, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync(ct);
        return account;
    }

    public async Task<bool> UpdateAsync(BankAccount account, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

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
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.BankAccounts
            .FirstOrDefaultAsync(a => a.BankAccountID == bankAccountId && a.WebsiteID == websiteId, ct);
        if (existing is null) return false;

        _context.BankAccounts.Remove(existing);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<BankAccount>> GetOfflinePaymentAccountsAsync(int websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.BankAccounts.AsNoTracking()
            .Include(a => a.Bank)
            .Where(a => a.WebsiteID == websiteId && a.IsActive && a.IsForOfflinePayment)
            .OrderBy(a => a.Title)
            .ToListAsync(ct);
    }
}
