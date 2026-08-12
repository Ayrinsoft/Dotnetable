using Dotnetable.Application.Authorization;
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
    private readonly IAdminNotificationService _notifications;

    public PayrollService(
        AppDbContext context, IHrService hr, IFinancialLedgerService ledger,
        IChartOfAccountService coa, IJournalService journals, IAdminNotificationService notifications)
    {
        _context = context;
        _hr = hr;
        _ledger = ledger;
        _coa = coa;
        _journals = journals;
        _notifications = notifications;
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

        var brackets = await _context.PayrollRateBrackets.AsNoTracking()
            .Where(b => b.WebsiteID == websiteId && b.IsActive)
            .OrderBy(b => b.Kind).ThenBy(b => b.SortOrder).ThenBy(b => b.FromAmount)
            .ToListAsync(ct);

        decimal tg = 0, tei = 0, ter = 0, ttax = 0, tnet = 0;
        foreach (var emp in employees)
        {
            var contract = await _hr.GetActiveContractAsync(emp.EmployeeID, to, ct);
            if (contract is null) continue;
            var gross = contract.BaseSalary;
            var empIns = CalcContribution(gross, contract, PayrollRateKind.EmployeeInsurance, brackets);
            var erIns = CalcContribution(gross, contract, PayrollRateKind.EmployerInsurance, brackets);
            var taxable = Math.Max(0, gross - empIns);
            var tax = CalcContribution(taxable, contract, PayrollRateKind.IncomeTax, brackets);
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
        try
        {
            await _notifications.NotifyRoleAsync(
                run.WebsiteID,
                [RoleKeys.PayrollView, RoleKeys.PayrollApprove, RoleKeys.HrView],
                AdminNotificationType.PayrollPending,
                "Payroll submitted",
                $"Run {run.RunNumber} net {run.TotalNet:0.##} {run.CurrencyCode} awaits approval.",
                $"/hr/payroll/{run.PayrollRunID}",
                run.PayrollRunID,
                ct);
        }
        catch { /* ignore */ }
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

    public async Task<byte[]> ExportRunExcelAsync(int payrollRunId, CancellationToken ct = default)
    {
        var run = await GetRunAsync(payrollRunId, ct)
            ?? throw new InvalidOperationException("Payroll run not found.");
        var headers = new[]
        {
            "EmployeeCode", "Employee", "Gross", "EmployeeInsurance", "EmployerInsurance",
            "IncomeTax", "Net", "EmployerCost", "Currency", "RunNumber", "PeriodFrom", "PeriodTo", "Status",
        };
        var rows = run.PayrollLines.Select(l => (IReadOnlyList<object?>)new object?[]
        {
            l.Employee?.EmployeeCode,
            $"{l.Employee?.GivenName} {l.Employee?.Surname}".Trim(),
            l.Gross, l.EmployeeInsurance, l.EmployerInsurance, l.IncomeTax, l.Net, l.EmployerCost,
            run.CurrencyCode, run.RunNumber, run.PeriodFrom.ToString("yyyy-MM-dd"), run.PeriodTo.ToString("yyyy-MM-dd"),
            ((PayrollRunStatus)run.Status).ToString(),
        });
        return ExcelWorkbook.Write("Payroll", headers, rows);
    }

    public async Task<byte[]> ExportInsurancePayableExcelAsync(int websiteId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var lines = await LinesInRange(websiteId, from, to, ct);
        var headers = new[]
        {
            "EmployeeCode", "Employee", "RunNumber", "PeriodFrom", "PeriodTo",
            "Gross", "EmployeeInsurance", "EmployerInsurance", "InsuranceTotal", "Currency",
        };
        var rows = lines.Select(l => (IReadOnlyList<object?>)new object?[]
        {
            l.Employee?.EmployeeCode,
            $"{l.Employee?.GivenName} {l.Employee?.Surname}".Trim(),
            l.PayrollRun?.RunNumber,
            l.PayrollRun?.PeriodFrom.ToString("yyyy-MM-dd"),
            l.PayrollRun?.PeriodTo.ToString("yyyy-MM-dd"),
            l.Gross, l.EmployeeInsurance, l.EmployerInsurance,
            l.EmployeeInsurance + l.EmployerInsurance,
            l.PayrollRun?.CurrencyCode,
        });
        return ExcelWorkbook.Write("InsurancePayable", headers, rows);
    }

    public async Task<byte[]> ExportTaxPayableExcelAsync(int websiteId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var lines = await LinesInRange(websiteId, from, to, ct);
        var headers = new[]
        {
            "EmployeeCode", "Employee", "RunNumber", "PeriodFrom", "PeriodTo",
            "Gross", "EmployeeInsurance", "TaxableBase", "IncomeTax", "Currency",
        };
        var rows = lines.Select(l => (IReadOnlyList<object?>)new object?[]
        {
            l.Employee?.EmployeeCode,
            $"{l.Employee?.GivenName} {l.Employee?.Surname}".Trim(),
            l.PayrollRun?.RunNumber,
            l.PayrollRun?.PeriodFrom.ToString("yyyy-MM-dd"),
            l.PayrollRun?.PeriodTo.ToString("yyyy-MM-dd"),
            l.Gross, l.EmployeeInsurance, Math.Max(0, l.Gross - l.EmployeeInsurance),
            l.IncomeTax, l.PayrollRun?.CurrencyCode,
        });
        return ExcelWorkbook.Write("TaxPayable", headers, rows);
    }

    public async Task<byte[]> ExportStatutoryExcelAsync(int websiteId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        // Combined sheet for insurance + tax payable filings.
        var lines = await LinesInRange(websiteId, from, to, ct);
        var headers = new[]
        {
            "EmployeeCode", "Employee", "RunNumber", "PeriodFrom", "PeriodTo",
            "Gross", "EmployeeInsurance", "EmployerInsurance", "IncomeTax", "Net", "EmployerCost", "Currency",
        };
        var rows = lines.Select(l => (IReadOnlyList<object?>)new object?[]
        {
            l.Employee?.EmployeeCode,
            $"{l.Employee?.GivenName} {l.Employee?.Surname}".Trim(),
            l.PayrollRun?.RunNumber,
            l.PayrollRun?.PeriodFrom.ToString("yyyy-MM-dd"),
            l.PayrollRun?.PeriodTo.ToString("yyyy-MM-dd"),
            l.Gross, l.EmployeeInsurance, l.EmployerInsurance, l.IncomeTax, l.Net, l.EmployerCost,
            l.PayrollRun?.CurrencyCode,
        });
        return ExcelWorkbook.Write("Statutory", headers, rows);
    }

    public async Task<byte[]> ExportPayslipsExcelAsync(int payrollRunId, CancellationToken ct = default)
    {
        var run = await GetRunAsync(payrollRunId, ct)
            ?? throw new InvalidOperationException("Payroll run not found.");
        var headers = new[]
        {
            "PayslipTitle", "EmployeeCode", "Employee", "NationalId", "JobTitle",
            "RunNumber", "PeriodFrom", "PeriodTo", "Currency",
            "Gross", "EmployeeInsurance", "EmployerInsurance", "IncomeTax", "Net", "EmployerCost", "Status",
        };
        var rows = run.PayrollLines.Select(l => (IReadOnlyList<object?>)new object?[]
        {
            $"Payslip {run.RunNumber}",
            l.Employee?.EmployeeCode,
            $"{l.Employee?.GivenName} {l.Employee?.Surname}".Trim(),
            l.Employee?.NationalId,
            l.Employee?.JobTitle,
            run.RunNumber,
            run.PeriodFrom.ToString("yyyy-MM-dd"),
            run.PeriodTo.ToString("yyyy-MM-dd"),
            run.CurrencyCode,
            l.Gross, l.EmployeeInsurance, l.EmployerInsurance, l.IncomeTax, l.Net, l.EmployerCost,
            ((PayrollRunStatus)run.Status).ToString(),
        });
        return ExcelWorkbook.Write("Payslips", headers, rows);
    }

    public async Task<string?> BuildPayslipHtmlAsync(int payrollLineId, CancellationToken ct = default)
    {
        var line = await _context.PayrollLines.AsNoTracking()
            .Include(l => l.Employee)
            .Include(l => l.PayrollRun)
            .FirstOrDefaultAsync(l => l.PayrollLineID == payrollLineId, ct);
        if (line is null) return null;
        return BuildPayslipHtml(line, line.PayrollRun);
    }

    public async Task<string?> BuildRunPayslipsHtmlAsync(int payrollRunId, CancellationToken ct = default)
    {
        var run = await GetRunAsync(payrollRunId, ct);
        if (run is null || run.PayrollLines.Count == 0) return null;
        var parts = run.PayrollLines.Select(l => BuildPayslipHtml(l, run));
        return string.Join("<div style=\"page-break-after:always\"></div>", parts);
    }

    private static string BuildPayslipHtml(PayrollLine line, PayrollRun run)
    {
        var name = $"{line.Employee?.GivenName} {line.Employee?.Surname}".Trim();
        string Row(string label, decimal amount) =>
            $"<tr><td style=\"padding:4px 8px\">{label}</td><td style=\"padding:4px 8px;text-align:right\">{amount:0.##} {run.CurrencyCode}</td></tr>";
        return $"""
            <div class="payslip" style="font-family:Segoe UI,Tahoma,sans-serif;max-width:640px;margin:0 auto;padding:16px;border:1px solid #ccc">
              <h2 style="margin:0 0 8px">Payslip</h2>
              <div style="margin-bottom:12px;color:#444">
                <div><strong>{System.Net.WebUtility.HtmlEncode(run.RunNumber)}</strong> · {run.PeriodFrom:yyyy-MM-dd} → {run.PeriodTo:yyyy-MM-dd}</div>
                <div>{System.Net.WebUtility.HtmlEncode(name)} ({System.Net.WebUtility.HtmlEncode(line.Employee?.EmployeeCode ?? "")})</div>
                <div>{System.Net.WebUtility.HtmlEncode(line.Employee?.JobTitle ?? "")}</div>
              </div>
              <table style="width:100%;border-collapse:collapse">
                {Row("Gross", line.Gross)}
                {Row("Employee insurance", line.EmployeeInsurance)}
                {Row("Employer insurance", line.EmployerInsurance)}
                {Row("Income tax", line.IncomeTax)}
                {Row("Net pay", line.Net)}
                {Row("Employer cost", line.EmployerCost)}
              </table>
              <p style="margin-top:16px;font-size:12px;color:#666">Status: {(PayrollRunStatus)run.Status}</p>
            </div>
            """;
    }

    public async Task<IReadOnlyList<PayrollRateBracket>> GetRateBracketsAsync(
        int websiteId, PayrollRateKind? kind = null, CancellationToken ct = default)
    {
        var q = _context.PayrollRateBrackets.AsNoTracking().Where(b => b.WebsiteID == websiteId);
        if (kind is PayrollRateKind k) q = q.Where(b => b.Kind == (byte)k);
        return await q.OrderBy(b => b.Kind).ThenBy(b => b.SortOrder).ThenBy(b => b.FromAmount).ToListAsync(ct);
    }

    public async Task<(bool Success, string? Error, PayrollRateBracket? Bracket)> UpsertRateBracketAsync(
        PayrollRateBracket bracket, CancellationToken ct = default)
    {
        if (bracket.WebsiteID <= 0) return (false, "Website is required.", null);
        if (bracket.Rate < 0 || bracket.Rate > 1) return (false, "Rate must be between 0 and 1 (e.g. 0.10 = 10%).", null);
        if (bracket.FromAmount < 0) return (false, "From amount cannot be negative.", null);
        if (bracket.ToAmount is decimal to && to <= bracket.FromAmount)
            return (false, "To amount must be greater than From amount.", null);
        if (!Enum.IsDefined(typeof(PayrollRateKind), bracket.Kind))
            return (false, "Invalid rate kind.", null);

        if (bracket.PayrollRateBracketID == 0)
        {
            bracket.CreatedAt = DateTime.UtcNow;
            _context.PayrollRateBrackets.Add(bracket);
        }
        else
        {
            var e = await _context.PayrollRateBrackets.FirstOrDefaultAsync(b => b.PayrollRateBracketID == bracket.PayrollRateBracketID, ct);
            if (e is null) return (false, "Bracket not found.", null);
            e.Kind = bracket.Kind;
            e.FromAmount = bracket.FromAmount;
            e.ToAmount = bracket.ToAmount;
            e.Rate = bracket.Rate;
            e.SortOrder = bracket.SortOrder;
            e.IsActive = bracket.IsActive;
            bracket = e;
        }
        await _context.SaveChangesAsync(ct);
        return (true, null, bracket);
    }

    public async Task<(bool Success, string? Error)> DeleteRateBracketAsync(int bracketId, CancellationToken ct = default)
    {
        var e = await _context.PayrollRateBrackets.FirstOrDefaultAsync(b => b.PayrollRateBracketID == bracketId, ct);
        if (e is null) return (false, "Not found.");
        _context.PayrollRateBrackets.Remove(e);
        await _context.SaveChangesAsync(ct);
        return (true, null);
    }

    private async Task<IReadOnlyList<PayrollLine>> LinesInRange(int websiteId, DateOnly from, DateOnly to, CancellationToken ct) =>
        await _context.PayrollLines.AsNoTracking()
            .Include(l => l.Employee)
            .Include(l => l.PayrollRun)
            .Where(l => l.PayrollRun.WebsiteID == websiteId
                        && l.PayrollRun.Status == (byte)PayrollRunStatus.Paid
                        && l.PayrollRun.PeriodFrom >= from && l.PayrollRun.PeriodTo <= to)
            .OrderBy(l => l.Employee.Surname)
            .ToListAsync(ct);

    /// <summary>
    /// Flat contract rates when UseFlatRates; otherwise progressive website brackets (fallback to flat).
    /// Progressive: tax each slice of the base between From/To at that bracket's rate.
    /// </summary>
    private static decimal CalcContribution(
        decimal baseAmount,
        EmployeeContract contract,
        PayrollRateKind kind,
        IReadOnlyList<PayrollRateBracket> allBrackets)
    {
        if (baseAmount <= 0) return 0;

        if (contract.UseFlatRates)
        {
            var flat = kind switch
            {
                PayrollRateKind.EmployeeInsurance => contract.EmployeeInsuranceRate,
                PayrollRateKind.EmployerInsurance => contract.EmployerInsuranceRate,
                PayrollRateKind.IncomeTax => contract.IncomeTaxRate,
                _ => 0m,
            };
            return Math.Round(baseAmount * flat, 4);
        }

        var brackets = allBrackets.Where(b => b.Kind == (byte)kind && b.IsActive)
            .OrderBy(b => b.SortOrder).ThenBy(b => b.FromAmount).ToList();
        if (brackets.Count == 0)
        {
            // No brackets configured — fall back to contract flat rates.
            var flat = kind switch
            {
                PayrollRateKind.EmployeeInsurance => contract.EmployeeInsuranceRate,
                PayrollRateKind.EmployerInsurance => contract.EmployerInsuranceRate,
                PayrollRateKind.IncomeTax => contract.IncomeTaxRate,
                _ => 0m,
            };
            return Math.Round(baseAmount * flat, 4);
        }

        decimal total = 0;
        foreach (var b in brackets)
        {
            if (baseAmount <= b.FromAmount) continue;
            var upper = b.ToAmount ?? decimal.MaxValue;
            var sliceStart = b.FromAmount;
            var sliceEnd = Math.Min(baseAmount, upper);
            if (sliceEnd <= sliceStart) continue;
            total += (sliceEnd - sliceStart) * b.Rate;
        }
        return Math.Round(total, 4);
    }
}
