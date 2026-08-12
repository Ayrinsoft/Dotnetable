using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface IChartOfAccountService
{
    Task<IReadOnlyList<ChartAccountDto>> GetTreeAsync(int websiteId, CancellationToken ct = default);
    Task<IReadOnlyList<ChartOfAccount>> GetFlatAsync(int websiteId, CancellationToken ct = default);
    /// <summary>Creates the standard chart + default ledger maps if the website has no accounts yet.</summary>
    Task EnsureSeededAsync(int websiteId, CancellationToken ct = default);
    Task<ChartOfAccount> UpsertAsync(ChartOfAccount account, CancellationToken ct = default);
    Task SetActiveAsync(int accountId, bool active, CancellationToken ct = default);
}
