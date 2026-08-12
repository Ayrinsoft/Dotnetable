using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class FiscalPeriodService : IFiscalPeriodService
{
    private readonly AppDbContext _context;
    public FiscalPeriodService(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<FiscalPeriod>> GetAllAsync(int websiteId, CancellationToken ct = default) =>
        await _context.FiscalPeriods.AsNoTracking()
            .Where(p => p.WebsiteID == websiteId)
            .OrderByDescending(p => p.PeriodFrom)
            .ToListAsync(ct);

    public async Task<FiscalPeriod> EnsureYearAsync(int websiteId, int year, CancellationToken ct = default)
    {
        var existing = await _context.FiscalPeriods
            .FirstOrDefaultAsync(p => p.WebsiteID == websiteId && p.PeriodFrom.Year == year && p.PeriodFrom.Month == 1, ct);
        if (existing is not null) return existing;
        var p = new FiscalPeriod
        {
            WebsiteID = websiteId,
            Name = year.ToString(),
            PeriodFrom = new DateOnly(year, 1, 1),
            PeriodTo = new DateOnly(year, 12, 31),
            IsClosed = false,
            CreatedAt = DateTime.UtcNow,
        };
        _context.FiscalPeriods.Add(p);
        await _context.SaveChangesAsync(ct);
        return p;
    }

    public async Task<(bool Success, string? Error)> CloseAsync(int fiscalPeriodId, int? memberId, CancellationToken ct = default)
    {
        var p = await _context.FiscalPeriods.FirstOrDefaultAsync(x => x.FiscalPeriodID == fiscalPeriodId, ct);
        if (p is null) return (false, "Period not found.");
        if (p.IsClosed) return (true, null);
        var openDrafts = await _context.JournalEntries.AnyAsync(j =>
            j.WebsiteID == p.WebsiteID && !j.IsPosted && j.EntryDate >= p.PeriodFrom && j.EntryDate <= p.PeriodTo, ct);
        if (openDrafts) return (false, "Close all draft journals in the period first.");
        p.IsClosed = true;
        p.ClosedAt = DateTime.UtcNow;
        p.ClosedByMemberID = memberId;
        await _context.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ReopenAsync(int fiscalPeriodId, int? memberId, CancellationToken ct = default)
    {
        var p = await _context.FiscalPeriods.FirstOrDefaultAsync(x => x.FiscalPeriodID == fiscalPeriodId, ct);
        if (p is null) return (false, "Period not found.");
        p.IsClosed = false;
        p.ClosedAt = null;
        p.ClosedByMemberID = null;
        await _context.SaveChangesAsync(ct);
        return (true, null);
    }
}
