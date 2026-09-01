using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ClientWalletWithdrawalService : IClientWalletWithdrawalService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IClientWalletService _wallets;
    private readonly IAdminNotificationService _notifications;
    private readonly ICurrencyConversionService _currency;

    public ClientWalletWithdrawalService(
        IDbContextFactory<AppDbContext> contextFactory, IClientWalletService wallets, IAdminNotificationService notifications,
        ICurrencyConversionService currency)
    {
        _contextFactory = contextFactory;
        _wallets = wallets;
        _notifications = notifications;
        _currency = currency;
    }

    public async Task<ClientWalletWithdrawal> RequestAsync(
        int websiteId, int clientId, int clientBankAccountId, decimal amount,
        string? currencyCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        if (amount <= 0)
            throw new InvalidOperationException("Withdrawal amount must be greater than zero.");

        var bankAccount = await _context.ClientBankAccounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.ClientBankAccountID == clientBankAccountId && a.WebsiteClientID == clientId && a.IsActive, ct);
        if (bankAccount is null)
            throw new InvalidOperationException("Bank account not found.");

        var holdTransaction = await _wallets.ApplyAsync(
            websiteId, clientId,
            (byte)ClientWalletTransactionType.WithdrawalHold,
            -amount,
            (byte)ClientWalletSourceType.ClientWalletWithdrawal,
            null,
            "Withdrawal hold",
            memberId: null,
            currencyCode,
            ct);

        var wallet = await _context.ClientWallets.AsNoTracking()
            .FirstAsync(w => w.ClientWalletID == holdTransaction.ClientWalletID, ct);

        decimal amountUsdDual = 0;
        if (await _currency.GetStorePricesInUsdAsync(websiteId, ct))
        {
            try { amountUsdDual = await _currency.ToUsdAsync(websiteId, amount, wallet.CurrencyCode, ct); }
            catch (InvalidOperationException) { amountUsdDual = 0; }
        }

        var withdrawal = new ClientWalletWithdrawal
        {
            WebsiteID = websiteId,
            WebsiteClientID = clientId,
            ClientWalletID = holdTransaction.ClientWalletID,
            ClientBankAccountID = clientBankAccountId,
            CurrencyCode = wallet.CurrencyCode,
            Amount = amount,
            AmountUsd = amountUsdDual,
            Status = (byte)ClientWalletWithdrawalStatus.Pending,
            RequestedAt = DateTime.UtcNow,
        };
        _context.ClientWalletWithdrawals.Add(withdrawal);
        await _context.SaveChangesAsync(ct);

        holdTransaction.SourceId = withdrawal.ClientWalletWithdrawalID;
        await _context.SaveChangesAsync(ct);

        await _notifications.NotifySiteAdminsAsync(
            websiteId,
            AdminNotificationType.WithdrawalRequested,
            "Withdrawal requested",
            $"Customer #{clientId} requested a withdrawal of {amount:0.##} {wallet.CurrencyCode}.",
            "/wallets/withdrawals",
            withdrawal.ClientWalletWithdrawalID,
            ct);

        return withdrawal;
    }

    public async Task<PagedResult<ClientWalletWithdrawal>> GetPagedAsync(int? websiteId, byte? status, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.ClientWalletWithdrawals.AsNoTracking()
            .Include(w => w.WebsiteClient)
            .Include(w => w.ClientBankAccount)
            .Include(w => w.CreatedByMember)
            .Include(w => w.ReviewedByMember)
            .AsQueryable();

        if (websiteId is int wid)
            q = q.Where(w => w.WebsiteID == wid);
        if (status is byte s)
            q = q.Where(w => w.Status == s);
        else if (query.GetSearch(nameof(ClientWalletWithdrawal.Status)) is string statusText && byte.TryParse(statusText, out var statusByte))
            q = q.Where(w => w.Status == statusByte);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(ClientWalletWithdrawal.RequestedAt), fallbackDescending: true)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<ClientWalletWithdrawal> { Items = items, TotalCount = total };
    }

    public async Task<IReadOnlyDictionary<byte, int>> GetStatusCountsAsync(int? websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.ClientWalletWithdrawals.AsNoTracking().AsQueryable();
        if (websiteId is int wid)
            q = q.Where(w => w.WebsiteID == wid);

        var rows = await q
            .GroupBy(w => w.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.Status, r => r.Count);
    }

    public async Task<PagedResult<ClientWalletWithdrawal>> GetByClientIdAsync(int clientId, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

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
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

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
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var withdrawal = await _context.ClientWalletWithdrawals
            .FirstOrDefaultAsync(w => w.ClientWalletWithdrawalID == withdrawalId, ct);
        if (withdrawal is null || withdrawal.Status != (byte)ClientWalletWithdrawalStatus.Pending)
            return false;

        // Reverse the held funds into the same currency wallet.
        var reverseAmount = withdrawal.Amount != 0 || withdrawal.AmountUsd == 0
            ? withdrawal.Amount
            : withdrawal.AmountUsd;
        await _wallets.ApplyAsync(
            withdrawal.WebsiteID, withdrawal.WebsiteClientID,
            (byte)ClientWalletTransactionType.WithdrawalReversed,
            reverseAmount,
            (byte)ClientWalletSourceType.ClientWalletWithdrawal,
            withdrawal.ClientWalletWithdrawalID,
            "Withdrawal rejected",
            memberId,
            withdrawal.CurrencyCode,
            ct);

        withdrawal.Status = (byte)ClientWalletWithdrawalStatus.Rejected;
        withdrawal.ReviewedByMemberID = memberId;
        withdrawal.ReviewedAt = DateTime.UtcNow;
        withdrawal.RejectReason = reason;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<(bool Success, string? Error, ClientWalletWithdrawal? Withdrawal)> RecordAdminPayoutAsync(
        int websiteId, int clientId, int clientBankAccountId, decimal amount,
        string? currencyCode, string? note, string? paymentRef, int memberId,
        DateTime? paidAtUtc = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        if (amount <= 0)
            return (false, "Withdrawal amount must be greater than zero.", null);
        if (string.IsNullOrWhiteSpace(note))
            return (false, "Payment description is required.", null);

        var clientOk = await _context.WebsiteClients.AsNoTracking()
            .AnyAsync(c => c.WebsiteClientID == clientId && c.WebsiteID == websiteId, ct);
        if (!clientOk)
            return (false, "Customer not found.", null);

        var bankAccount = await _context.ClientBankAccounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.ClientBankAccountID == clientBankAccountId
                                      && a.WebsiteClientID == clientId
                                      && a.IsActive, ct);
        if (bankAccount is null)
            return (false, "Customer bank account not found.", null);

        var paidAt = NormalizeUtc(paidAtUtc) ?? DateTime.UtcNow;
        var desc = note.Trim();
        if (desc.Length > 500) desc = desc[..500];

        ClientWalletTransaction holdTransaction;
        try
        {
            holdTransaction = await _wallets.ApplyAsync(
                websiteId, clientId,
                (byte)ClientWalletTransactionType.WithdrawalHold,
                -amount,
                (byte)ClientWalletSourceType.ClientWalletWithdrawal,
                null,
                desc,
                memberId,
                currencyCode,
                ct);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message, null);
        }

        var wallet = await _context.ClientWallets.AsNoTracking()
            .FirstAsync(w => w.ClientWalletID == holdTransaction.ClientWalletID, ct);

        decimal amountUsdDual = 0;
        if (await _currency.GetStorePricesInUsdAsync(websiteId, ct))
        {
            try { amountUsdDual = await _currency.ToUsdAsync(websiteId, amount, wallet.CurrencyCode, ct); }
            catch (InvalidOperationException) { amountUsdDual = 0; }
        }

        var withdrawal = new ClientWalletWithdrawal
        {
            WebsiteID = websiteId,
            WebsiteClientID = clientId,
            ClientWalletID = holdTransaction.ClientWalletID,
            ClientBankAccountID = clientBankAccountId,
            CurrencyCode = wallet.CurrencyCode,
            Amount = amount,
            AmountUsd = amountUsdDual,
            Status = (byte)ClientWalletWithdrawalStatus.Paid,
            Note = desc,
            PaymentRefNumber = string.IsNullOrWhiteSpace(paymentRef) ? null : paymentRef.Trim(),
            CreatedByMemberID = memberId,
            ReviewedByMemberID = memberId,
            ReviewedAt = paidAt,
            PaidAt = paidAt,
            RequestedAt = paidAt,
        };
        _context.ClientWalletWithdrawals.Add(withdrawal);
        await _context.SaveChangesAsync(ct);

        holdTransaction.SourceId = withdrawal.ClientWalletWithdrawalID;
        await _context.SaveChangesAsync(ct);

        return (true, null, withdrawal);
    }

    private static DateTime? NormalizeUtc(DateTime? value)
    {
        if (value is null) return null;
        return value.Value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
            : value.Value.ToUniversalTime();
    }
}
