using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Values for <see cref="Order"/>.Status (TINYINT). No Domain enum exists yet for this column,
/// so this is the single source of truth for the numeric values used by <see cref="IOrderService"/>.</summary>
public enum OrderStatus : byte
{
    PendingPayment = 1,
    Paid = 2,
    Processing = 3,
    Shipped = 4,
    Completed = 5,
    Cancelled = 6,
    Refunded = 7,
}

/// <summary>Statuses that still need admin/fulfillment action (excludes terminal Completed / Cancelled / Refunded).</summary>
public static class OrderStatusQueues
{
    public static readonly OrderStatus[] Actionable =
    [
        OrderStatus.PendingPayment,
        OrderStatus.Paid,
        OrderStatus.Processing,
        OrderStatus.Shipped,
    ];
}

/// <summary>Result of a checkout attempt.</summary>
public sealed record CheckoutResult(bool Success, string? Error, int? OrderId, string? OrderNumber);

/// <summary>
/// Converts a cart into an order (price/shipping/tax/discount snapshot, stock reservation) and manages
/// the order status lifecycle. Amounts are always snapshotted in both the order's display currency and
/// USD at the moment of checkout — later exchange-rate changes never retroactively change an order.
/// </summary>
public interface IOrderService
{
    Task<CheckoutResult> CheckoutAsync(
        int websiteId, int clientId, int cartId, int addressId, int shippingMethodId,
        string? currencyCode = null, string? note = null, CancellationToken ct = default);

    /// <summary>An order by id, scoped to a client when <paramref name="clientId"/> is given (404s otherwise), unrestricted for admin use when null.</summary>
    Task<Order?> GetByIdAsync(int orderId, int? clientId = null, CancellationToken ct = default);

    Task<PagedResult<Order>> GetPagedAsync(int? websiteId, byte? status, GridQuery query, CancellationToken ct = default);

    /// <summary>Counts of orders per <see cref="Order"/>.Status for the optional website scope.</summary>
    Task<IReadOnlyDictionary<byte, int>> GetStatusCountsAsync(int? websiteId, CancellationToken ct = default);

    Task<PagedResult<Order>> GetClientHistoryAsync(int clientId, GridQuery query, CancellationToken ct = default);

    /// <summary>Validates the transition, writes an <see cref="OrderStatusHistory"/> entry, and on
    /// entering Paid/Processing decrements reserved stock into real stock movements; on Cancelled/Refunded
    /// releases any still-reserved stock.</summary>
    Task<bool> TransitionStatusAsync(int orderId, OrderStatus newStatus, int? memberId, string? note, CancellationToken ct = default);

    /// <summary>True when the client has a Paid-or-later order containing this product (drives review "verified purchase").</summary>
    Task<bool> ClientHasPaidOrderForProductAsync(int clientId, int productId, CancellationToken ct = default);
}
