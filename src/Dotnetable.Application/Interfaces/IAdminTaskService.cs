using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Aggregates open operational queues (orders, offline payments, refunds, withdrawals)
/// so admins see real pending work on the home page and notifications page — not only
/// historical <see cref="IAdminNotificationService"/> rows.
/// </summary>
public interface IAdminTaskService
{
    /// <param name="websiteId">Null = all websites (master). Otherwise scope to one site.</param>
    Task<IReadOnlyList<AdminOpenTask>> GetOpenTasksAsync(
        int? websiteId,
        bool includeOrders,
        bool includePayments,
        bool includeRefunds,
        bool includeWithdrawals,
        CancellationToken ct = default);
}
