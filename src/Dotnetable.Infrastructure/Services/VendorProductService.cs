using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class VendorProductService : IVendorProductService
{
    private readonly AppDbContext _context;

    public VendorProductService(AppDbContext context) => _context = context;

    public async Task<PagedResult<VendorProductListItemDto>> GetPagedAsync(int vendorId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.VendorProducts.AsNoTracking()
            .Where(vp => vp.VendorID == vendorId)
            .Include(vp => vp.Vendor)
            .Include(vp => vp.ProductVariant).ThenInclude(v => v.Product)
            .AsQueryable();

        if (query.GetSearch("Sku") is string sku)
            q = q.Where(vp => vp.ProductVariant.Sku.Contains(sku));
        if (query.GetSearch("ProductTitle") is string title)
            q = q.Where(vp => vp.ProductVariant.Product.Title.Contains(title)
                              || vp.ProductVariant.Title.Contains(title));
        if (query.GetSearch("VariantTitle") is string vTitle)
            q = q.Where(vp => vp.ProductVariant.Title.Contains(vTitle));
        if (query.GetSearch(nameof(VendorProduct.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(vp => vp.IsActive == isActive);

        // Single free-text box from list page (key "q") matches product, variant title, or SKU.
        if (query.GetSearch("q") is string free && !string.IsNullOrWhiteSpace(free))
        {
            var s = free.Trim();
            q = q.Where(vp =>
                vp.ProductVariant.Product.Title.Contains(s)
                || vp.ProductVariant.Title.Contains(s)
                || vp.ProductVariant.Sku.Contains(s));
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(vp => vp.ProductVariant.Product.Title)
            .ThenBy(vp => vp.ProductVariant.Title)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<VendorProductListItemDto>
        {
            TotalCount = total,
            Items = items.Select(MapListItem).ToList(),
        };
    }

    public async Task<List<VendorProductListItemDto>> GetByProductIdAsync(int productId, CancellationToken ct = default)
    {
        var items = await _context.VendorProducts.AsNoTracking()
            .Where(vp => vp.ProductVariant.ProductID == productId)
            .Include(vp => vp.Vendor)
            .Include(vp => vp.ProductVariant).ThenInclude(v => v.Product)
            .OrderBy(vp => vp.Vendor.Name)
            .ThenBy(vp => vp.ProductVariant.Title)
            .ThenBy(vp => vp.ProductVariant.Sku)
            .ToListAsync(ct);

        return items.Select(MapListItem).ToList();
    }

    private static VendorProductListItemDto MapListItem(VendorProduct vp) => new()
    {
        VendorProductID = vp.VendorProductID,
        VendorID = vp.VendorID,
        VendorName = vp.Vendor?.Name ?? string.Empty,
        VendorType = vp.Vendor?.VendorType ?? 0,
        ProductVariantID = vp.ProductVariantID,
        ProductID = vp.ProductVariant.ProductID,
        ProductTitle = vp.ProductVariant.Product.Title,
        VariantTitle = vp.ProductVariant.Title,
        Sku = vp.ProductVariant.Sku,
        ReferencePrice = vp.ReferencePrice > 0 ? vp.ReferencePrice : vp.ReferencePriceUsd,
        ReferencePriceUsd = vp.ReferencePriceUsd,
        OverridePriceLocal = vp.OverridePriceLocal,
        OverridePrice = vp.OverridePrice,
        StockQuantity = vp.StockQuantity,
        QuantityReserved = vp.QuantityReserved,
        AvailableQuantity = IVendorProductService.Available(vp),
        DeliveryDays = vp.DeliveryDays,
        IsActive = vp.IsActive,
    };

    public async Task<List<VendorVariantPickDto>> SearchEligibleVariantsAsync(
        int vendorId, string? search, int take = 25, bool includeAlreadyListed = false, CancellationToken ct = default)
    {
        var vendor = await _context.Vendors.AsNoTracking()
            .FirstOrDefaultAsync(v => v.VendorID == vendorId, ct);
        if (vendor is null) return new List<VendorVariantPickDto>();

        // Site vendors list products owned by the linked website; others list host-website products.
        var catalogWebsiteId = vendor.VendorType == (byte)VendorType.Site && vendor.LinkedWebsiteID is int lid
            ? lid
            : vendor.WebsiteID;

        var listedIds = await _context.VendorProducts.AsNoTracking()
            .Where(vp => vp.VendorID == vendorId)
            .Select(vp => vp.ProductVariantID)
            .ToListAsync(ct);
        var listed = listedIds.ToHashSet();

        var q = _context.ProductVariants.AsNoTracking()
            .Where(v => v.IsActive
                        && v.Product.IsActive
                        && v.Product.WebsiteID == catalogWebsiteId);

        // Member sellers only pick variants from products they created (their catalog).
        if (vendor.VendorType == (byte)VendorType.Member && vendor.MemberID is int ownerId)
            q = q.Where(v => v.Product.CreatedByMemberID == ownerId);

        if (!includeAlreadyListed && listed.Count > 0)
            q = q.Where(v => !listed.Contains(v.ProductVariantID));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(v =>
                v.Sku.Contains(s)
                || v.Title.Contains(s)
                || v.Product.Title.Contains(s)
                || v.Product.Slug.Contains(s));
        }

        take = Math.Clamp(take, 1, 50);
        var rows = await q
            .OrderBy(v => v.Product.Title)
            .ThenBy(v => v.Title)
            .ThenBy(v => v.Sku)
            .Take(take)
            .Select(v => new
            {
                v.ProductVariantID,
                v.ProductID,
                ProductTitle = v.Product.Title,
                VariantTitle = v.Title,
                v.Sku,
                v.ReferencePrice,
                v.ReferencePriceUsd,
            })
            .ToListAsync(ct);

        return rows.Select(v => new VendorVariantPickDto
        {
            ProductVariantID = v.ProductVariantID,
            ProductID = v.ProductID,
            ProductTitle = v.ProductTitle,
            VariantTitle = v.VariantTitle ?? string.Empty,
            Sku = v.Sku,
            ReferencePrice = v.ReferencePrice > 0 ? v.ReferencePrice : v.ReferencePriceUsd,
            ReferencePriceUsd = v.ReferencePriceUsd,
            AlreadyListed = listed.Contains(v.ProductVariantID),
        }).ToList();
    }

    public async Task<VendorProduct?> GetByIdAsync(int vendorProductId, CancellationToken ct = default) =>
        await _context.VendorProducts
            .Include(vp => vp.Vendor)
            .Include(vp => vp.ProductVariant).ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(vp => vp.VendorProductID == vendorProductId, ct);

    public async Task<VendorProduct?> FindAsync(int vendorId, int productVariantId, CancellationToken ct = default) =>
        await _context.VendorProducts
            .FirstOrDefaultAsync(vp => vp.VendorID == vendorId && vp.ProductVariantID == productVariantId, ct);

    public async Task<(bool Success, string? Error, VendorProduct? Item)> UpsertAsync(
        VendorProduct model, int? actingMemberId = null, CancellationToken ct = default)
    {
        var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.VendorID == model.VendorID, ct);
        if (vendor is null) return (false, "Vendor not found.", null);

        if (actingMemberId is int mid)
        {
            if (vendor.VendorType != (byte)VendorType.Member || vendor.MemberID != mid)
                return (false, "You can only manage listings for your own vendor account.", null);
        }

        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.ProductVariantID == model.ProductVariantID, ct);
        if (variant is null) return (false, "Product variant not found.", null);

        var ownershipError = ValidateOwnership(vendor, variant);
        if (ownershipError is not null) return (false, ownershipError, null);

        VendorProduct entity;
        if (model.VendorProductID > 0)
        {
            entity = await _context.VendorProducts.FirstOrDefaultAsync(vp => vp.VendorProductID == model.VendorProductID, ct)
                     ?? throw new InvalidOperationException("Vendor product not found.");
            if (entity.VendorID != model.VendorID)
                return (false, "Vendor product mismatch.", null);
        }
        else
        {
            var existing = await FindAsync(model.VendorID, model.ProductVariantID, ct);
            if (existing is not null)
            {
                entity = existing;
            }
            else
            {
                entity = new VendorProduct
                {
                    WebsiteID = vendor.WebsiteID,
                    VendorID = vendor.VendorID,
                    ProductVariantID = model.ProductVariantID,
                };
                _context.VendorProducts.Add(entity);
            }
        }

        // Site-currency listing price is authority; USD dual kept for conversion bridge.
        entity.ReferencePrice = model.ReferencePrice > 0
            ? model.ReferencePrice
            : (variant.ReferencePrice > 0 ? variant.ReferencePrice : model.ReferencePriceUsd);
        entity.OverridePriceLocal = model.OverridePriceLocal;
        entity.ReferencePriceUsd = model.ReferencePriceUsd > 0
            ? model.ReferencePriceUsd
            : (variant.ReferencePriceUsd > 0 ? variant.ReferencePriceUsd : entity.ReferencePrice);
        entity.OverridePrice = model.OverridePrice;
        // -1 = unlimited (digital only). Physical listings clamp to ≥ 0.
        var isDigital = variant.Product is not null
            && (variant.Product.ProductType != (byte)ProductType.Physical || !variant.Product.RequiresShipping);
        int desiredStock;
        if (model.StockQuantity < 0)
        {
            if (!isDigital)
                return (false, "Unlimited stock (-1) is only allowed for digital products.", null);
            desiredStock = -1;
        }
        else
        {
            desiredStock = Math.Max(0, model.StockQuantity);
            if (desiredStock < entity.QuantityReserved)
                return (false, $"Stock ({desiredStock}) cannot be below reserved quantity ({entity.QuantityReserved}).", null);
        }
        entity.StockQuantity = desiredStock;
        entity.DeliveryDays = model.DeliveryDays < 0 ? 0 : model.DeliveryDays;
        entity.IsActive = model.IsActive;
        entity.WebsiteID = vendor.WebsiteID;

        await _context.SaveChangesAsync(ct);
        // Inventory total for the host variant = sum of store listing stocks.
        await SyncInventoryOnHandFromListingsAsync(entity.WebsiteID, entity.ProductVariantID, ct);
        return (true, null, entity);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int vendorProductId, int? actingMemberId = null, CancellationToken ct = default)
    {
        var entity = await _context.VendorProducts
            .Include(vp => vp.Vendor)
            .FirstOrDefaultAsync(vp => vp.VendorProductID == vendorProductId, ct);
        if (entity is null) return (true, null);

        if (actingMemberId is int mid)
        {
            if (entity.Vendor.VendorType != (byte)VendorType.Member || entity.Vendor.MemberID != mid)
                return (false, "You can only manage listings for your own vendor account.");
        }

        if (entity.QuantityReserved > 0)
            return (false, "Cannot delete a listing with reserved stock from open orders.");

        var websiteId = entity.WebsiteID;
        var variantId = entity.ProductVariantID;
        _context.VendorProducts.Remove(entity);
        await _context.SaveChangesAsync(ct);
        await SyncInventoryOnHandFromListingsAsync(websiteId, variantId, ct);
        return (true, null);
    }

    public async Task<VendorProduct?> EnsureListingForVariantAsync(
        int hostWebsiteId, int vendorId, int productVariantId, CancellationToken ct = default)
    {
        var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.VendorID == vendorId && v.WebsiteID == hostWebsiteId, ct);
        if (vendor is null || !vendor.IsActive) return null;

        var existing = await FindAsync(vendorId, productVariantId, ct);
        if (existing is not null) return existing;

        var variant = await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.ProductVariantID == productVariantId, ct);
        if (variant is null) return null;
        if (ValidateOwnership(vendor, variant) is not null) return null;

        // Seed from source-site store listings only (never raw warehouse). Zero ⇒ not sellable until stock is set.
        var sourceStock = await _context.VendorProducts.AsNoTracking()
            .Where(vp => vp.ProductVariantID == productVariantId
                         && vp.WebsiteID == variant.WebsiteID
                         && vp.IsActive)
            .Select(vp => vp.StockQuantity - vp.QuantityReserved)
            .SumAsync(ct);

        var listing = new VendorProduct
        {
            WebsiteID = hostWebsiteId,
            VendorID = vendorId,
            ProductVariantID = productVariantId,
            // Per-seller listing price starts from the catalog variant; sellers can override later.
            ReferencePrice = variant.ReferencePrice > 0 ? variant.ReferencePrice : variant.ReferencePriceUsd,
            ReferencePriceUsd = variant.ReferencePriceUsd > 0 ? variant.ReferencePriceUsd : variant.ReferencePrice,
            OverridePriceLocal = null,
            OverridePrice = null,
            StockQuantity = Math.Max(0, sourceStock),
            QuantityReserved = 0,
            DeliveryDays = 1,
            IsActive = true,
        };
        _context.VendorProducts.Add(listing);
        await _context.SaveChangesAsync(ct);
        await SyncInventoryOnHandFromListingsAsync(hostWebsiteId, productVariantId, ct);
        return listing;
    }

    public async Task<List<VendorProduct>> GetActiveByVendorAsync(int vendorId, CancellationToken ct = default) =>
        await _context.VendorProducts.AsNoTracking()
            .Where(vp => vp.VendorID == vendorId && vp.IsActive)
            .Include(vp => vp.ProductVariant).ThenInclude(v => v.Product)
            .ToListAsync(ct);

    public async Task<bool> ReserveAsync(int vendorProductId, int qty, CancellationToken ct = default)
    {
        if (qty <= 0) return true;
        var item = await _context.VendorProducts.FirstOrDefaultAsync(vp => vp.VendorProductID == vendorProductId, ct);
        if (item is null || !item.IsActive) return false;
        if (IVendorProductService.Available(item) < qty) return false;

        // Unlimited digital stock: no reservation bookkeeping (sales never deplete).
        if (IVendorProductService.IsUnlimited(item))
            return true;

        item.QuantityReserved += qty;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task ReleaseReservationAsync(int vendorProductId, int qty, CancellationToken ct = default)
    {
        if (qty <= 0) return;
        var item = await _context.VendorProducts.FirstOrDefaultAsync(vp => vp.VendorProductID == vendorProductId, ct);
        if (item is null || IVendorProductService.IsUnlimited(item)) return;

        item.QuantityReserved = Math.Max(0, item.QuantityReserved - qty);
        await _context.SaveChangesAsync(ct);
    }

    public async Task CommitSaleAsync(int vendorProductId, int qty, CancellationToken ct = default)
    {
        if (qty <= 0) return;
        var item = await _context.VendorProducts.FirstOrDefaultAsync(vp => vp.VendorProductID == vendorProductId, ct);
        if (item is null) return;

        // Unlimited digital: leave StockQuantity at -1 forever.
        if (IVendorProductService.IsUnlimited(item))
            return;

        item.StockQuantity = Math.Max(0, item.StockQuantity - qty);
        item.QuantityReserved = Math.Max(0, item.QuantityReserved - qty);
        await _context.SaveChangesAsync(ct);
        // Inventory on-hand is reduced via DecrementOnFulfill; re-sync after both complete in OrderService.
    }

    public async Task SyncInventoryOnHandFromListingsAsync(int websiteId, int productVariantId, CancellationToken ct = default)
    {
        var listings = await _context.VendorProducts
            .Where(vp => vp.WebsiteID == websiteId && vp.ProductVariantID == productVariantId)
            .ToListAsync(ct);

        // Source of truth for total inventory: sum of store listing on-hand quantities (0 when no stores).
        // Any unlimited (-1) listing marks inventory as a large sentinel so warehouse checks succeed.
        var hasUnlimited = listings.Any(vp => vp.StockQuantity < 0);
        var sumOnHand = hasUnlimited
            ? int.MaxValue / 4
            : listings.Sum(vp => Math.Max(0, vp.StockQuantity));

        var inv = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.WebsiteID == websiteId && i.ProductVariantID == productVariantId, ct);
        if (listings.Count == 0)
        {
            if (inv is null) return;
            // No store listings ⇒ nothing sellable; keep on-hand at reserved floor only.
            inv.QuantityOnHand = inv.QuantityReserved;
            await _context.SaveChangesAsync(ct);
            return;
        }

        if (inv is null)
        {
            inv = new InventoryItem
            {
                WebsiteID = websiteId,
                ProductVariantID = productVariantId,
                QuantityOnHand = sumOnHand,
                QuantityReserved = 0,
                ReorderLevel = 0,
                AvgCost = 0,
                AvgCostUsd = 0,
                RowVersion = new byte[8],
            };
            _context.InventoryItems.Add(inv);
        }
        else
        {
            // Never drop on-hand below open reservations held at inventory level.
            inv.QuantityOnHand = Math.Max(sumOnHand, inv.QuantityReserved);
        }

        await _context.SaveChangesAsync(ct);
    }

    // Stable English headers so re-import works regardless of UI language.
    private static readonly string[] ListingExcelHeaders =
    [
        "VendorProductID", "VendorID", "VendorName", "ProductID", "ProductTitle",
        "VariantTitle", "Sku", "Price", "Stock", "DeliveryDays", "IsActive", "Reserved",
    ];

    public async Task<byte[]> ExportListingsExcelAsync(
        int websiteId, int? vendorId = null, int? actingMemberId = null, CancellationToken ct = default)
    {
        var q = await BuildScopedListingsQueryAsync(websiteId, vendorId, actingMemberId, ct);
        if (q is null)
            return ExcelWorkbook.Write("Listings", ListingExcelHeaders, Array.Empty<IReadOnlyList<object?>>());

        var rows = await q
            .OrderBy(vp => vp.Vendor.Name)
            .ThenBy(vp => vp.ProductVariant.Product.Title)
            .ThenBy(vp => vp.ProductVariant.Title)
            .ThenBy(vp => vp.ProductVariant.Sku)
            .Select(vp => new
            {
                vp.VendorProductID,
                vp.VendorID,
                VendorName = vp.Vendor.Name,
                ProductID = vp.ProductVariant.ProductID,
                ProductTitle = vp.ProductVariant.Product.Title,
                VariantTitle = vp.ProductVariant.Title,
                Sku = vp.ProductVariant.Sku,
                Price = vp.ReferencePrice > 0 ? vp.ReferencePrice : vp.ReferencePriceUsd,
                vp.StockQuantity,
                vp.DeliveryDays,
                vp.IsActive,
                vp.QuantityReserved,
            })
            .ToListAsync(ct);

        return ExcelWorkbook.Write(
            "Listings",
            ListingExcelHeaders,
            rows.Select(r => (IReadOnlyList<object?>)
            [
                r.VendorProductID,
                r.VendorID,
                r.VendorName,
                r.ProductID,
                r.ProductTitle,
                r.VariantTitle,
                r.Sku,
                r.Price,
                r.StockQuantity,
                r.DeliveryDays,
                r.IsActive ? "1" : "0",
                r.QuantityReserved,
            ]));
    }

    public async Task<VendorListingImportResult> ImportListingsExcelAsync(
        int websiteId, Stream excel, int? vendorId = null, int? actingMemberId = null, CancellationToken ct = default)
    {
        var rows = ExcelWorkbook.Read(excel);
        if (rows.Count == 0)
            return new VendorListingImportResult(0, 0, 0, ["Excel file is empty."]);

        var headerMap = MapListingHeaders(rows[0]);
        if (!headerMap.TryGetValue("VendorProductID", out var idCol)
            || !headerMap.TryGetValue("Price", out var priceCol)
            || !headerMap.TryGetValue("Stock", out var stockCol))
        {
            return new VendorListingImportResult(0, 0, 0,
            [
                "Missing required columns. Export a fresh file first (needs VendorProductID, Price, Stock).",
            ]);
        }

        var deliveryCol = headerMap.TryGetValue("DeliveryDays", out var dCol) ? dCol : -1;
        var activeCol = headerMap.TryGetValue("IsActive", out var aCol) ? aCol : -1;

        int updated = 0, unchanged = 0, skipped = 0;
        var errors = new List<string>();
        var syncKeys = new HashSet<(int WebsiteId, int VariantId)>();

        // Preload listings in scope for fast lookup + security boundary.
        var scoped = await BuildScopedListingsQueryAsync(websiteId, vendorId, actingMemberId, ct);
        if (scoped is null)
            return new VendorListingImportResult(0, 0, 0, ["No vendor listings available for this scope."]);

        var entities = await scoped
            .Include(vp => vp.Vendor)
            .Include(vp => vp.ProductVariant).ThenInclude(v => v.Product)
            .ToDictionaryAsync(vp => vp.VendorProductID, ct);

        for (var i = 1; i < rows.Count; i++)
        {
            var row = rows[i];
            var line = i + 1;

            var idText = Cell(row, idCol);
            if (string.IsNullOrWhiteSpace(idText))
            {
                skipped++;
                continue;
            }

            if (!int.TryParse(idText, out var listingId) || listingId <= 0)
            {
                errors.Add($"Row {line}: invalid VendorProductID '{idText}'.");
                skipped++;
                continue;
            }

            if (!entities.TryGetValue(listingId, out var entity))
            {
                errors.Add($"Row {line}: listing #{listingId} not found in this website/seller scope.");
                skipped++;
                continue;
            }

            if (!TryParseDecimal(Cell(row, priceCol), out var price) || price < 0)
            {
                errors.Add($"Row {line}: invalid Price.");
                skipped++;
                continue;
            }

            if (!TryParseInt(Cell(row, stockCol), out var stock))
            {
                errors.Add($"Row {line}: invalid Stock (use whole numbers; -1 = unlimited digital).");
                skipped++;
                continue;
            }

            var deliveryDays = entity.DeliveryDays;
            if (deliveryCol >= 0)
            {
                var dText = Cell(row, deliveryCol);
                if (!string.IsNullOrWhiteSpace(dText))
                {
                    if (!TryParseInt(dText, out var d) || d < 0)
                    {
                        errors.Add($"Row {line}: invalid DeliveryDays.");
                        skipped++;
                        continue;
                    }
                    deliveryDays = d;
                }
            }

            var isActive = entity.IsActive;
            if (activeCol >= 0)
            {
                var aText = Cell(row, activeCol);
                if (!string.IsNullOrWhiteSpace(aText) && !TryParseBool(aText, out isActive))
                {
                    errors.Add($"Row {line}: invalid IsActive (use 1/0, true/false, yes/no).");
                    skipped++;
                    continue;
                }
            }

            // Digital-only unlimited stock.
            var isDigital = entity.ProductVariant.Product is not null
                && (entity.ProductVariant.Product.ProductType != (byte)ProductType.Physical
                    || !entity.ProductVariant.Product.RequiresShipping);
            int desiredStock;
            if (stock < 0)
            {
                if (!isDigital)
                {
                    errors.Add($"Row {line}: unlimited stock (-1) is only allowed for digital products (SKU {entity.ProductVariant.Sku}).");
                    skipped++;
                    continue;
                }
                desiredStock = -1;
            }
            else
            {
                desiredStock = stock;
                if (desiredStock < entity.QuantityReserved)
                {
                    errors.Add($"Row {line}: stock ({desiredStock}) cannot be below reserved ({entity.QuantityReserved}) for SKU {entity.ProductVariant.Sku}.");
                    skipped++;
                    continue;
                }
            }

            // Scale USD dual when local price changes so conversion bridge stays consistent.
            var oldLocal = entity.ReferencePrice > 0 ? entity.ReferencePrice : entity.ReferencePriceUsd;
            var newLocal = price;
            decimal newUsd = entity.ReferencePriceUsd;
            if (oldLocal > 0 && newLocal != oldLocal && entity.ReferencePriceUsd > 0)
                newUsd = Math.Round(entity.ReferencePriceUsd * (newLocal / oldLocal), 6, MidpointRounding.AwayFromZero);
            else if (newUsd <= 0)
                newUsd = newLocal;

            var noChange = Math.Abs(entity.ReferencePrice - newLocal) < 0.0000001m
                && entity.StockQuantity == desiredStock
                && entity.DeliveryDays == deliveryDays
                && entity.IsActive == isActive;

            if (noChange)
            {
                unchanged++;
                continue;
            }

            entity.ReferencePrice = newLocal;
            entity.ReferencePriceUsd = newUsd > 0 ? newUsd : newLocal;
            entity.StockQuantity = desiredStock;
            entity.DeliveryDays = deliveryDays;
            entity.IsActive = isActive;

            updated++;
            syncKeys.Add((entity.WebsiteID, entity.ProductVariantID));
        }

        if (updated > 0)
        {
            await _context.SaveChangesAsync(ct);
            foreach (var (wid, vid) in syncKeys)
                await SyncInventoryOnHandFromListingsAsync(wid, vid, ct);
        }

        return new VendorListingImportResult(updated, unchanged, skipped, errors);
    }

    private async Task<IQueryable<VendorProduct>?> BuildScopedListingsQueryAsync(
        int websiteId, int? vendorId, int? actingMemberId, CancellationToken ct)
    {
        if (websiteId <= 0) return null;

        IQueryable<VendorProduct> q = _context.VendorProducts
            .Where(vp => vp.WebsiteID == websiteId);

        if (actingMemberId is int mid)
        {
            // Seller: only their member-vendor shop(s) on this host.
            var myVendorIds = await _context.Vendors.AsNoTracking()
                .Where(v => v.WebsiteID == websiteId
                            && v.VendorType == (byte)VendorType.Member
                            && v.MemberID == mid)
                .Select(v => v.VendorID)
                .ToListAsync(ct);
            if (myVendorIds.Count == 0) return null;
            if (vendorId is int forced && !myVendorIds.Contains(forced))
                return null;
            var allowed = vendorId is int only && myVendorIds.Contains(only)
                ? new List<int> { only }
                : myVendorIds;
            q = q.Where(vp => allowed.Contains(vp.VendorID));
        }
        else if (vendorId is int vid)
        {
            q = q.Where(vp => vp.VendorID == vid);
        }

        return q
            .Include(vp => vp.Vendor)
            .Include(vp => vp.ProductVariant).ThenInclude(v => v.Product);
    }

    private static Dictionary<string, int> MapListingHeaders(string[] headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var c = 0; c < headerRow.Length; c++)
        {
            var name = headerRow[c]?.Trim() ?? string.Empty;
            if (name.Length == 0) continue;
            // First occurrence wins (stable export layout).
            if (!map.ContainsKey(name))
                map[name] = c;
        }
        return map;
    }

    private static string Cell(string[] row, int col) =>
        col >= 0 && col < row.Length ? row[col]?.Trim() ?? string.Empty : string.Empty;

    private static bool TryParseDecimal(string text, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        text = text.Replace(",", "").Trim();
        return decimal.TryParse(text, System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture, out value)
            || decimal.TryParse(text, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.CurrentCulture, out value);
    }

    private static bool TryParseInt(string text, out int value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        text = text.Replace(",", "").Trim();
        if (decimal.TryParse(text, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out var d)
            || decimal.TryParse(text, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.CurrentCulture, out d))
        {
            if (d != Math.Truncate(d)) return false;
            value = (int)d;
            return true;
        }
        return false;
    }

    private static bool TryParseBool(string text, out bool value)
    {
        value = false;
        text = text.Trim();
        if (text is "1" or "true" or "True" or "TRUE" or "yes" or "Yes" or "Y" or "y")
        {
            value = true;
            return true;
        }
        if (text is "0" or "false" or "False" or "FALSE" or "no" or "No" or "N" or "n")
        {
            value = false;
            return true;
        }
        return bool.TryParse(text, out value);
    }

    /// <summary>
    /// Site vendors may only list products owned by the linked website (Product.WebsiteID == LinkedWebsiteID).
    /// This blocks re-sharing goods the linked site itself obtained from other sites.
    /// Member vendors may only list products on the host website (and preferably created by them).
    /// Display vendors may list any host product.
    /// </summary>
    private static string? ValidateOwnership(Vendor vendor, ProductVariant variant)
    {
        return (VendorType)vendor.VendorType switch
        {
            VendorType.Site when vendor.LinkedWebsiteID is int lid =>
                variant.Product.WebsiteID == lid
                    ? null
                    : "Only products owned by the linked website can be listed (borrowed products cannot be re-shared).",
            VendorType.Member =>
                variant.Product.WebsiteID != vendor.WebsiteID
                    ? "Member vendors can only list products on their host website."
                    : vendor.MemberID is int mid && variant.Product.CreatedByMemberID != mid
                        ? "Member vendors can only list products they created."
                        : null,
            _ =>
                variant.Product.WebsiteID == vendor.WebsiteID
                    ? null
                    : "Display vendors can only list products on the host website.",
        };
    }
}
