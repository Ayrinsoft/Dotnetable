using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface ISettlementService
{
    Task<PagedResult<Settlement>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default);
    Task<Settlement?> GetByIdAsync(int settlementId, CancellationToken ct = default);
    Task<bool> ApproveAsync(int settlementId, int? memberId, CancellationToken ct = default);
    Task<bool> MarkPaidAsync(int settlementId, int? bankAccountId, string? paymentRef, int? memberId, CancellationToken ct = default);
    Task<bool> CancelAsync(int settlementId, string? note, int? memberId, CancellationToken ct = default);

    /// <summary>Vendor FX / cost report: how much site currency was spent for destination-currency payables.</summary>
    Task<IReadOnlyList<SettlementFxReportRow>> GetFxReportAsync(
        int websiteId, DateOnly? from, DateOnly? to, int? vendorId, CancellationToken ct = default);
}
