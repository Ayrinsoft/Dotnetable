using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
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

    public async Task<Warehouse> UpsertAsync(Warehouse warehouse, CancellationToken ct = default)
    {
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

    public async Task<bool> ReserveAsync(int warehouseId, int productVariantId, int qty, CancellationToken ct = default)
    {
        if (qty <= 0) return true;
        for (var attempt = 0; attempt < 2; attempt++)
        {
            var stock = await _context.WarehouseStocks
                .FirstOrDefaultAsync(s => s.WarehouseID == warehouseId && s.ProductVariantID == productVariantId, ct);
            if (stock is null) return false;

            var available = stock.QuantityOnHand - stock.QuantityReserved;
            if (available < qty) return false;

            stock.QuantityReserved += qty;
            try
            {
                await _context.SaveChangesAsync(ct);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                foreach (var entry in _context.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged))
                    entry.State = EntityState.Detached;
                if (attempt == 1) return false;
            }
        }
        return false;
    }

    public async Task ReleaseReservationAsync(int warehouseId, int productVariantId, int qty, CancellationToken ct = default)
    {
        if (qty <= 0) return;
        for (var attempt = 0; attempt < 2; attempt++)
        {
            var stock = await _context.WarehouseStocks
                .FirstOrDefaultAsync(s => s.WarehouseID == warehouseId && s.ProductVariantID == productVariantId, ct);
            if (stock is null) return;

            stock.QuantityReserved = Math.Max(0, stock.QuantityReserved - qty);
            try
            {
                await _context.SaveChangesAsync(ct);
                return;
            }
            catch (DbUpdateConcurrencyException)
            {
                foreach (var entry in _context.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged))
                    entry.State = EntityState.Detached;
                if (attempt == 1) return;
            }
        }
    }

    public async Task<int> SumOnHandForVariantAsync(int websiteId, int productVariantId, CancellationToken ct = default)
    {
        return await _context.WarehouseStocks.AsNoTracking()
            .Where(s => s.Warehouse.WebsiteID == websiteId && s.ProductVariantID == productVariantId)
            .SumAsync(s => (int?)s.QuantityOnHand, ct) ?? 0;
    }
}
