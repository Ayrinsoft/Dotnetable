using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class WarehouseService : IWarehouseService
{
    private readonly AppDbContext _context;
    public WarehouseService(AppDbContext context) => _context = context;

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
}
