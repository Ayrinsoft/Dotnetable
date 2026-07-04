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

    public ClientWalletService(AppDbContext context) => _context = context;

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
            // Another request created the wallet concurrently (WebsiteClientID is unique) — reload it.
            _context.Entry(wallet).State = EntityState.Detached;
            wallet = await _context.ClientWallets.FirstAsync(w => w.WebsiteClientID == clientId, ct);
        }

        return wallet;
    }

    public async Task<decimal> GetBalanceAsync(int clientId, CancellationToken ct = default)
    {
        var wallet = await _context.ClientWallets.AsNoTracking()
            .FirstOrDefaultAsync(w => w.WebsiteClientID == clientId, ct);
        return wallet?.BalanceUsd ?? 0m;
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
        try
        {
            return await ApplyOnceAsync(websiteId, clientId, type, signedAmountUsd, sourceType, sourceId, note, memberId, ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // The wallet's RowVersion changed under us (concurrent debit/credit) — reload and retry once.
            return await ApplyOnceAsync(websiteId, clientId, type, signedAmountUsd, sourceType, sourceId, note, memberId, ct);
        }
    }

    private async Task<ClientWalletTransaction> ApplyOnceAsync(
        int websiteId,
        int clientId,
        byte type,
        decimal signedAmountUsd,
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
            // Re-attach/reload with tracking so RowVersion is current for the concurrency check.
            if (_context.Entry(wallet).State == EntityState.Detached)
                wallet = await _context.ClientWallets.FirstAsync(w => w.WebsiteClientID == clientId, ct);

            var balanceAfter = wallet.BalanceUsd + signedAmountUsd;
            if (signedAmountUsd < 0 && balanceAfter < 0)
                throw new InvalidOperationException("Insufficient wallet balance.");

            var transaction = new ClientWalletTransaction
            {
                WebsiteID = websiteId,
                ClientWalletID = wallet.ClientWalletID,
                Type = type,
                AmountUsd = signedAmountUsd,
                BalanceAfterUsd = balanceAfter,
                SourceType = sourceType,
                SourceId = sourceId,
                Note = note,
                CreatedByMemberID = memberId,
                CreatedAt = DateTime.UtcNow,
            };
            _context.ClientWalletTransactions.Add(transaction);

            wallet.BalanceUsd = balanceAfter;

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
