using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _context;

    public InventoryService(AppDbContext context) => _context = context;

    public async Task<StockAvailability> GetAvailabilityAsync(int websiteId, int variantId, CancellationToken ct = default)
    {
        var item = await _context.InventoryItems.AsNoTracking()
            .FirstOrDefaultAsync(i => i.WebsiteID == websiteId && i.ProductVariantID == variantId, ct);
        return ToAvailability(item);
    }

    public async Task<Dictionary<int, StockAvailability>> GetAvailabilityBulkAsync(int websiteId, IEnumerable<int> variantIds, CancellationToken ct = default)
    {
        var ids = variantIds.Distinct().ToList();
        var items = await _context.InventoryItems.AsNoTracking()
            .Where(i => i.WebsiteID == websiteId && ids.Contains(i.ProductVariantID))
            .ToListAsync(ct);

        var result = ids.ToDictionary(id => id, _ => new StockAvailability(0, 0, 0));
        foreach (var item in items)
            result[item.ProductVariantID] = ToAvailability(item);
        return result;
    }

    public async Task<bool> ReserveAsync(int websiteId, int variantId, int qty, CancellationToken ct = default)
    {
        for (var attempt = 0; attempt < 2; attempt++)
        {
            var item = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.WebsiteID == websiteId && i.ProductVariantID == variantId, ct);
            if (item is null) return false;

            var available = item.QuantityOnHand - item.QuantityReserved;
            if (available < qty) return false;

            item.QuantityReserved += qty;
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

    public async Task ReleaseReservationAsync(int websiteId, int variantId, int qty, CancellationToken ct = default)
    {
        for (var attempt = 0; attempt < 2; attempt++)
        {
            var item = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.WebsiteID == websiteId && i.ProductVariantID == variantId, ct);
            if (item is null) return;

            item.QuantityReserved = Math.Max(0, item.QuantityReserved - qty);
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

    public async Task DecrementOnFulfillAsync(int websiteId, int variantId, int qty, int? orderId, int? orderItemId, int? memberId, CancellationToken ct = default)
    {
        for (var attempt = 0; attempt < 2; attempt++)
        {
            var item = await GetOrCreateAsync(websiteId, variantId, ct);

            item.QuantityOnHand -= qty;
            item.QuantityReserved = Math.Max(0, item.QuantityReserved - qty);

            _context.StockMovements.Add(new StockMovement
            {
                WebsiteID = websiteId,
                ProductVariantID = variantId,
                Type = (byte)StockMovementType.Sale,
                Quantity = -qty,
                UnitCostUsd = item.AvgCostUsd,
                CurrencyCode = "USD",
                ExchangeRateToUsd = 1m,
                OrderID = orderId,
                OrderItemID = orderItemId,
                CreatedByMemberID = memberId,
                CreatedAt = DateTime.UtcNow,
            });

            try
            {
                await _context.SaveChangesAsync(ct);
                return;
            }
            catch (DbUpdateConcurrencyException)
            {
                foreach (var entry in _context.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged))
                    entry.State = EntityState.Detached;
                if (attempt == 1) throw;
            }
        }
    }

    public async Task AdjustAsync(int websiteId, int variantId, int delta, decimal? unitCostUsd, string? note, int memberId, CancellationToken ct = default)
    {
        for (var attempt = 0; attempt < 2; attempt++)
        {
            var item = await GetOrCreateAsync(websiteId, variantId, ct);

            item.QuantityOnHand += delta;
            if (unitCostUsd is decimal cost && delta > 0)
                item.AvgCostUsd = item.QuantityOnHand > 0
                    ? ((item.AvgCostUsd * (item.QuantityOnHand - delta)) + (cost * delta)) / item.QuantityOnHand
                    : cost;

            _context.StockMovements.Add(new StockMovement
            {
                WebsiteID = websiteId,
                ProductVariantID = variantId,
                Type = (byte)StockMovementType.Adjustment,
                Quantity = delta,
                UnitCostUsd = unitCostUsd ?? item.AvgCostUsd,
                CurrencyCode = "USD",
                ExchangeRateToUsd = 1m,
                Note = note,
                CreatedByMemberID = memberId,
                CreatedAt = DateTime.UtcNow,
            });

            try
            {
                await _context.SaveChangesAsync(ct);
                return;
            }
            catch (DbUpdateConcurrencyException)
            {
                foreach (var entry in _context.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged))
                    entry.State = EntityState.Detached;
                if (attempt == 1) throw;
            }
        }
    }

    public async Task<PagedResult<InventoryItem>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.InventoryItems.AsNoTracking()
            .Include(i => i.ProductVariant).ThenInclude(v => v.Product)
            .Where(i => i.WebsiteID == websiteId);

        if (query.GetSearch(nameof(ProductVariant.Sku)) is string sku)
            q = q.Where(i => i.ProductVariant.Sku.Contains(sku));
        if (query.GetSearch("Title") is string title)
            q = q.Where(i => i.ProductVariant.Product.Title.Contains(title));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(InventoryItem.InventoryItemID))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<InventoryItem> { Items = items, TotalCount = total };
    }

    public async Task<List<InventoryItem>> GetLowStockAsync(int websiteId, CancellationToken ct = default) =>
        await _context.InventoryItems.AsNoTracking()
            .Include(i => i.ProductVariant).ThenInclude(v => v.Product)
            .Where(i => i.WebsiteID == websiteId && i.QuantityOnHand <= i.ReorderLevel)
            .OrderBy(i => i.QuantityOnHand)
            .ToListAsync(ct);

    private async Task<InventoryItem> GetOrCreateAsync(int websiteId, int variantId, CancellationToken ct)
    {
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.WebsiteID == websiteId && i.ProductVariantID == variantId, ct);
        if (item is not null) return item;

        item = new InventoryItem
        {
            WebsiteID = websiteId,
            ProductVariantID = variantId,
            QuantityOnHand = 0,
            QuantityReserved = 0,
            ReorderLevel = 0,
            AvgCostUsd = 0,
        };
        _context.InventoryItems.Add(item);
        return item;
    }

    private static StockAvailability ToAvailability(InventoryItem? item) => item is null
        ? new StockAvailability(0, 0, 0)
        : new StockAvailability(item.QuantityOnHand, item.QuantityReserved, item.QuantityOnHand - item.QuantityReserved);
}
