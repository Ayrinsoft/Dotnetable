using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

public interface IStaffTaskService
{
    Task<IReadOnlyList<StaffTaskColleagueDto>> ListColleaguesAsync(
        int websiteId, int actorMemberId, CancellationToken ct = default);

    Task<IReadOnlyList<StaffTaskRelatedHitDto>> SearchRelatedAsync(
        int websiteId, StaffTaskRelatedKind kind, string? query, CancellationToken ct = default);

    Task<IReadOnlyList<StaffTaskDto>> ListAsync(StaffTaskListFilter filter, CancellationToken ct = default);

    Task<IReadOnlyList<(byte Status, int Count)>> CountByStatusAsync(
        StaffTaskListFilter filter, CancellationToken ct = default);

    Task<int> CountOpenAsync(int websiteId, int actorMemberId, bool canManage, CancellationToken ct = default);

    Task<StaffTaskDto?> GetByIdAsync(int staffTaskId, CancellationToken ct = default);

    Task<(bool Success, string? Error, StaffTaskDto? Task)> CreateAsync(
        StaffTaskWriteRequest request, int actorMemberId, bool canManage, CancellationToken ct = default);

    Task<(bool Success, string? Error, StaffTaskDto? Task)> UpdateAsync(
        int staffTaskId, StaffTaskWriteRequest request, int actorMemberId, bool canManage, CancellationToken ct = default);

    Task<(bool Success, string? Error)> SetStatusAsync(
        int staffTaskId, StaffTaskStatus status, int actorMemberId, bool canManage, CancellationToken ct = default);

    Task<(bool Success, string? Error)> DeleteAsync(
        int staffTaskId, int actorMemberId, bool canManage, CancellationToken ct = default);
}
