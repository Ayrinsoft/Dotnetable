using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface IFiscalPeriodService
{
    Task<IReadOnlyList<FiscalPeriod>> GetAllAsync(int websiteId, CancellationToken ct = default);
    Task<FiscalPeriod> EnsureYearAsync(int websiteId, int year, CancellationToken ct = default);
    Task<(bool Success, string? Error)> CloseAsync(int fiscalPeriodId, int? memberId, CancellationToken ct = default);
    Task<(bool Success, string? Error)> ReopenAsync(int fiscalPeriodId, int? memberId, CancellationToken ct = default);
}
