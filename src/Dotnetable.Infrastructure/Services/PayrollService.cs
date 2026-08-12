using Dotnetable.Application.DTOs;
using Dotnetable.Application.Financial;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class PayrollService : IPayrollService
{
    private readonly AppDbContext _context;
    private readonly IHrService _hr;
    private readonly IFinancialLedgerService _ledger;
    private readonly IChartOfAccountService _coa;
    private readonly IJournalService _journals;

    public PayrollService(
        AppDbContext context, IHrService hr, IFinancialLedgerService ledger,
        IChartOfAccountService coa, IJournalService journals)
    {
        _context = context;
        _hr = hr;
        _ledger = ledger;
        _coa = coa;
        _journals = journals;
    }

    public async Task<PagedResult<PayrollRun>> GetRunsAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.PayrollRuns.AsNoTracking().Where(r => r.WebsiteID == websiteId);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(r => r.PeriodFrom).Skip(query.Skip).Take(query.Take).ToListAsync(ct);
        return new PagedResult<PayrollRun> { Items = items, TotalCount = total };
    }

    public async Task<PayrollRun?> GetRunAsync(int payrollRunId, CancellationToken ct = default) =>
        await _context.PayrollRuns.AsNoTracking()
            .Include(r => r.PayrollLines).ThenInclude(l => l.Employee)
            .FirstOrDefaultAsync(r => r.PayrollRunID == payrollRunId, ct);

    public async Task<(bool Success, string? Error, PayrollRun? Run)> CreateRunAsync(
        int websiteId, DateOnly from, DateOnly to, int? memberId, CancellationToken ct = default)
    {
        if (to < from) return (false, "Invalid period.", null);
        var website = await _context.Websites.AsNoTracking().FirstOrDefaultAsync(w => w.WebsiteID == websiteId, ct);
        if (website is null) return (false, "Website not found.", null);

        var employees = await _context.Employees
            .Where(e => e.WebsiteID == websiteId && e.Status == (byte)EmployeeStatus.Active)
            .ToListAsync(ct);
        if (employees.Count == 0) return (false, "No active employees.", null);

        var currency = website.DefaultCurrencyCode;
        var seq = await _context.PayrollRuns.CountAsync(r => r.WebsiteID == websiteId, ct) + 1;
        var run = new PayrollRun
        {
            WebsiteID = websiteId,
            RunNumber = $"PR-{from:yyyyMM}-{seq:D4}",
            PeriodFrom = from,
            PeriodTo = to,
            Status = (byte)PayrollRunStatus.Draft,
            CurrencyCode = currency,
            CreatedByMemberID = memberId,
            CreatedAt = DateTime.UtcNow,
        };

        decimal tg = 0, tei = 0, ter = 0, ttax = 0, tnet = 0;
        foreach (var emp in employees)
        {
            var contract = await _hr.GetActiveContractAsync(emp.EmployeeID, to, ct);
            if (contract is null) continue;
            var gross = contract.BaseSalary;
            var empIns = Math.Round(gross * contract.EmployeeInsuranceRate, 4);
            var erIns = Math.Round(gross * contract.EmployerInsuranceRate, 4);
            var tax = Math.Round(Math.Max(0, gross - empIns) * contract.IncomeTaxRate, 4);
            var net = gross - empIns - tax;
            run.PayrollLines.Add(new PayrollLine
            {
                EmployeeID = emp.EmployeeID,
                Gross = gross,
                EmployeeInsurance = empIns,
                EmployerInsurance = erIns,
                IncomeTax = tax,
                Net = net,
                EmployerCost = gross + erIns,
            });
            tg += gross; tei += empIns; ter += erIns; ttax += tax; tnet += net;
        }
        if (run.PayrollLines.Count == 0) return (false, "No employees with active contracts in period.", null);

        run.TotalGross = tg;
        run.TotalEmployeeInsurance = tei;
        run.TotalEmployerInsurance = ter;
        run.TotalIncomeTax = ttax;
        run.TotalNet = tnet;
        _context.PayrollRuns.Add(run);
        await _context.SaveChangesAsync(ct);
        return (true, null, run);
    }

    public async Task<(bool Success, string? Error)> SubmitAsync(int runId, int? memberId, CancellationToken ct = default)
    {
        var run = await _context.PayrollRuns.FirstOrDefaultAsync(r => r.PayrollRunID == runId, ct);
        if (run is null) return (false, "Not found.");
        if (run.Status != (byte)PayrollRunStatus.Draft) return (false, "Only draft runs can be submitted.");
        run.Status = (byte)PayrollRunStatus.Submitted;
        await _context.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ApproveAsync(int runId, int? memberId, CancellationToken ct = default)
    {
        var run = await _context.PayrollRuns.FirstOrDefaultAsync(r => r.PayrollRunID == runId, ct);
        if (run is null) return (false, "Not found.");
        if (run.Status is not ((byte)PayrollRunStatus.Draft or (byte)PayrollRunStatus.Submitted))
            return (false, "Invalid status.");
        run.Status = (byte)PayrollRunStatus.Approved;
        run.ApprovedAt = DateTime.UtcNow;
        run.ApprovedByMemberID = memberId;
        await _context.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> MarkPaidAsync(int runId, int? memberId, CancellationToken ct = default)
    {
        var run = await _context.PayrollRuns
            .Include(r => r.PayrollLines)
            .FirstOrDefaultAsync(r => r.PayrollRunID == runId, ct);
        if (run is null) return (false, "Not found.");
        if (run.Status != (byte)PayrollRunStatus.Approved) return (false, "Approve the run before paying.");

        run.Status = (byte)PayrollRunStatus.Paid;
        run.PaidAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        var group = Guid.NewGuid();
        var now = DateTime.Now;
        // L1 operational components
        await PostL1(run, FinancialTransactionTypes.Manual, FinancialFlow.Out, run.TotalNet, "Payroll net pay", group, memberId, ct);
        await PostL1(run, FinancialTransactionTypes.Manual, FinancialFlow.Component, run.TotalEmployeeInsurance, "Employee insurance withheld", group, memberId, ct);
        await PostL1(run, FinancialTransactionTypes.Manual, FinancialFlow.Component, run.TotalIncomeTax, "Payroll income tax withheld", group, memberId, ct);
        await PostL1(run, FinancialTransactionTypes.Manual, FinancialFlow.Component, run.TotalEmployerInsurance, "Employer insurance", group, memberId, ct);
        await PostL1(run, FinancialTransactionTypes.Manual, FinancialFlow.Component, run.TotalGross, "Payroll gross", group, memberId, ct);

        // Dedicated payroll GL is posted below; skip L1 auto-projector for this group.

        try
        {
            await _coa.EnsureSeededAsync(run.WebsiteID, ct);
            var codes = await _context.ChartOfAccounts.AsNoTracking()
                .Where(a => a.WebsiteID == run.WebsiteID && a.IsSystem)
                .ToDictionaryAsync(a => a.Code, a => a.ChartOfAccountID, StringComparer.OrdinalIgnoreCase, ct);
            int A(string c) => codes.GetValueOrDefault(c);
            var cash = A("1100"); var insPay = A("2300"); var taxPay = A("2200"); var payPay = A("2400");
            var exp = A("5200"); var erExp = A("5300");
            var lines = new List<JournalLineDto>
            {
                new() { ChartOfAccountID = exp, Debit = run.TotalGross, Credit = 0, Description = "Gross payroll" },
                new() { ChartOfAccountID = erExp, Debit = run.TotalEmployerInsurance, Credit = 0, Description = "Employer insurance" },
                new() { ChartOfAccountID = cash, Debit = 0, Credit = run.TotalNet, Description = "Net paid" },
                new() { ChartOfAccountID = insPay, Debit = 0, Credit = run.TotalEmployeeInsurance + run.TotalEmployerInsurance, Description = "Insurance payable" },
                new() { ChartOfAccountID = taxPay, Debit = 0, Credit = run.TotalIncomeTax, Description = "Tax payable" },
            };
            // balance residual to payroll payable
            var d = lines.Sum(l => l.Debit); var c = lines.Sum(l => l.Credit);
            if (d != c && payPay > 0)
            {
                if (d > c) lines.Add(new() { ChartOfAccountID = payPay, Debit = 0, Credit = d - c, Description = "Payroll payable residual" });
                else lines.Add(new() { ChartOfAccountID = payPay, Debit = c - d, Credit = 0, Description = "Payroll payable residual" });
            }
            var (ok, _, entry) = await _journals.CreateDraftAsync(
                run.WebsiteID, DateOnly.FromDateTime(DateTime.UtcNow),
                $"Payroll {run.RunNumber}", run.CurrencyCode, true, lines, memberId,
                "Payroll", $"PAYROLL:{run.PayrollRunID}", ct);
            if (ok && entry is not null)
                await _journals.PostAsync(entry.JournalEntryID, memberId, ct);
        }
        catch { /* GL optional */ }

        return (true, null);
    }

    private async Task PostL1(PayrollRun run, string type, byte flow, decimal amount, string title, Guid group, int? memberId, CancellationToken ct)
    {
        if (amount <= 0) return;
        await _ledger.PostAsync(new PostFinancialEntryRequest
        {
            WebsiteId = run.WebsiteID,
            TransactionType = type,
            Flow = flow,
            Amount = amount,
            AmountUsd = amount,
            CurrencyCode = run.CurrencyCode,
            Title = $"{title} ({run.RunNumber})",
            ReportToTax = true,
            VendorVisible = false,
            EventGroupId = group,
            MemberId = memberId,
            MetaJson = $"{{\"payrollRunId\":{run.PayrollRunID}}}",
            SkipGlProjection = true,
        }, ct);
    }

    public async Task<IReadOnlyList<PayrollLine>> GetInsuranceReportAsync(int websiteId, DateOnly from, DateOnly to, CancellationToken ct = default) =>
        await LinesInRange(websiteId, from, to, ct);

    public async Task<IReadOnlyList<PayrollLine>> GetTaxReportAsync(int websiteId, DateOnly from, DateOnly to, CancellationToken ct = default) =>
        await LinesInRange(websiteId, from, to, ct);

    private async Task<IReadOnlyList<PayrollLine>> LinesInRange(int websiteId, DateOnly from, DateOnly to, CancellationToken ct) =>
        await _context.PayrollLines.AsNoTracking()
            .Include(l => l.Employee)
            .Include(l => l.PayrollRun)
            .Where(l => l.PayrollRun.WebsiteID == websiteId
                        && l.PayrollRun.Status == (byte)PayrollRunStatus.Paid
                        && l.PayrollRun.PeriodFrom >= from && l.PayrollRun.PeriodTo <= to)
            .OrderBy(l => l.Employee.Surname)
            .ToListAsync(ct);
}
