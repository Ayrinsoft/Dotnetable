using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class InventoryService : IInventoryService
{
    // Prefer ambient UoW context when OrderService (etc.) has an open multi-service transaction.
    private readonly AppDbContext _fallback;
    private AppDbContext _context => AmbientDbContext.Current ?? _fallback;
    private readonly ICurrencyConversionService _currency;

    public InventoryService(AppDbContext context, ICurrencyConversionService currency)
    {
        _fallback = context;
        _currency = currency;
    }

    public async Task<StockAvailability> GetAvailabilityAsync(int websiteId, int variantId, CancellationToken ct = default)
    {
        // Sellable stock is the sum of store listings only (not a free-floating warehouse row).
        var rows = await _context.VendorProducts.AsNoTracking()
            .Where(vp => vp.WebsiteID == websiteId && vp.ProductVariantID == variantId && vp.IsActive)
            .Select(vp => new { vp.StockQuantity, vp.QuantityReserved })
            .ToListAsync(ct);
        if (rows.Count == 0)
            return new StockAvailability(0, 0, 0);

        if (rows.Any(r => r.StockQuantity < 0))
            return new StockAvailability(int.MaxValue / 4, 0, int.MaxValue / 4);

        var onHand = rows.Sum(r => Math.Max(0, r.StockQuantity));
        var reserved = rows.Sum(r => Math.Max(0, r.QuantityReserved));
        return new StockAvailability(onHand, reserved, Math.Max(0, onHand - reserved));
    }

    public async Task<Dictionary<int, StockAvailability>> GetAvailabilityBulkAsync(int websiteId, IEnumerable<int> variantIds, CancellationToken ct = default)
    {
        var ids = variantIds.Distinct().ToList();
        var result = ids.ToDictionary(id => id, _ => new StockAvailability(0, 0, 0));
        if (ids.Count == 0) return result;

        var raw = await _context.VendorProducts.AsNoTracking()
            .Where(vp => vp.WebsiteID == websiteId && ids.Contains(vp.ProductVariantID) && vp.IsActive)
            .Select(vp => new { vp.ProductVariantID, vp.StockQuantity, vp.QuantityReserved })
            .ToListAsync(ct);

        foreach (var g in raw.GroupBy(x => x.ProductVariantID))
        {
            if (g.Any(x => x.StockQuantity < 0))
            {
                result[g.Key] = new StockAvailability(int.MaxValue / 4, 0, int.MaxValue / 4);
                continue;
            }
            var onHand = g.Sum(x => Math.Max(0, x.StockQuantity));
            var reserved = g.Sum(x => Math.Max(0, x.QuantityReserved));
            result[g.Key] = new StockAvailability(onHand, reserved, Math.Max(0, onHand - reserved));
        }
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
            var (currencyCode, rate) = await ResolveSiteRateAsync(websiteId, ct);

            item.QuantityOnHand -= qty;
            item.QuantityReserved = Math.Max(0, item.QuantityReserved - qty);

            var unitCost = item.AvgCost > 0 ? item.AvgCost : item.AvgCostUsd * rate;
            var unitCostUsd = item.AvgCostUsd > 0 ? item.AvgCostUsd : (rate == 0 ? 0 : unitCost / rate);

            _context.StockMovements.Add(new StockMovement
            {
                WebsiteID = websiteId,
                ProductVariantID = variantId,
                Type = (byte)StockMovementType.Sale,
                Quantity = -qty,
                UnitCost = unitCost,
                UnitCostUsd = unitCostUsd,
                CurrencyCode = currencyCode,
                ExchangeRateToUsd = rate,
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

    public async Task AdjustAsync(int websiteId, int variantId, int delta, decimal? unitCost, string? note, int memberId, CancellationToken ct = default)
    {
        for (var attempt = 0; attempt < 2; attempt++)
        {
            var item = await GetOrCreateAsync(websiteId, variantId, ct);
            var (currencyCode, rate) = await ResolveSiteRateAsync(websiteId, ct);

            decimal? unitCostUsd = null;
            decimal? unitCostLocal = unitCost;
            if (unitCost is decimal cost)
            {
                unitCostUsd = rate == 0 ? cost : cost / rate;
                if (delta > 0)
                {
                    var prevLocal = item.AvgCost > 0 ? item.AvgCost : item.AvgCostUsd * rate;
                    item.AvgCost = item.QuantityOnHand + delta > 0
                        ? ((prevLocal * item.QuantityOnHand) + (cost * delta)) / (item.QuantityOnHand + delta)
                        : cost;
                    item.AvgCostUsd = rate == 0 ? item.AvgCost : item.AvgCost / rate;
                }
            }

            item.QuantityOnHand += delta;

            var movementCost = unitCostLocal ?? (item.AvgCost > 0 ? item.AvgCost : item.AvgCostUsd * rate);
            var movementCostUsd = unitCostUsd ?? (item.AvgCostUsd > 0 ? item.AvgCostUsd : (rate == 0 ? movementCost : movementCost / rate));

            _context.StockMovements.Add(new StockMovement
            {
                WebsiteID = websiteId,
                ProductVariantID = variantId,
                Type = (byte)StockMovementType.Adjustment,
                Quantity = delta,
                UnitCost = movementCost,
                UnitCostUsd = movementCostUsd,
                CurrencyCode = currencyCode,
                ExchangeRateToUsd = rate,
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

    public async Task SetOnHandAsync(int websiteId, int variantId, int quantityOnHand, string? note, int memberId, CancellationToken ct = default)
    {
        if (quantityOnHand < 0)
            throw new ArgumentOutOfRangeException(nameof(quantityOnHand), "Stock quantity cannot be negative.");

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var item = await GetOrCreateAsync(websiteId, variantId, ct);

            if (quantityOnHand < item.QuantityReserved)
                throw new InvalidOperationException(
                    $"Cannot set on-hand stock ({quantityOnHand}) below reserved quantity ({item.QuantityReserved}).");

            var delta = quantityOnHand - item.QuantityOnHand;
            if (delta == 0)
            {
                if (_context.Entry(item).State == EntityState.Added)
                    await _context.SaveChangesAsync(ct);
                return;
            }

            item.QuantityOnHand = quantityOnHand;
            var (currencyCode, rate) = await ResolveSiteRateAsync(websiteId, ct);
            var unitCost = item.AvgCost > 0 ? item.AvgCost : item.AvgCostUsd * rate;
            var unitCostUsd = item.AvgCostUsd > 0 ? item.AvgCostUsd : (rate == 0 ? unitCost : unitCost / rate);

            _context.StockMovements.Add(new StockMovement
            {
                WebsiteID = websiteId,
                ProductVariantID = variantId,
                Type = (byte)StockMovementType.Adjustment,
                Quantity = delta,
                UnitCost = unitCost,
                UnitCostUsd = unitCostUsd,
                CurrencyCode = currencyCode,
                ExchangeRateToUsd = rate,
                Note = note ?? "Product stock set",
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

    public async Task SyncOnHandFromVendorListingsAsync(int websiteId, int productVariantId, CancellationToken ct = default)
    {
        var listings = await _context.VendorProducts.AsNoTracking()
            .Where(vp => vp.WebsiteID == websiteId && vp.ProductVariantID == productVariantId)
            .Select(vp => vp.StockQuantity)
            .ToListAsync(ct);

        var sumOnHand = listings.Sum(q => Math.Max(0, q));
        var item = await GetOrCreateAsync(websiteId, productVariantId, ct);
        // Store listings are the only source of on-hand; no listings ⇒ on-hand collapses to reserved floor.
        item.QuantityOnHand = Math.Max(sumOnHand, item.QuantityReserved);
        await _context.SaveChangesAsync(ct);
    }

    public async Task RestockReturnAsync(int websiteId, int variantId, int qty, decimal? unitCost, string? note, int memberId, CancellationToken ct = default)
    {
        if (qty <= 0) return;

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var item = await GetOrCreateAsync(websiteId, variantId, ct);
            var (currencyCode, rate) = await ResolveSiteRateAsync(websiteId, ct);

            decimal? unitCostUsd = null;
            decimal costLocal = unitCost ?? (item.AvgCost > 0 ? item.AvgCost : item.AvgCostUsd * rate);
            if (unitCost is decimal cost)
            {
                unitCostUsd = rate == 0 ? cost : cost / rate;
                var prevLocal = item.AvgCost > 0 ? item.AvgCost : item.AvgCostUsd * rate;
                item.AvgCost = item.QuantityOnHand + qty > 0
                    ? ((prevLocal * item.QuantityOnHand) + (cost * qty)) / (item.QuantityOnHand + qty)
                    : cost;
                item.AvgCostUsd = rate == 0 ? item.AvgCost : item.AvgCost / rate;
            }

            item.QuantityOnHand += qty;
            var movementCostUsd = unitCostUsd ?? (item.AvgCostUsd > 0 ? item.AvgCostUsd : (rate == 0 ? costLocal : costLocal / rate));

            _context.StockMovements.Add(new StockMovement
            {
                WebsiteID = websiteId,
                ProductVariantID = variantId,
                Type = (byte)StockMovementType.Return,
                Quantity = qty,
                UnitCost = costLocal,
                UnitCostUsd = movementCostUsd,
                CurrencyCode = currencyCode,
                ExchangeRateToUsd = rate,
                Note = note ?? "Customer return",
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

    public async Task SyncOnHandFromWarehousesAsync(int websiteId, int productVariantId, int warehouseOnHandSum, CancellationToken ct = default)
    {
        var item = await GetOrCreateAsync(websiteId, productVariantId, ct);
        // Physical warehouse book is authoritative when WMS is used; never drop below reserved.
        item.QuantityOnHand = Math.Max(Math.Max(0, warehouseOnHandSum), item.QuantityReserved);
        await _context.SaveChangesAsync(ct);
    }

    private async Task<(string CurrencyCode, decimal Rate)> ResolveSiteRateAsync(int websiteId, CancellationToken ct)
    {
        try
        {
            return await _currency.GetActiveRateAsync(websiteId, null, ct);
        }
        catch (InvalidOperationException)
        {
            return ("USD", 1m);
        }
    }

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
            AvgCost = 0,
            AvgCostUsd = 0,
            RowVersion = new byte[8],
        };
        _context.InventoryItems.Add(item);
        return item;
    }

    private static StockAvailability ToAvailability(InventoryItem? item) => item is null
        ? new StockAvailability(0, 0, 0)
        : new StockAvailability(item.QuantityOnHand, item.QuantityReserved, item.QuantityOnHand - item.QuantityReserved);
}
