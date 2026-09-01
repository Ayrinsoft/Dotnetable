using Dotnetable.Application.DTOs;
using Dotnetable.Application.Financial;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class AccountingReportService : IAccountingReportService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IChartOfAccountService _coa;

    public AccountingReportService(IDbContextFactory<AppDbContext> contextFactory, IChartOfAccountService coa)
    {
        _contextFactory = contextFactory;
        _coa = coa;
    }

    public async Task<IReadOnlyList<TrialBalanceRowDto>> GetTrialBalanceAsync(
        int websiteId, DateOnly from, DateOnly to, bool taxOnly = false, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        await _coa.EnsureSeededAsync(websiteId, ct);
        var accounts = await _context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.WebsiteID == websiteId && a.IsActive)
            .OrderBy(a => a.Code)
            .ToListAsync(ct);

        var q = _context.JournalEntryLines.AsNoTracking()
            .Where(l => l.JournalEntry.WebsiteID == websiteId
                        && l.JournalEntry.IsPosted
                        && !l.JournalEntry.IsReversed
                        && l.JournalEntry.EntryDate >= from
                        && l.JournalEntry.EntryDate <= to);
        if (taxOnly)
            q = q.Where(l => l.JournalEntry.ReportToTax);

        var sums = await q
            .GroupBy(l => l.ChartOfAccountID)
            .Select(g => new { AccountId = g.Key, Debit = g.Sum(x => x.Debit), Credit = g.Sum(x => x.Credit) })
            .ToListAsync(ct);
        var map = sums.ToDictionary(x => x.AccountId);

        return accounts.Select(a =>
        {
            map.TryGetValue(a.ChartOfAccountID, out var s);
            var debit = s?.Debit ?? 0;
            var credit = s?.Credit ?? 0;
            var balance = a.AccountType is (byte)GlAccountType.Asset or (byte)GlAccountType.Expense
                ? debit - credit
                : credit - debit;
            return new TrialBalanceRowDto
            {
                ChartOfAccountID = a.ChartOfAccountID,
                Code = a.Code,
                Name = a.Name,
                AccountType = a.AccountType,
                Debit = debit,
                Credit = credit,
                Balance = balance,
            };
        }).Where(r => r.Debit != 0 || r.Credit != 0 || r.Balance != 0).ToList();
    }

    public async Task<ProfitAndLossDto> GetProfitAndLossAsync(
        int websiteId, DateOnly from, DateOnly to, bool taxOnly = false, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var tb = await GetTrialBalanceAsync(websiteId, from, to, taxOnly, ct);
        var income = tb.Where(r => r.AccountType == (byte)GlAccountType.Income).ToList();
        var expenses = tb.Where(r => r.AccountType == (byte)GlAccountType.Expense).ToList();
        var totalIncome = income.Sum(r => r.Balance);
        var totalExpenses = expenses.Sum(r => r.Balance);

        var currency = await _context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId)
            .Select(w => w.DefaultCurrencyCode)
            .FirstOrDefaultAsync(ct) ?? "USD";

        return new ProfitAndLossDto
        {
            From = from,
            To = to,
            CurrencyCode = currency,
            Income = income,
            Expenses = expenses,
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetProfit = totalIncome - totalExpenses,
        };
    }

    public async Task<TaxReconciliationDto> GetTaxReconciliationAsync(
        int websiteId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var fromDt = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toDt = to.ToDateTime(new TimeOnly(23, 59, 59), DateTimeKind.Utc);

        var orderTax = await _context.Orders.AsNoTracking()
            .Where(o => o.WebsiteID == websiteId && o.ReportToTax
                        && o.Status != 6 // Cancelled
                        && o.CreatedAt >= fromDt && o.CreatedAt <= toDt)
            .SumAsync(o => (decimal?)o.TaxTotal, ct) ?? 0;

        var settlementTax = await _context.Settlements.AsNoTracking()
            .Where(s => s.WebsiteID == websiteId && s.Status != 4
                        && s.CreatedAt >= fromDt && s.CreatedAt <= toDt)
            .SumAsync(s => (decimal?)s.TaxAmount, ct) ?? 0;

        var ledgerTax = await _context.FinancialLedgerEntries.AsNoTracking()
            .Where(e => e.WebsiteID == websiteId && e.IsCurrent && e.ReportToTax
                        && e.TransactionType == FinancialTransactionTypes.OrderTax
                        && e.OccurredDate >= from && e.OccurredDate <= to)
            .SumAsync(e => (decimal?)e.Amount, ct) ?? 0;

        await _coa.EnsureSeededAsync(websiteId, ct);
        var taxAccId = await _context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.WebsiteID == websiteId && a.Code == "2200")
            .Select(a => (int?)a.ChartOfAccountID)
            .FirstOrDefaultAsync(ct);

        decimal glTax = 0;
        if (taxAccId is int aid)
        {
            glTax = await _context.JournalEntryLines.AsNoTracking()
                .Where(l => l.ChartOfAccountID == aid
                            && l.JournalEntry.WebsiteID == websiteId
                            && l.JournalEntry.IsPosted
                            && l.JournalEntry.ReportToTax
                            && l.JournalEntry.EntryDate >= from
                            && l.JournalEntry.EntryDate <= to)
                .SumAsync(l => (decimal?)(l.Credit - l.Debit), ct) ?? 0;
        }

        return new TaxReconciliationDto
        {
            OrderTaxTotal = orderTax,
            SettlementTaxTotal = settlementTax,
            LedgerTaxComponentTotal = ledgerTax,
            GlTaxPayableMovement = glTax,
        };
    }

    public async Task<byte[]> ExportTrialBalanceExcelAsync(int websiteId, DateOnly from, DateOnly to, bool taxOnly = false, CancellationToken ct = default)
    {
        var rows = await GetTrialBalanceAsync(websiteId, from, to, taxOnly, ct);
        return ExcelWorkbook.Write("TrialBalance",
            new[] { "Code", "Name", "Type", "Debit", "Credit", "Balance" },
            rows.Select(r => (IReadOnlyList<object?>)new object?[] { r.Code, r.Name, r.AccountType, r.Debit, r.Credit, r.Balance }));
    }

    public async Task<byte[]> ExportProfitAndLossExcelAsync(int websiteId, DateOnly from, DateOnly to, bool taxOnly = false, CancellationToken ct = default)
    {
        var pnl = await GetProfitAndLossAsync(websiteId, from, to, taxOnly, ct);
        var list = new List<IReadOnlyList<object?>>();
        list.Add(new object?[] { "INCOME", "", "" });
        foreach (var r in pnl.Income)
            list.Add(new object?[] { r.Code, r.Name, r.Balance });
        list.Add(new object?[] { "Total income", "", pnl.TotalIncome });
        list.Add(new object?[] { "EXPENSES", "", "" });
        foreach (var r in pnl.Expenses)
            list.Add(new object?[] { r.Code, r.Name, r.Balance });
        list.Add(new object?[] { "Total expenses", "", pnl.TotalExpenses });
        list.Add(new object?[] { "Net profit/(loss)", "", pnl.NetProfit });
        return ExcelWorkbook.Write("PnL", new[] { "Code", "Name", "Amount" }, list);
    }

    public async Task<byte[]> ExportJournalsExcelAsync(int websiteId, DateOnly from, DateOnly to, bool taxOnly = false, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.JournalEntryLines.AsNoTracking()
            .Include(l => l.JournalEntry)
            .Include(l => l.ChartOfAccount)
            .Where(l => l.JournalEntry.WebsiteID == websiteId
                        && l.JournalEntry.IsPosted
                        && l.JournalEntry.EntryDate >= from
                        && l.JournalEntry.EntryDate <= to);
        if (taxOnly) q = q.Where(l => l.JournalEntry.ReportToTax);
        var lines = await q.OrderBy(l => l.JournalEntry.EntryDate).ThenBy(l => l.JournalEntryID).ToListAsync(ct);
        return ExcelWorkbook.Write("Journals",
            new[] { "Entry", "Date", "Account", "Debit", "Credit", "Description", "ReportToTax" },
            lines.Select(l => (IReadOnlyList<object?>)new object?[]
            {
                l.JournalEntry.EntryNumber,
                l.JournalEntry.EntryDate.ToString("yyyy-MM-dd"),
                $"{l.ChartOfAccount.Code} {l.ChartOfAccount.Name}",
                l.Debit, l.Credit, l.Description, l.JournalEntry.ReportToTax
            }));
    }
}
