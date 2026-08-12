using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class SettlementService : ISettlementService
{
    private readonly AppDbContext _context;
    private readonly IFinancialLedgerService _ledger;

    public SettlementService(AppDbContext context, IFinancialLedgerService ledger)
    {
        _context = context;
        _ledger = ledger;
    }

    public async Task<PagedResult<Settlement>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.Settlements.AsNoTracking()
            .Include(s => s.Vendor)
            .Include(s => s.Supplier)
            .Include(s => s.TargetWebsite)
            .Where(s => s.WebsiteID == websiteId);

        if (query.GetSearch("Status") is string st && byte.TryParse(st, out var status))
            q = q.Where(s => s.Status == status);
        if (query.GetSearch(nameof(Settlement.Note)) is string note)
            q = q.Where(s => s.Note != null && s.Note.Contains(note));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Settlement.CreatedAt), fallbackDescending: true)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Settlement> { Items = items, TotalCount = total };
    }

    public async Task<Settlement?> GetByIdAsync(int settlementId, CancellationToken ct = default) =>
        await _context.Settlements.AsNoTracking()
            .Include(s => s.Vendor)
            .Include(s => s.Supplier)
            .Include(s => s.TargetWebsite)
            .Include(s => s.BankAccount)
            .Include(s => s.SettlementItems)
            .FirstOrDefaultAsync(s => s.SettlementID == settlementId, ct);

    public async Task<bool> ApproveAsync(int settlementId, int? memberId, CancellationToken ct = default)
    {
        var s = await _context.Settlements.FirstOrDefaultAsync(x => x.SettlementID == settlementId, ct);
        if (s is null) return false;
        if (s.Status is not ((byte)SettlementStatus.Open or (byte)SettlementStatus.Draft))
            return false;

        s.Status = (byte)SettlementStatus.Approved;
        s.ApprovedByMemberID = memberId;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> MarkPaidAsync(int settlementId, int? bankAccountId, string? paymentRef, int? memberId, CancellationToken ct = default)
    {
        var s = await _context.Settlements.FirstOrDefaultAsync(x => x.SettlementID == settlementId, ct);
        if (s is null) return false;
        if (s.Status is (byte)SettlementStatus.Paid or (byte)SettlementStatus.Cancelled)
            return false;

        s.Status = (byte)SettlementStatus.Paid;
        s.BankAccountID = bankAccountId ?? s.BankAccountID;
        s.PaymentRefNumber = string.IsNullOrWhiteSpace(paymentRef) ? s.PaymentRefNumber : paymentRef.Trim();
        s.PaidAt = DateTime.UtcNow;
        s.ApprovedByMemberID ??= memberId;
        await _context.SaveChangesAsync(ct);

        try
        {
            await _ledger.PostAsync(new PostFinancialEntryRequest
            {
                WebsiteId = s.WebsiteID,
                TransactionType = Application.Financial.FinancialTransactionTypes.SettlementPaid,
                Flow = Application.Financial.FinancialFlow.Out,
                Amount = s.TotalAmount,
                AmountUsd = s.TotalAmount,
                CurrencyCode = s.CurrencyCode,
                Title = $"Settlement S-{s.SettlementID} paid",
                Description = s.PaymentRefNumber,
                ReportToTax = true,
                VendorVisible = true,
                VendorId = s.VendorID,
                SettlementId = s.SettlementID,
                MemberId = memberId,
            }, ct);
        }
        catch { /* ignore */ }

        return true;
    }

    public async Task<bool> CancelAsync(int settlementId, string? note, int? memberId, CancellationToken ct = default)
    {
        var s = await _context.Settlements.FirstOrDefaultAsync(x => x.SettlementID == settlementId, ct);
        if (s is null) return false;
        if (s.Status == (byte)SettlementStatus.Paid) return false;

        s.Status = (byte)SettlementStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(note))
            s.Note = string.IsNullOrWhiteSpace(s.Note) ? note.Trim() : $"{s.Note} | Cancelled: {note.Trim()}";
        s.ApprovedByMemberID ??= memberId;
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
