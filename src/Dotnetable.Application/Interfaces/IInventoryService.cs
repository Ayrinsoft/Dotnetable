using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Movement kind recorded on <see cref="StockMovement"/>.Type (TINYINT). No Domain enum exists yet for
/// this column, so this is the single source of truth for the numeric values used by <see cref="IInventoryService"/>.</summary>
public enum StockMovementType : byte
{
    Purchase = 1,
    Sale = 2,
    Adjustment = 3,
    Return = 4,
}

/// <summary>Availability snapshot for a single product variant, derived from its <see cref="InventoryItem"/> row.</summary>
public readonly record struct StockAvailability(int OnHand, int Reserved, int Available);

/// <summary>Stock levels, reservations and admin adjustments for <see cref="InventoryItem"/> rows.
/// One row per (WebsiteID, ProductVariantID); rows are created lazily on first adjustment.</summary>
public interface IInventoryService
{
    Task<StockAvailability> GetAvailabilityAsync(int websiteId, int variantId, CancellationToken ct = default);

    Task<Dictionary<int, StockAvailability>> GetAvailabilityBulkAsync(int websiteId, IEnumerable<int> variantIds, CancellationToken ct = default);

    /// <summary>Increments QuantityReserved by <paramref name="qty"/> when enough stock is available.
    /// Guarded by <see cref="InventoryItem.RowVersion"/>; retries once on a concurrency conflict, then reports failure.</summary>
    Task<bool> ReserveAsync(int websiteId, int variantId, int qty, CancellationToken ct = default);

    /// <summary>Releases a previously reserved quantity (e.g. cart expiry, cancelled checkout).</summary>
    Task ReleaseReservationAsync(int websiteId, int variantId, int qty, CancellationToken ct = default);

    /// <summary>Fulfils an order line: writes a Sale <see cref="StockMovement"/> and decrements both
    /// QuantityOnHand and QuantityReserved by <paramref name="qty"/>.</summary>
    Task DecrementOnFulfillAsync(int websiteId, int variantId, int qty, int? orderId, int? orderItemId, int? memberId, CancellationToken ct = default);

    /// <summary>Manual admin stock adjustment: writes an Adjustment <see cref="StockMovement"/> and changes
    /// QuantityOnHand by <paramref name="delta"/> (positive or negative), creating the <see cref="InventoryItem"/>
    /// row if it doesn't exist yet for that website+variant.</summary>
    Task AdjustAsync(int websiteId, int variantId, int delta, decimal? unitCostUsd, string? note, int memberId, CancellationToken ct = default);

    /// <summary>
    /// Sets absolute on-hand quantity for a variant (used when registering/editing a product).
    /// Creates the inventory row if missing. When the value changes, writes an Adjustment
    /// <see cref="StockMovement"/> for the delta. Rejects values below the reserved quantity.
    /// </summary>
    Task SetOnHandAsync(int websiteId, int variantId, int quantityOnHand, string? note, int memberId, CancellationToken ct = default);

    /// <summary>Paged admin stock grid, joined to variant + product for display.</summary>
    Task<PagedResult<InventoryItem>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default);

    Task<List<InventoryItem>> GetLowStockAsync(int websiteId, CancellationToken ct = default);
}
