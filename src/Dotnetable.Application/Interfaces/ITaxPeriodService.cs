using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

public interface ITaxPeriodService
{
    Task<PagedResult<TaxPeriod>> GetPagedAsync(int websiteId, byte? status, GridQuery query, CancellationToken ct = default);
    Task<TaxPeriod?> GetByIdAsync(int taxPeriodId, CancellationToken ct = default);
    Task<(bool Success, string? Error, TaxPeriod? Period)> CreateAsync(
        int websiteId, string periodCode, DateOnly from, DateOnly to, string? note, int? memberId, CancellationToken ct = default);
    Task<(bool Success, string? Error)> UpdateStatusAsync(int taxPeriodId, TaxPeriodStatus status, int? memberId, CancellationToken ct = default);
    /// <summary>Runs live VAT report for the period dates and stores snapshot totals.</summary>
    Task<(bool Success, string? Error, TaxPeriod? Period)> GenerateSnapshotAsync(int taxPeriodId, int? memberId, CancellationToken ct = default);
    Task<(bool Success, string? Error)> CloseAsync(int taxPeriodId, int? memberId, CancellationToken ct = default);
}
