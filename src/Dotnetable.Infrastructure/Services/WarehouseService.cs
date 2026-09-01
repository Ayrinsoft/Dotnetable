using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class WarehouseService : IWarehouseService
{
    // Prefer ambient UoW context when OrderService has an open multi-service transaction.
    private readonly AppDbContext _fallback;
    private AppDbContext _context => AmbientDbContext.Current ?? _fallback;

    public WarehouseService(AppDbContext context) => _fallback = context;

    public async Task EnsureDefaultAsync(int websiteId, CancellationToken ct = default)
    {
        if (await _context.Warehouses.AnyAsync(w => w.WebsiteID == websiteId, ct)) return;
        _context.Warehouses.Add(new Warehouse
        {
            WebsiteID = websiteId,
            Code = "MAIN",
            Name = "Main warehouse",
            IsDefault = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Warehouse>> GetAllAsync(int websiteId, CancellationToken ct = default)
    {
        await EnsureDefaultAsync(websiteId, ct);
        return await _context.Warehouses.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId)
            .OrderByDescending(w => w.IsDefault).ThenBy(w => w.Name)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<Warehouse>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
        await EnsureDefaultAsync(websiteId, ct);

        var q = _context.Warehouses.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId);

        if (query.GetSearch(nameof(Warehouse.Code)) is string code)
            q = q.Where(w => w.Code.Contains(code));
        if (query.GetSearch(nameof(Warehouse.Name)) is string name)
            q = q.Where(w => w.Name.Contains(name));
        if (query.GetSearch(nameof(Warehouse.IsDefault)) is string def && bool.TryParse(def, out var isDefault))
            q = q.Where(w => w.IsDefault == isDefault);
        if (query.GetSearch(nameof(Warehouse.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(w => w.IsActive == isActive);

        var total = await q.CountAsync(ct);
        var ordered = string.IsNullOrWhiteSpace(query.OrderBy)
            ? q.OrderByDescending(w => w.IsDefault).ThenBy(w => w.Name)
            : q.ApplyOrderBy(query.OrderBy, nameof(Warehouse.Name));
        var items = await ordered.Skip(query.Skip).Take(query.Take).ToListAsync(ct);

        return new PagedResult<Warehouse> { Items = items, TotalCount = total };
    }

    public async Task<Warehouse> UpsertAsync(Warehouse warehouse, CancellationToken ct = default)
    {
        warehouse.Code = warehouse.Code.Trim();
        warehouse.Name = warehouse.Name.Trim();
        warehouse.Address = string.IsNullOrWhiteSpace(warehouse.Address) ? null : warehouse.Address.Trim();

        var codeTaken = await _context.Warehouses.AnyAsync(w =>
            w.WebsiteID == warehouse.WebsiteID
            && w.WarehouseID != warehouse.WarehouseID
            && w.Code == warehouse.Code, ct);
        if (codeTaken)
            throw new InvalidOperationException("A warehouse with this code already exists on the site.");

        if (warehouse.IsDefault)
        {
            var others = await _context.Warehouses
                .Where(w => w.WebsiteID == warehouse.WebsiteID && w.WarehouseID != warehouse.WarehouseID && w.IsDefault)
                .ToListAsync(ct);
            foreach (var o in others) o.IsDefault = false;
        }

        if (warehouse.WarehouseID == 0)
        {
            warehouse.CreatedAt = DateTime.UtcNow;
            _context.Warehouses.Add(warehouse);
        }
        else
        {
            var existing = await _context.Warehouses.FirstAsync(w => w.WarehouseID == warehouse.WarehouseID, ct);
            existing.Code = warehouse.Code;
            existing.Name = warehouse.Name;
            existing.Address = warehouse.Address;
            existing.IsDefault = warehouse.IsDefault;
            existing.IsActive = warehouse.IsActive;
            warehouse = existing;
        }
        await _context.SaveChangesAsync(ct);
        return warehouse;
    }

    public async Task<IReadOnlyList<WarehouseStock>> GetStockAsync(int warehouseId, CancellationToken ct = default) =>
        await _context.WarehouseStocks.AsNoTracking()
            .Include(s => s.ProductVariant).ThenInclude(v => v.Product)
            .Where(s => s.WarehouseID == warehouseId)
            .OrderBy(s => s.ProductVariant.Sku)
            .ToListAsync(ct);

    public async Task<int?> GetDefaultWarehouseIdAsync(int websiteId, CancellationToken ct = default)
    {
        var hasAny = await _context.Warehouses.AsNoTracking()
            .AnyAsync(w => w.WebsiteID == websiteId && w.IsActive, ct);
        if (!hasAny) return null;

        await EnsureDefaultAsync(websiteId, ct);
        return await _context.Warehouses.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId && w.IsActive)
            .OrderByDescending(w => w.IsDefault)
            .Select(w => (int?)w.WarehouseID)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> GetAvailableAsync(int warehouseId, int productVariantId, CancellationToken ct = default)
    {
        var stock = await _context.WarehouseStocks.AsNoTracking()
            .FirstOrDefaultAsync(s => s.WarehouseID == warehouseId && s.ProductVariantID == productVariantId, ct);
        if (stock is null) return 0;
        return Math.Max(0, stock.QuantityOnHand - stock.QuantityReserved);
    }

    // Atomic conditional UPDATE — see the note on InventoryService.ReserveAsync.
    public async Task<bool> ReserveAsync(int warehouseId, int productVariantId, int qty, CancellationToken ct = default)
    {
        if (qty <= 0) return true;

        var affected = await _context.WarehouseStocks
            .Where(s => s.WarehouseID == warehouseId
                        && s.ProductVariantID == productVariantId
                        && s.QuantityOnHand - s.QuantityReserved >= qty)
            .ExecuteUpdateAsync(u => u.SetProperty(s => s.QuantityReserved, s => s.QuantityReserved + qty), ct);

        if (affected > 0) await RefreshTrackedAsync(warehouseId, productVariantId, ct);
        return affected > 0;
    }

    public async Task ReleaseReservationAsync(int warehouseId, int productVariantId, int qty, CancellationToken ct = default)
    {
        if (qty <= 0) return;

        await _context.WarehouseStocks
            .Where(s => s.WarehouseID == warehouseId && s.ProductVariantID == productVariantId)
            .ExecuteUpdateAsync(u => u.SetProperty(
                s => s.QuantityReserved,
                s => s.QuantityReserved > qty ? s.QuantityReserved - qty : 0), ct);

        await RefreshTrackedAsync(warehouseId, productVariantId, ct);
    }

    /// <summary>
    /// ExecuteUpdate bypasses the change tracker, so a WarehouseStock this context already loaded
    /// would keep serving the pre-reservation counter. See InventoryService.RefreshTrackedAsync.
    /// </summary>
    private async Task RefreshTrackedAsync(int warehouseId, int productVariantId, CancellationToken ct)
    {
        var tracked = _context.ChangeTracker.Entries<WarehouseStock>()
            .FirstOrDefault(e => e.Entity.WarehouseID == warehouseId && e.Entity.ProductVariantID == productVariantId);

        if (tracked is not null)
            await tracked.ReloadAsync(ct);
    }

    public async Task<int> SumOnHandForVariantAsync(int websiteId, int productVariantId, CancellationToken ct = default)
    {
        return await _context.WarehouseStocks.AsNoTracking()
            .Where(s => s.Warehouse.WebsiteID == websiteId && s.ProductVariantID == productVariantId)
            .SumAsync(s => (int?)s.QuantityOnHand, ct) ?? 0;
    }
}
