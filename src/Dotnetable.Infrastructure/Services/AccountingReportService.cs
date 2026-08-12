using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class AccountingReportService : IAccountingReportService
{
    private readonly AppDbContext _context;
    private readonly IChartOfAccountService _coa;

    public AccountingReportService(AppDbContext context, IChartOfAccountService coa)
    {
        _context = context;
        _coa = coa;
    }

    public async Task<IReadOnlyList<TrialBalanceRowDto>> GetTrialBalanceAsync(
        int websiteId, DateOnly from, DateOnly to, bool taxOnly = false, CancellationToken ct = default)
    {
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
}
