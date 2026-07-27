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
}
