using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface IWarehouseService
{
    Task<IReadOnlyList<Warehouse>> GetAllAsync(int websiteId, CancellationToken ct = default);
    Task<Warehouse> UpsertAsync(Warehouse warehouse, CancellationToken ct = default);
    Task EnsureDefaultAsync(int websiteId, CancellationToken ct = default);
    Task<IReadOnlyList<WarehouseStock>> GetStockAsync(int warehouseId, CancellationToken ct = default);

    /// <summary>Default (or first active) warehouse id for the site, or null if none.</summary>
    Task<int?> GetDefaultWarehouseIdAsync(int websiteId, CancellationToken ct = default);

    /// <summary>
    /// Available warehouse units = OnHand − Reserved.
    /// When WMS is not used for the site, returns int.MaxValue/4 (no warehouse gate).
    /// </summary>
    Task<int> GetAvailableAsync(int warehouseId, int productVariantId, CancellationToken ct = default);

    /// <summary>Reserve warehouse stock at checkout. Fails when available &lt; qty.</summary>
    Task<bool> ReserveAsync(int warehouseId, int productVariantId, int qty, CancellationToken ct = default);

    /// <summary>Release a previous warehouse reservation (cancel / unpaid).</summary>
    Task ReleaseReservationAsync(int warehouseId, int productVariantId, int qty, CancellationToken ct = default);

    /// <summary>
    /// Sum of on-hand across all warehouses for a website + variant (physical book).
    /// Used to keep InventoryItem in sync when WMS is active.
    /// </summary>
    Task<int> SumOnHandForVariantAsync(int websiteId, int productVariantId, CancellationToken ct = default);
}
