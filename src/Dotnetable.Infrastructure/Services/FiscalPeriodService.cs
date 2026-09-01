using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class FiscalPeriodService : IFiscalPeriodService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IChartOfAccountService _coa;
    private readonly IJournalService _journals;

    public FiscalPeriodService(IDbContextFactory<AppDbContext> contextFactory, IChartOfAccountService coa, IJournalService journals)
    {
        _contextFactory = contextFactory;
        _coa = coa;
        _journals = journals;
    }

    public async Task<IReadOnlyList<FiscalPeriod>> GetAllAsync(int websiteId, bool? closedOnly = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.FiscalPeriods.AsNoTracking().Where(p => p.WebsiteID == websiteId);
        if (closedOnly is bool c) q = q.Where(p => p.IsClosed == c);
        return await q.OrderByDescending(p => p.PeriodFrom).ToListAsync(ct);
    }

    public async Task<FiscalPeriod?> GetCurrentOpenAsync(int websiteId, DateOnly? asOf = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var day = asOf ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return await _context.FiscalPeriods.AsNoTracking()
            .Where(p => p.WebsiteID == websiteId && !p.IsClosed && p.PeriodFrom <= day && p.PeriodTo >= day)
            .OrderByDescending(p => p.PeriodFrom)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<FiscalCalendarSettingsDto> GetCalendarSettingsAsync(int websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var w = await _context.Websites.AsNoTracking().FirstOrDefaultAsync(x => x.WebsiteID == websiteId, ct);
        if (w is null)
            return new FiscalCalendarSettingsDto();
        return new FiscalCalendarSettingsDto
        {
            Cadence = Enum.IsDefined(typeof(FiscalPeriodCadence), w.FiscalPeriodCadence)
                ? (FiscalPeriodCadence)w.FiscalPeriodCadence
                : FiscalPeriodCadence.Monthly,
            FiscalYearStartMonth = w.FiscalYearStartMonth is >= 1 and <= 12 ? w.FiscalYearStartMonth : (byte)1,
            FiscalWeekStartDay = w.FiscalWeekStartDay <= 6 ? w.FiscalWeekStartDay : (byte)1,
            CloseDueDays = w.FiscalCloseDueDays < 0 ? 0 : w.FiscalCloseDueDays,
        };
    }

    public async Task<(bool Success, string? Error)> SaveCalendarSettingsAsync(
        int websiteId, FiscalCalendarSettingsDto settings, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var w = await _context.Websites.FirstOrDefaultAsync(x => x.WebsiteID == websiteId, ct);
        if (w is null) return (false, "Website not found.");
        if (settings.FiscalYearStartMonth is < 1 or > 12)
            return (false, "Fiscal year start month must be 1–12.");
        if (settings.FiscalWeekStartDay > 6)
            return (false, "Week start day must be 0 (Sunday) through 6 (Saturday).");
        if (settings.CloseDueDays is < 0 or > 90)
            return (false, "Close due days must be between 0 and 90.");

        w.FiscalPeriodCadence = (byte)settings.Cadence;
        w.FiscalYearStartMonth = settings.FiscalYearStartMonth;
        w.FiscalWeekStartDay = settings.FiscalWeekStartDay;
        w.FiscalCloseDueDays = settings.CloseDueDays;
        await _context.SaveChangesAsync(ct);

        // Refresh due dates on open periods.
        var open = await _context.FiscalPeriods.Where(p => p.WebsiteID == websiteId && !p.IsClosed).ToListAsync(ct);
        foreach (var p in open)
            p.CloseDueDate = p.PeriodTo.AddDays(settings.CloseDueDays);
        await _context.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error, int Created)> GenerateForYearAsync(
        int websiteId, int year, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        if (websiteId <= 0 || year is < 2000 or > 2100)
            return (false, "Invalid website or year.", 0);

        var settings = await GetCalendarSettingsAsync(websiteId, ct);
        var ranges = BuildRanges(year, settings);
        var existing = await _context.FiscalPeriods.AsNoTracking()
            .Where(p => p.WebsiteID == websiteId && p.PeriodFrom.Year == year)
            .Select(p => new { p.PeriodFrom, p.PeriodTo })
            .ToListAsync(ct);

        var created = 0;
        foreach (var (from, to, name) in ranges)
        {
            if (existing.Any(e => e.PeriodFrom == from && e.PeriodTo == to))
                continue;
            // Overlap check against any existing period
            var overlap = await _context.FiscalPeriods.AnyAsync(p =>
                p.WebsiteID == websiteId
                && p.PeriodFrom <= to && p.PeriodTo >= from, ct);
            if (overlap) continue;

            _context.FiscalPeriods.Add(new FiscalPeriod
            {
                WebsiteID = websiteId,
                Name = name,
                PeriodFrom = from,
                PeriodTo = to,
                CloseDueDate = to.AddDays(settings.CloseDueDays),
                IsClosed = false,
                CreatedAt = DateTime.UtcNow,
            });
            created++;
        }
        if (created > 0)
            await _context.SaveChangesAsync(ct);
        return (true, null, created);
    }

    public async Task<FiscalPeriod> EnsureYearAsync(int websiteId, int year, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        await GenerateForYearAsync(websiteId, year, ct);
        var first = await _context.FiscalPeriods
            .FirstOrDefaultAsync(p => p.WebsiteID == websiteId && p.PeriodFrom.Year == year, ct);
        if (first is not null) return first;

        // Fallback single year period
        var p = new FiscalPeriod
        {
            WebsiteID = websiteId,
            Name = year.ToString(),
            PeriodFrom = new DateOnly(year, 1, 1),
            PeriodTo = new DateOnly(year, 12, 31),
            CloseDueDate = new DateOnly(year, 12, 31).AddDays(5),
            IsClosed = false,
            CreatedAt = DateTime.UtcNow,
        };
        _context.FiscalPeriods.Add(p);
        await _context.SaveChangesAsync(ct);
        return p;
    }

    public async Task<(bool Success, string? Error)> CloseAsync(int fiscalPeriodId, int? memberId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var p = await _context.FiscalPeriods.FirstOrDefaultAsync(x => x.FiscalPeriodID == fiscalPeriodId, ct);
        if (p is null) return (false, "Period not found.");
        if (p.IsClosed) return (true, null);

        // Only close the earliest open period that has no earlier open periods (sequential close).
        var earlierOpen = await _context.FiscalPeriods.AnyAsync(x =>
            x.WebsiteID == p.WebsiteID && !x.IsClosed && x.PeriodTo < p.PeriodFrom, ct);
        if (earlierOpen)
            return (false, "Close earlier open periods first.");

        var openDrafts = await _context.JournalEntries.AnyAsync(j =>
            j.WebsiteID == p.WebsiteID && !j.IsPosted
            && j.EntryDate >= p.PeriodFrom && j.EntryDate <= p.PeriodTo, ct);
        if (openDrafts)
            return (false, "Close or post all draft journals in the period first.");

        await _coa.EnsureSeededAsync(p.WebsiteID, ct);
        await EnsureRetainedEarningsAccountAsync(_context, p.WebsiteID, ct);

        // Account balances from all posted journals up to PeriodTo (includes prior opening + activity).
        var balances = await GetAccountBalancesThroughAsync(p.WebsiteID, p.PeriodTo, ct);
        var currency = await _context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == p.WebsiteID)
            .Select(w => w.DefaultCurrencyCode)
            .FirstOrDefaultAsync(ct) ?? "USD";

        // Net P&L for THIS period only (income − expense activity in range, excluding opening balance journals).
        var periodPl = await GetPeriodProfitAndLossAsync(_context, p.WebsiteID, p.PeriodFrom, p.PeriodTo, ct);

        int? closingJeId = null;
        // Closing entry on last day: zero P&L into retained earnings (for this period's net).
        if (periodPl != 0)
        {
            var reId = await GetAccountIdByCodeAsync(_context, p.WebsiteID, "3200", ct)
                       ?? await GetAccountIdByCodeAsync(_context, p.WebsiteID, "3100", ct);
            if (reId is null)
                return (false, "Equity account (3200/3100) missing.");

            var plLines = await BuildPlCloseLinesAsync(_context, p.WebsiteID, p.PeriodFrom, p.PeriodTo, reId.Value, periodPl, ct);
            if (plLines.Count >= 2)
            {
                var (okC, errC, closing) = await _journals.CreateDraftAsync(
                    p.WebsiteID, p.PeriodTo,
                    $"Period close P&L → RE ({p.Name})",
                    currency, false, plLines, memberId,
                    "PeriodClose", $"PCLOSE:{p.FiscalPeriodID}", ct);
                if (!okC || closing is null) return (false, errC ?? "Could not create closing journal.");
                var (okP, errP) = await _journals.PostAsync(closing.JournalEntryID, memberId, ct);
                if (!okP) return (false, errP ?? "Could not post closing journal.");
                closingJeId = closing.JournalEntryID;
            }
        }

        // Refresh balances after closing entry
        balances = await GetAccountBalancesThroughAsync(p.WebsiteID, p.PeriodTo, ct);

        // Ensure / create next period
        var nextFrom = p.PeriodTo.AddDays(1);
        var settings = await GetCalendarSettingsAsync(p.WebsiteID, ct);
        var next = await _context.FiscalPeriods
            .FirstOrDefaultAsync(x => x.WebsiteID == p.WebsiteID && x.PeriodFrom == nextFrom, ct);
        if (next is null)
        {
            var nextTo = ComputeNextPeriodEnd(nextFrom, settings);
            next = new FiscalPeriod
            {
                WebsiteID = p.WebsiteID,
                Name = NameForRange(nextFrom, nextTo, settings.Cadence),
                PeriodFrom = nextFrom,
                PeriodTo = nextTo,
                CloseDueDate = nextTo.AddDays(settings.CloseDueDays),
                IsClosed = false,
                CreatedAt = DateTime.UtcNow,
            };
            _context.FiscalPeriods.Add(next);
            await _context.SaveChangesAsync(ct);
        }

        // Opening balance journal on first day of next period (balance sheet only).
        var obLines = BuildOpeningBalanceLines(balances);
        if (obLines.Count >= 2)
        {
            var (okO, errO, opening) = await _journals.CreateDraftAsync(
                p.WebsiteID, next.PeriodFrom,
                $"Opening balances from {p.Name}",
                currency, false, obLines, memberId,
                "OpeningBalance", $"OB:{next.FiscalPeriodID}:from:{p.FiscalPeriodID}", ct);
            if (!okO || opening is null) return (false, errO ?? "Could not create opening balance journal.");
            var (okPo, errPo) = await _journals.PostAsync(opening.JournalEntryID, memberId, ct);
            if (!okPo) return (false, errPo ?? "Could not post opening balance journal.");
            next.OpeningJournalEntryID = opening.JournalEntryID;
        }

        p.IsClosed = true;
        p.ClosedAt = DateTime.UtcNow;
        p.ClosedByMemberID = memberId;
        p.ClosingJournalEntryID = closingJeId;
        await _context.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ReopenAsync(int fiscalPeriodId, int? memberId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var p = await _context.FiscalPeriods.FirstOrDefaultAsync(x => x.FiscalPeriodID == fiscalPeriodId, ct);
        if (p is null) return (false, "Period not found.");
        if (!p.IsClosed) return (true, null);

        // Only reopen the latest closed period (no later closed periods).
        var laterClosed = await _context.FiscalPeriods.AnyAsync(x =>
            x.WebsiteID == p.WebsiteID && x.IsClosed && x.PeriodFrom > p.PeriodFrom, ct);
        if (laterClosed)
            return (false, "Only the most recently closed period can be reopened.");

        // Block if next period has non-opening journals
        var next = await _context.FiscalPeriods
            .FirstOrDefaultAsync(x => x.WebsiteID == p.WebsiteID && x.PeriodFrom == p.PeriodTo.AddDays(1), ct);
        if (next is not null)
        {
            var activity = await _context.JournalEntries.AnyAsync(j =>
                j.WebsiteID == p.WebsiteID
                && j.EntryDate >= next.PeriodFrom && j.EntryDate <= next.PeriodTo
                && j.IsPosted
                && j.SourceType != "OpeningBalance"
                && j.SourceType != "PeriodClose", ct);
            if (activity)
                return (false, "Next period already has activity; cannot reopen.");

            // Soft-remove opening journal link (leave journal for audit; mark reversed if needed)
            next.OpeningJournalEntryID = null;
        }

        p.IsClosed = false;
        p.ClosedAt = null;
        p.ClosedByMemberID = null;
        p.ClosingJournalEntryID = null;
        await _context.SaveChangesAsync(ct);
        return (true, null);
    }

    private async Task EnsureRetainedEarningsAccountAsync(AppDbContext _context, int websiteId, CancellationToken ct)
    {
        if (await _context.ChartOfAccounts.AnyAsync(a => a.WebsiteID == websiteId && a.Code == "3200", ct))
            return;
        _context.ChartOfAccounts.Add(new ChartOfAccount
        {
            WebsiteID = websiteId,
            Code = "3200",
            Name = "Retained earnings",
            AccountType = (byte)GlAccountType.Equity,
            IsActive = true,
            IsSystem = true,
            SortOrder = 95,
        });
        await _context.SaveChangesAsync(ct);
    }

    private async Task<int?> GetAccountIdByCodeAsync(AppDbContext _context, int websiteId, string code, CancellationToken ct)
    {
        return await _context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.WebsiteID == websiteId && a.Code == code)
            .Select(a => (int?)a.ChartOfAccountID)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<Dictionary<int, (byte Type, decimal Balance)>> GetAccountBalancesThroughAsync(
        int websiteId, DateOnly through, CancellationToken ct)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var accounts = await _context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.WebsiteID == websiteId && a.IsActive)
            .Select(a => new { a.ChartOfAccountID, a.AccountType })
            .ToListAsync(ct);

        var sums = await _context.JournalEntryLines.AsNoTracking()
            .Where(l => l.JournalEntry.WebsiteID == websiteId
                        && l.JournalEntry.IsPosted
                        && !l.JournalEntry.IsReversed
                        && l.JournalEntry.EntryDate <= through)
            .GroupBy(l => l.ChartOfAccountID)
            .Select(g => new { Id = g.Key, Debit = g.Sum(x => x.Debit), Credit = g.Sum(x => x.Credit) })
            .ToListAsync(ct);
        var map = sums.ToDictionary(x => x.Id);

        var result = new Dictionary<int, (byte, decimal)>();
        foreach (var a in accounts)
        {
            map.TryGetValue(a.ChartOfAccountID, out var s);
            var debit = s?.Debit ?? 0;
            var credit = s?.Credit ?? 0;
            var bal = a.AccountType is (byte)GlAccountType.Asset or (byte)GlAccountType.Expense
                ? debit - credit
                : credit - debit;
            if (bal != 0)
                result[a.ChartOfAccountID] = (a.AccountType, bal);
        }
        return result;
    }

    private async Task<decimal> GetPeriodProfitAndLossAsync(AppDbContext _context, int websiteId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        // Net income for period activity excluding opening-balance journals (those are BS only).
        var lines = await _context.JournalEntryLines.AsNoTracking()
            .Where(l => l.JournalEntry.WebsiteID == websiteId
                        && l.JournalEntry.IsPosted
                        && !l.JournalEntry.IsReversed
                        && l.JournalEntry.EntryDate >= from
                        && l.JournalEntry.EntryDate <= to
                        && l.JournalEntry.SourceType != "OpeningBalance")
            .Select(l => new { l.ChartOfAccount.AccountType, l.Debit, l.Credit })
            .ToListAsync(ct);

        decimal income = 0, expense = 0;
        foreach (var l in lines)
        {
            if (l.AccountType == (byte)GlAccountType.Income)
                income += l.Credit - l.Debit;
            else if (l.AccountType == (byte)GlAccountType.Expense)
                expense += l.Debit - l.Credit;
        }
        return income - expense;
    }

    private async Task<List<JournalLineDto>> BuildPlCloseLinesAsync(AppDbContext _context, 
        int websiteId, DateOnly from, DateOnly to, int retainedEarningsId, decimal netProfit, CancellationToken ct)
    {
        // Zero out each income/expense account for the period into RE.
        var sums = await _context.JournalEntryLines.AsNoTracking()
            .Where(l => l.JournalEntry.WebsiteID == websiteId
                        && l.JournalEntry.IsPosted
                        && !l.JournalEntry.IsReversed
                        && l.JournalEntry.EntryDate >= from
                        && l.JournalEntry.EntryDate <= to
                        && l.JournalEntry.SourceType != "OpeningBalance"
                        && (l.ChartOfAccount.AccountType == (byte)GlAccountType.Income
                            || l.ChartOfAccount.AccountType == (byte)GlAccountType.Expense))
            .GroupBy(l => new { l.ChartOfAccountID, l.ChartOfAccount.AccountType })
            .Select(g => new
            {
                g.Key.ChartOfAccountID,
                g.Key.AccountType,
                Debit = g.Sum(x => x.Debit),
                Credit = g.Sum(x => x.Credit),
            })
            .ToListAsync(ct);

        var lines = new List<JournalLineDto>();
        foreach (var s in sums)
        {
            if (s.AccountType == (byte)GlAccountType.Income)
            {
                var bal = s.Credit - s.Debit; // credit-normal
                if (bal == 0) continue;
                // Debit income to zero, credit RE
                lines.Add(new JournalLineDto { ChartOfAccountID = s.ChartOfAccountID, Debit = bal > 0 ? bal : 0, Credit = bal < 0 ? -bal : 0 });
            }
            else
            {
                var bal = s.Debit - s.Credit; // debit-normal expense
                if (bal == 0) continue;
                // Credit expense to zero, debit RE
                lines.Add(new JournalLineDto { ChartOfAccountID = s.ChartOfAccountID, Debit = bal < 0 ? -bal : 0, Credit = bal > 0 ? bal : 0 });
            }
        }

        // Balance to RE
        var d = lines.Sum(l => l.Debit);
        var c = lines.Sum(l => l.Credit);
        if (d != c)
        {
            if (d > c)
                lines.Add(new JournalLineDto { ChartOfAccountID = retainedEarningsId, Debit = 0, Credit = d - c, Description = "Net income to RE" });
            else
                lines.Add(new JournalLineDto { ChartOfAccountID = retainedEarningsId, Debit = c - d, Credit = 0, Description = "Net loss to RE" });
        }
        return lines.Where(l => l.Debit > 0 || l.Credit > 0).ToList();
    }

    private static List<JournalLineDto> BuildOpeningBalanceLines(Dictionary<int, (byte Type, decimal Balance)> balances)
    {
        var lines = new List<JournalLineDto>();
        foreach (var (accountId, (type, bal)) in balances)
        {
            // Only balance-sheet accounts carry forward.
            if (type is (byte)GlAccountType.Income or (byte)GlAccountType.Expense)
                continue;
            if (bal == 0) continue;

            if (type == (byte)GlAccountType.Asset)
            {
                // Positive asset = debit
                lines.Add(new JournalLineDto
                {
                    ChartOfAccountID = accountId,
                    Debit = bal > 0 ? bal : 0,
                    Credit = bal < 0 ? -bal : 0,
                    Description = "Opening balance",
                });
            }
            else // Liability or Equity — credit normal
            {
                lines.Add(new JournalLineDto
                {
                    ChartOfAccountID = accountId,
                    Debit = bal < 0 ? -bal : 0,
                    Credit = bal > 0 ? bal : 0,
                    Description = "Opening balance",
                });
            }
        }

        // Force balance (rounding / incomplete charts)
        var d = lines.Sum(l => l.Debit);
        var c = lines.Sum(l => l.Credit);
        if (d != c && lines.Count > 0)
        {
            // Put residual on first equity-like line or first credit line
            var residual = d - c;
            if (residual > 0)
                lines.Add(new JournalLineDto { ChartOfAccountID = lines[0].ChartOfAccountID, Debit = 0, Credit = residual, Description = "Opening balance plug" });
            else
                lines.Add(new JournalLineDto { ChartOfAccountID = lines[0].ChartOfAccountID, Debit = -residual, Credit = 0, Description = "Opening balance plug" });
        }
        return lines.Where(l => l.Debit > 0 || l.Credit > 0).ToList();
    }

    private static List<(DateOnly From, DateOnly To, string Name)> BuildRanges(int year, FiscalCalendarSettingsDto s)
    {
        var list = new List<(DateOnly, DateOnly, string)>();
        switch (s.Cadence)
        {
            case FiscalPeriodCadence.Daily:
                var d = new DateOnly(year, 1, 1);
                var end = new DateOnly(year, 12, 31);
                while (d <= end)
                {
                    list.Add((d, d, d.ToString("yyyy-MM-dd")));
                    d = d.AddDays(1);
                }
                break;
            case FiscalPeriodCadence.Weekly:
                var start = new DateOnly(year, 1, 1);
                // Align to week start
                while ((int)start.DayOfWeek != s.FiscalWeekStartDay && start.Year == year)
                    start = start.AddDays(-1);
                if (start.Year < year) start = start.AddDays(7);
                var cursor = start;
                while (cursor.Year == year || (cursor.Year < year + 1 && cursor.AddDays(6).Year == year))
                {
                    var to = cursor.AddDays(6);
                    if (to.Year > year) to = new DateOnly(year, 12, 31);
                    if (cursor.Year == year || to.Year == year)
                        list.Add((cursor.Year < year ? new DateOnly(year, 1, 1) : cursor, to, $"W{cursor:yyyy-MM-dd}"));
                    cursor = cursor.AddDays(7);
                    if (cursor > new DateOnly(year, 12, 31)) break;
                }
                break;
            case FiscalPeriodCadence.Yearly:
                var yStart = new DateOnly(year, s.FiscalYearStartMonth, 1);
                var yEnd = yStart.AddYears(1).AddDays(-1);
                list.Add((yStart, yEnd, $"FY {year}"));
                break;
            default: // Monthly
                for (var m = 1; m <= 12; m++)
                {
                    var from = new DateOnly(year, m, 1);
                    var to = from.AddMonths(1).AddDays(-1);
                    list.Add((from, to, from.ToString("yyyy-MM")));
                }
                break;
        }
        return list;
    }

    private static DateOnly ComputeNextPeriodEnd(DateOnly from, FiscalCalendarSettingsDto s) => s.Cadence switch
    {
        FiscalPeriodCadence.Daily => from,
        FiscalPeriodCadence.Weekly => from.AddDays(6),
        FiscalPeriodCadence.Yearly => from.AddYears(1).AddDays(-1),
        _ => from.AddMonths(1).AddDays(-1),
    };

    private static string NameForRange(DateOnly from, DateOnly to, FiscalPeriodCadence cadence) => cadence switch
    {
        FiscalPeriodCadence.Daily => from.ToString("yyyy-MM-dd"),
        FiscalPeriodCadence.Weekly => $"W{from:yyyy-MM-dd}",
        FiscalPeriodCadence.Yearly => $"FY {from.Year}",
        _ => from.ToString("yyyy-MM"),
    };
}
