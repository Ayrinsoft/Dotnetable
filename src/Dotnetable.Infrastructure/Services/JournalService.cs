using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class JournalService : IJournalService
{
    private readonly AppDbContext _context;

    public JournalService(AppDbContext context) => _context = context;

    public async Task<PagedResult<JournalEntryDto>> GetPagedAsync(
        int websiteId, DateOnly? from, DateOnly? to, bool? posted, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.JournalEntries.AsNoTracking()
            .Include(j => j.JournalEntryLines)
            .Where(j => j.WebsiteID == websiteId);
        if (from is DateOnly f) q = q.Where(j => j.EntryDate >= f);
        if (to is DateOnly t) q = q.Where(j => j.EntryDate <= t);
        if (posted is bool p) q = q.Where(j => j.IsPosted == p);

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(j => j.EntryDate).ThenByDescending(j => j.JournalEntryID)
            .Skip(query.Skip).Take(query.Take).ToListAsync(ct);
        return new PagedResult<JournalEntryDto>
        {
            Items = items.Select(MapDto).ToList(),
            TotalCount = total,
        };
    }

    public async Task<JournalEntryDto?> GetByIdAsync(int journalEntryId, CancellationToken ct = default)
    {
        var j = await _context.JournalEntries.AsNoTracking()
            .Include(x => x.JournalEntryLines).ThenInclude(l => l.ChartOfAccount)
            .FirstOrDefaultAsync(x => x.JournalEntryID == journalEntryId, ct);
        return j is null ? null : MapDto(j);
    }

    public async Task<(bool Success, string? Error, JournalEntry? Entry)> CreateDraftAsync(
        int websiteId, DateOnly entryDate, string? description, string currencyCode,
        bool reportToTax, IReadOnlyList<JournalLineDto> lines, int? memberId,
        string? sourceType = null, string? sourceKey = null, CancellationToken ct = default)
    {
        var (ok, err) = ValidateLines(lines);
        if (!ok) return (false, err, null);

        if (!string.IsNullOrWhiteSpace(sourceKey)
            && await _context.JournalEntries.AnyAsync(j => j.WebsiteID == websiteId && j.SourceKey == sourceKey, ct))
            return (false, "Journal already exists for this source.", null);

        var closed = await _context.FiscalPeriods.AnyAsync(p =>
            p.WebsiteID == websiteId && p.IsClosed && p.PeriodFrom <= entryDate && p.PeriodTo >= entryDate, ct);
        if (closed) return (false, "Fiscal period is closed for this date.", null);

        var periodId = await _context.FiscalPeriods.AsNoTracking()
            .Where(p => p.WebsiteID == websiteId && p.PeriodFrom <= entryDate && p.PeriodTo >= entryDate)
            .Select(p => (int?)p.FiscalPeriodID)
            .FirstOrDefaultAsync(ct);

        var seq = await _context.JournalEntries.CountAsync(j => j.WebsiteID == websiteId, ct) + 1;
        var entry = new JournalEntry
        {
            WebsiteID = websiteId,
            EntryNumber = $"JE-{entryDate:yyyyMM}-{seq:D5}",
            EntryDate = entryDate,
            Description = description,
            SourceType = sourceType,
            SourceKey = sourceKey,
            CurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? "USD" : currencyCode.Trim().ToUpperInvariant(),
            ReportToTax = reportToTax,
            FiscalPeriodID = periodId,
            IsPosted = false,
            CreatedByMemberID = memberId,
            CreatedAt = DateTime.UtcNow,
        };
        foreach (var line in lines.Where(l => l.Debit > 0 || l.Credit > 0))
        {
            entry.JournalEntryLines.Add(new JournalEntryLine
            {
                ChartOfAccountID = line.ChartOfAccountID,
                Debit = line.Debit,
                Credit = line.Credit,
                Description = line.Description,
            });
        }
        _context.JournalEntries.Add(entry);
        await _context.SaveChangesAsync(ct);
        return (true, null, entry);
    }

    public async Task<(bool Success, string? Error)> PostAsync(int journalEntryId, int? memberId, CancellationToken ct = default)
    {
        var entry = await _context.JournalEntries
            .Include(j => j.JournalEntryLines)
            .FirstOrDefaultAsync(j => j.JournalEntryID == journalEntryId, ct);
        if (entry is null) return (false, "Journal not found.");
        if (entry.IsPosted) return (false, "Already posted.");
        if (entry.IsReversed) return (false, "Reversed entry cannot be posted.");

        var debit = entry.JournalEntryLines.Sum(l => l.Debit);
        var credit = entry.JournalEntryLines.Sum(l => l.Credit);
        if (debit <= 0 || debit != credit)
            return (false, "Journal must be balanced (total debit = total credit > 0).");

        var closed = await _context.FiscalPeriods.AnyAsync(p =>
            p.WebsiteID == entry.WebsiteID && p.IsClosed
            && p.PeriodFrom <= entry.EntryDate && p.PeriodTo >= entry.EntryDate, ct);
        if (closed) return (false, "Fiscal period is closed.");

        entry.IsPosted = true;
        entry.PostedAt = DateTime.UtcNow;
        entry.PostedByMemberID = memberId;
        await _context.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error, JournalEntry? Reversal)> ReverseAsync(
        int journalEntryId, int? memberId, string? note, CancellationToken ct = default)
    {
        var original = await _context.JournalEntries
            .Include(j => j.JournalEntryLines)
            .FirstOrDefaultAsync(j => j.JournalEntryID == journalEntryId, ct);
        if (original is null) return (false, "Journal not found.", null);
        if (!original.IsPosted) return (false, "Only posted journals can be reversed.", null);
        if (original.IsReversed) return (false, "Already reversed.", null);

        var lines = original.JournalEntryLines.Select(l => new JournalLineDto
        {
            ChartOfAccountID = l.ChartOfAccountID,
            Debit = l.Credit,
            Credit = l.Debit,
            Description = l.Description,
        }).ToList();

        var (ok, err, rev) = await CreateDraftAsync(
            original.WebsiteID, DateOnly.FromDateTime(DateTime.UtcNow),
            note ?? $"Reversal of {original.EntryNumber}",
            original.CurrencyCode, original.ReportToTax, lines, memberId,
            "Reversal", $"REV:{original.JournalEntryID}", ct);
        if (!ok || rev is null) return (false, err, null);

        rev.ReversesJournalEntryID = original.JournalEntryID;
        original.IsReversed = true;
        await _context.SaveChangesAsync(ct);

        var post = await PostAsync(rev.JournalEntryID, memberId, ct);
        if (!post.Success) return (false, post.Error, rev);
        return (true, null, rev);
    }

    private static (bool Ok, string? Error) ValidateLines(IReadOnlyList<JournalLineDto> lines)
    {
        if (lines.Count == 0) return (false, "At least one line is required.");
        var debit = lines.Sum(l => l.Debit);
        var credit = lines.Sum(l => l.Credit);
        if (debit <= 0 || debit != credit)
            return (false, "Lines must balance (Σ debit = Σ credit > 0).");
        if (lines.Any(l => l.Debit < 0 || l.Credit < 0 || (l.Debit > 0 && l.Credit > 0)))
            return (false, "Each line must have debit or credit (not both, not negative).");
        return (true, null);
    }

    private static JournalEntryDto MapDto(JournalEntry j)
    {
        var lines = j.JournalEntryLines.Select(l => new JournalLineDto
        {
            ChartOfAccountID = l.ChartOfAccountID,
            AccountCode = l.ChartOfAccount?.Code,
            AccountName = l.ChartOfAccount?.Name,
            Debit = l.Debit,
            Credit = l.Credit,
            Description = l.Description,
        }).ToList();
        return new JournalEntryDto
        {
            JournalEntryID = j.JournalEntryID,
            EntryNumber = j.EntryNumber,
            EntryDate = j.EntryDate,
            Description = j.Description,
            SourceType = j.SourceType,
            SourceKey = j.SourceKey,
            CurrencyCode = j.CurrencyCode,
            ReportToTax = j.ReportToTax,
            IsPosted = j.IsPosted,
            IsReversed = j.IsReversed,
            CreatedAt = j.CreatedAt,
            PostedAt = j.PostedAt,
            TotalDebit = lines.Sum(l => l.Debit),
            TotalCredit = lines.Sum(l => l.Credit),
            Lines = lines,
        };
    }
}
