using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

public interface IPayrollService
{
    Task<PagedResult<PayrollRun>> GetRunsAsync(int websiteId, GridQuery query, CancellationToken ct = default);
    Task<PayrollRun?> GetRunAsync(int payrollRunId, CancellationToken ct = default);
    Task<(bool Success, string? Error, PayrollRun? Run)> CreateRunAsync(
        int websiteId, DateOnly from, DateOnly to, int? memberId, CancellationToken ct = default);
    Task<(bool Success, string? Error)> SubmitAsync(int runId, int? memberId, CancellationToken ct = default);
    Task<(bool Success, string? Error)> ApproveAsync(int runId, int? memberId, CancellationToken ct = default);
    Task<(bool Success, string? Error)> MarkPaidAsync(int runId, int? memberId, CancellationToken ct = default);
    Task<IReadOnlyList<PayrollLine>> GetInsuranceReportAsync(int websiteId, DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<IReadOnlyList<PayrollLine>> GetTaxReportAsync(int websiteId, DateOnly from, DateOnly to, CancellationToken ct = default);

    Task<byte[]> ExportRunExcelAsync(int payrollRunId, CancellationToken ct = default);
    Task<byte[]> ExportInsurancePayableExcelAsync(int websiteId, DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<byte[]> ExportTaxPayableExcelAsync(int websiteId, DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<byte[]> ExportStatutoryExcelAsync(int websiteId, DateOnly from, DateOnly to, CancellationToken ct = default);

    /// <summary>One-sheet Excel of printable payslip fields (one row per employee line).</summary>
    Task<byte[]> ExportPayslipsExcelAsync(int payrollRunId, CancellationToken ct = default);

    /// <summary>HTML fragment for a single employee payslip (print via admin print helper).</summary>
    Task<string?> BuildPayslipHtmlAsync(int payrollLineId, CancellationToken ct = default);

    /// <summary>HTML for all lines in a run (page-break between employees).</summary>
    Task<string?> BuildRunPayslipsHtmlAsync(int payrollRunId, CancellationToken ct = default);

    Task<IReadOnlyList<PayrollRateBracket>> GetRateBracketsAsync(int websiteId, PayrollRateKind? kind = null, CancellationToken ct = default);
    Task<(bool Success, string? Error, PayrollRateBracket? Bracket)> UpsertRateBracketAsync(PayrollRateBracket bracket, CancellationToken ct = default);
    Task<(bool Success, string? Error)> DeleteRateBracketAsync(int bracketId, CancellationToken ct = default);
}
