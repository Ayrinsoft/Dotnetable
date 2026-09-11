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

    /// <summary>
    /// In-app + email + WhatsApp (when configured) for members who have any of the given role keys
    /// on their policy (or are site admins). Falls back to all site admins if no role matches.
    /// </summary>
    Task NotifyRoleAsync(
        int websiteId,
        IReadOnlyList<string> roleKeys,
        AdminNotificationType type,
        string title,
        string message,
        string? actionUrl = null,
        int? relatedEntityId = null,
        CancellationToken ct = default);

    /// <summary>In-app (+ optional email/WhatsApp) notification for a single member.</summary>
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

    /// <summary>
    /// Deletes every notification (for every recipient) raised for a given source record — e.g. when
    /// the contact message / order / ticket a notification pointed at is itself deleted, so the
    /// Dashboard's unread badge doesn't keep counting a notification whose target no longer exists.
    /// </summary>
    Task DeleteByRelatedEntityAsync(AdminNotificationType type, int relatedEntityId, CancellationToken ct = default);
}
