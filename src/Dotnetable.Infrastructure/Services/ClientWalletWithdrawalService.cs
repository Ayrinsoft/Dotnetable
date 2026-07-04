using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ClientWalletWithdrawalService : IClientWalletWithdrawalService
{
    private readonly AppDbContext _context;
    private readonly IClientWalletService _wallets;

    public ClientWalletWithdrawalService(AppDbContext context, IClientWalletService wallets)
    {
        _context = context;
        _wallets = wallets;
    }

    public async Task<ClientWalletWithdrawal> RequestAsync(int websiteId, int clientId, int clientBankAccountId, decimal amountUsd, CancellationToken ct = default)
    {
        if (amountUsd <= 0)
            throw new InvalidOperationException("Withdrawal amount must be greater than zero.");

        var bankAccount = await _context.ClientBankAccounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.ClientBankAccountID == clientBankAccountId && a.WebsiteClientID == clientId && a.IsActive, ct);
        if (bankAccount is null)
            throw new InvalidOperationException("Bank account not found.");

        // ApplyAsync itself guards against an insufficient balance (throws InvalidOperationException).
        var holdTransaction = await _wallets.ApplyAsync(
            websiteId, clientId,
            (byte)ClientWalletTransactionType.WithdrawalHold,
            -amountUsd,
            (byte)ClientWalletSourceType.ClientWalletWithdrawal,
            null,
            "Withdrawal hold",
            memberId: null,
            ct);

        var withdrawal = new ClientWalletWithdrawal
        {
            WebsiteID = websiteId,
            WebsiteClientID = clientId,
            ClientWalletID = holdTransaction.ClientWalletID,
            ClientBankAccountID = clientBankAccountId,
            AmountUsd = amountUsd,
            Status = (byte)ClientWalletWithdrawalStatus.Pending,
            RequestedAt = DateTime.UtcNow,
        };
        _context.ClientWalletWithdrawals.Add(withdrawal);
        await _context.SaveChangesAsync(ct);

        // Link the hold transaction back to this withdrawal now that we have its id.
        holdTransaction.SourceId = withdrawal.ClientWalletWithdrawalID;
        await _context.SaveChangesAsync(ct);

        return withdrawal;
    }

    public async Task<PagedResult<ClientWalletWithdrawal>> GetPagedAsync(int? websiteId, byte? status, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.ClientWalletWithdrawals.AsNoTracking()
            .Include(w => w.WebsiteClient)
            .Include(w => w.ClientBankAccount)
            .AsQueryable();

        if (websiteId is int wid)
            q = q.Where(w => w.WebsiteID == wid);
        if (status is byte s)
            q = q.Where(w => w.Status == s);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(ClientWalletWithdrawal.RequestedAt), fallbackDescending: true)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<ClientWalletWithdrawal> { Items = items, TotalCount = total };
    }

    public async Task<PagedResult<ClientWalletWithdrawal>> GetByClientIdAsync(int clientId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.ClientWalletWithdrawals.AsNoTracking()
            .Include(w => w.ClientBankAccount)
            .Where(w => w.WebsiteClientID == clientId);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(ClientWalletWithdrawal.RequestedAt), fallbackDescending: true)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<ClientWalletWithdrawal> { Items = items, TotalCount = total };
    }

    public async Task<bool> ApproveAsync(int withdrawalId, int memberId, string? paymentRefNumber, CancellationToken ct = default)
    {
        var withdrawal = await _context.ClientWalletWithdrawals
            .FirstOrDefaultAsync(w => w.ClientWalletWithdrawalID == withdrawalId, ct);
        if (withdrawal is null || withdrawal.Status != (byte)ClientWalletWithdrawalStatus.Pending)
            return false;

        withdrawal.Status = (byte)ClientWalletWithdrawalStatus.Paid;
        withdrawal.ReviewedByMemberID = memberId;
        withdrawal.ReviewedAt = DateTime.UtcNow;
        withdrawal.PaymentRefNumber = paymentRefNumber;
        withdrawal.PaidAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> RejectAsync(int withdrawalId, int memberId, string reason, CancellationToken ct = default)
    {
        var withdrawal = await _context.ClientWalletWithdrawals
            .FirstOrDefaultAsync(w => w.ClientWalletWithdrawalID == withdrawalId, ct);
        if (withdrawal is null || withdrawal.Status != (byte)ClientWalletWithdrawalStatus.Pending)
            return false;

        // Reverse the held funds back into the customer's wallet.
        await _wallets.ApplyAsync(
            withdrawal.WebsiteID, withdrawal.WebsiteClientID,
            (byte)ClientWalletTransactionType.WithdrawalReversed,
            withdrawal.AmountUsd,
            (byte)ClientWalletSourceType.ClientWalletWithdrawal,
            withdrawal.ClientWalletWithdrawalID,
            "Withdrawal rejected",
            memberId,
            ct);

        withdrawal.Status = (byte)ClientWalletWithdrawalStatus.Rejected;
        withdrawal.ReviewedByMemberID = memberId;
        withdrawal.ReviewedAt = DateTime.UtcNow;
        withdrawal.RejectReason = reason;

        await _context.SaveChangesAsync(ct);
        return true;
    }
}
