using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class TaxPeriodService : ITaxPeriodService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ITaxReportService _reports;
    private readonly IAdminNotificationService _notifications;

    public TaxPeriodService(IDbContextFactory<AppDbContext> contextFactory, ITaxReportService reports, IAdminNotificationService notifications)
    {
        _contextFactory = contextFactory;
        _reports = reports;
        _notifications = notifications;
    }

    public async Task<PagedResult<TaxPeriod>> GetPagedAsync(int websiteId, byte? status, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.TaxPeriods.AsNoTracking().Where(p => p.WebsiteID == websiteId);
        if (status is byte s) q = q.Where(p => p.Status == s);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(p => p.FromDate).Skip(query.Skip).Take(query.Take).ToListAsync(ct);
        return new PagedResult<TaxPeriod> { Items = items, TotalCount = total };
    }

    public async Task<TaxPeriod?> GetByIdAsync(int taxPeriodId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.TaxPeriods.AsNoTracking()
            .FirstOrDefaultAsync(p => p.TaxPeriodID == taxPeriodId, ct);
    }

    public async Task<(bool Success, string? Error, TaxPeriod? Period)> CreateAsync(
        int websiteId, string periodCode, DateOnly from, DateOnly to, string? note, int? memberId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        if (websiteId <= 0) return (false, "Website is required.", null);
        if (to < from) return (false, "Invalid date range.", null);
        var code = (periodCode ?? "").Trim();
        if (code.Length == 0) code = $"{from:yyyy-MM}";
        if (code.Length > 30) return (false, "Period code is too long (max 30).", null);

        if (await _context.TaxPeriods.AnyAsync(p => p.WebsiteID == websiteId && p.PeriodCode == code, ct))
            return (false, "A tax period with this code already exists.", null);

        var period = new TaxPeriod
        {
            WebsiteID = websiteId,
            PeriodCode = code,
            FromDate = from,
            ToDate = to,
            Status = (byte)TaxPeriodStatus.Open,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            CreatedByMemberID = memberId,
            CreatedAt = DateTime.UtcNow,
        };
        _context.TaxPeriods.Add(period);
        await _context.SaveChangesAsync(ct);
        return (true, null, period);
    }

    public async Task<(bool Success, string? Error)> UpdateStatusAsync(
        int taxPeriodId, TaxPeriodStatus status, int? memberId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var p = await _context.TaxPeriods.FirstOrDefaultAsync(x => x.TaxPeriodID == taxPeriodId, ct);
        if (p is null) return (false, "Period not found.");
        if (p.Status == (byte)TaxPeriodStatus.Filed && status != TaxPeriodStatus.Filed)
            return (false, "Filed periods cannot change status.");
        p.Status = (byte)status;
        if (status is TaxPeriodStatus.Closed or TaxPeriodStatus.Filed)
        {
            p.ClosedAt ??= DateTime.UtcNow;
            p.ClosedByMemberID ??= memberId;
        }
        await _context.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error, TaxPeriod? Period)> GenerateSnapshotAsync(
        int taxPeriodId, int? memberId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var p = await _context.TaxPeriods.FirstOrDefaultAsync(x => x.TaxPeriodID == taxPeriodId, ct);
        if (p is null) return (false, "Period not found.", null);
        if (p.Status == (byte)TaxPeriodStatus.Filed)
            return (false, "Cannot re-snapshot a filed period.", null);

        var report = await _reports.GetVatReportAsync(new VatReportRequest
        {
            WebsiteId = p.WebsiteID,
            From = p.FromDate,
            To = p.ToDate,
        }, ct);

        p.OutputTaxSnapshot = report.OutputSales.Tax;
        p.SettlementTaxSnapshot = report.SettlementTax.Tax;
        p.NetTaxSnapshot = report.OutputSales.Tax + report.SettlementTax.Tax;
        p.OutputOrderCount = report.OutputSales.DocumentCount;
        p.SettlementCount = report.SettlementTax.DocumentCount;
        p.SnapshotAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return (true, null, p);
    }

    public async Task<(bool Success, string? Error)> CloseAsync(int taxPeriodId, int? memberId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var snap = await GenerateSnapshotAsync(taxPeriodId, memberId, ct);
        if (!snap.Success) return (snap.Success, snap.Error);

        var p = await _context.TaxPeriods.FirstAsync(x => x.TaxPeriodID == taxPeriodId, ct);
        p.Status = (byte)TaxPeriodStatus.Closed;
        p.ClosedAt = DateTime.UtcNow;
        p.ClosedByMemberID = memberId;
        await _context.SaveChangesAsync(ct);

        try
        {
            await _notifications.NotifyRoleAsync(
                p.WebsiteID,
                [RoleKeys.TaxView, RoleKeys.TaxEdit, RoleKeys.AccountingView],
                AdminNotificationType.TaxPeriod,
                "Tax period closed",
                $"Period {p.PeriodCode}: net tax snapshot {p.NetTaxSnapshot:0.##} ({p.FromDate:yyyy-MM-dd} → {p.ToDate:yyyy-MM-dd}).",
                $"/sales/tax/periods/{p.TaxPeriodID}",
                p.TaxPeriodID,
                ct);
        }
        catch { /* ignore notify failures */ }

        return (true, null);
    }
}
