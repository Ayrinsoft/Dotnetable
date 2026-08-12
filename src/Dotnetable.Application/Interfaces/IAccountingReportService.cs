using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

public interface IAccountingReportService
{
    Task<IReadOnlyList<TrialBalanceRowDto>> GetTrialBalanceAsync(
        int websiteId, DateOnly from, DateOnly to, bool taxOnly = false, CancellationToken ct = default);

    Task<ProfitAndLossDto> GetProfitAndLossAsync(
        int websiteId, DateOnly from, DateOnly to, bool taxOnly = false, CancellationToken ct = default);
}
