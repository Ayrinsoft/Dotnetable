using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ClientWalletService : IClientWalletService
{
    private readonly AppDbContext _context;
    private readonly ICurrencyConversionService _currency;

    public ClientWalletService(AppDbContext context, ICurrencyConversionService currency)
    {
        _context = context;
        _currency = currency;
    }

    public async Task<ClientWallet> GetOrCreateAsync(int websiteId, int clientId, CancellationToken ct = default)
    {
        var wallet = await _context.ClientWallets
            .FirstOrDefaultAsync(w => w.WebsiteClientID == clientId, ct);
        if (wallet is not null)
            return wallet;

        wallet = new ClientWallet
        {
            WebsiteID = websiteId,
            WebsiteClientID = clientId,
            Balance = 0,
            BalanceUsd = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        _context.ClientWallets.Add(wallet);
        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            _context.Entry(wallet).State = EntityState.Detached;
            wallet = await _context.ClientWallets.FirstAsync(w => w.WebsiteClientID == clientId, ct);
        }

        return wallet;
    }

    public async Task<decimal> GetBalanceAsync(int clientId, CancellationToken ct = default)
    {
        var wallet = await _context.ClientWallets.AsNoTracking()
            .FirstOrDefaultAsync(w => w.WebsiteClientID == clientId, ct);
        if (wallet is null) return 0m;
        // Site currency is authority; fall back to USD for legacy rows.
        return wallet.Balance != 0 || wallet.BalanceUsd == 0 ? wallet.Balance : wallet.BalanceUsd;
    }

    public async Task<PagedResult<ClientWalletTransaction>> GetHistoryAsync(int clientId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.ClientWalletTransactions.AsNoTracking()
            .Where(t => t.ClientWallet.WebsiteClientID == clientId);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(ClientWalletTransaction.ClientWalletTransactionID), fallbackDescending: true)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<ClientWalletTransaction> { Items = items, TotalCount = total };
    }

    public async Task<ClientWalletTransaction> ApplyAsync(
        int websiteId,
        int clientId,
        byte type,
        decimal signedAmountUsd,
        byte? sourceType,
        int? sourceId,
        string? note,
        int? memberId,
        CancellationToken ct = default)
    {
        // Parameter name kept for API compatibility; amount is operational site-currency amount.
        // Dual USD is derived via rates when available.
        try
        {
            return await ApplyOnceAsync(websiteId, clientId, type, signedAmountUsd, sourceType, sourceId, note, memberId, ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return await ApplyOnceAsync(websiteId, clientId, type, signedAmountUsd, sourceType, sourceId, note, memberId, ct);
        }
    }

    private async Task<ClientWalletTransaction> ApplyOnceAsync(
        int websiteId,
        int clientId,
        byte type,
        decimal signedAmountLocal,
        byte? sourceType,
        int? sourceId,
        string? note,
        int? memberId,
        CancellationToken ct)
    {
        await using var tx = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            var wallet = await GetOrCreateAsync(websiteId, clientId, ct);
            if (_context.Entry(wallet).State == EntityState.Detached)
                wallet = await _context.ClientWallets.FirstAsync(w => w.WebsiteClientID == clientId, ct);

            // Prefer site balance; bootstrap from USD when local is still zero after legacy data.
            var currentLocal = wallet.Balance != 0 || wallet.BalanceUsd == 0
                ? wallet.Balance
                : wallet.BalanceUsd;

            var balanceAfterLocal = currentLocal + signedAmountLocal;
            if (signedAmountLocal < 0 && balanceAfterLocal < 0)
                throw new InvalidOperationException("Insufficient wallet balance.");

            decimal signedAmountUsd;
            decimal balanceAfterUsd;
            try
            {
                signedAmountUsd = await _currency.ToUsdAsync(websiteId, signedAmountLocal, null, ct);
                balanceAfterUsd = await _currency.ToUsdAsync(websiteId, balanceAfterLocal, null, ct);
            }
            catch (InvalidOperationException)
            {
                signedAmountUsd = signedAmountLocal;
                balanceAfterUsd = balanceAfterLocal;
            }

            var transaction = new ClientWalletTransaction
            {
                WebsiteID = websiteId,
                ClientWalletID = wallet.ClientWalletID,
                Type = type,
                Amount = signedAmountLocal,
                AmountUsd = signedAmountUsd,
                BalanceAfter = balanceAfterLocal,
                BalanceAfterUsd = balanceAfterUsd,
                SourceType = sourceType,
                SourceId = sourceId,
                Note = note,
                CreatedByMemberID = memberId,
                CreatedAt = DateTime.UtcNow,
            };
            _context.ClientWalletTransactions.Add(transaction);

            wallet.Balance = balanceAfterLocal;
            wallet.BalanceUsd = balanceAfterUsd;

            await _context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return transaction;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
