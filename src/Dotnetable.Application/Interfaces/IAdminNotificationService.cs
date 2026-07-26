using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

public interface IAdminNotificationService
{
    Task NotifySiteAdminsAsync(
        int websiteId,
        AdminNotificationType type,
        string title,
        string message,
        string? actionUrl = null,
        int? relatedEntityId = null,
        CancellationToken ct = default);

    /// <summary>In-app (+ optional email) notification for a single member.</summary>
    Task NotifyMemberAsync(
        int memberId,
        int websiteId,
        AdminNotificationType type,
        string title,
        string message,
        string? actionUrl = null,
        int? relatedEntityId = null,
        CancellationToken ct = default);

    Task<PagedResult<AdminNotification>> GetPagedAsync(int memberId, GridQuery query, CancellationToken ct = default);

    Task<IReadOnlyList<AdminNotification>> GetRecentAsync(int memberId, int take = 10, CancellationToken ct = default);

    Task<int> GetUnreadCountAsync(int memberId, CancellationToken ct = default);

    Task MarkReadAsync(int notificationId, int memberId, CancellationToken ct = default);

    Task MarkAllReadAsync(int memberId, CancellationToken ct = default);

    Task DeleteAsync(int notificationId, int memberId, CancellationToken ct = default);
}
