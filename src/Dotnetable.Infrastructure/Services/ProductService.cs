using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ProductService : IProductService
{
    /// <summary>Product.Status value that marks a product as publicly published.</summary>
    public const byte PublishedStatus = 1;

    private readonly AppDbContext _context;
    private readonly ICurrencyConversionService _currency;
    private readonly IVendorService _vendors;

    public ProductService(AppDbContext context, ICurrencyConversionService currency, IVendorService vendors)
    {
        _context = context;
        _currency = currency;
        _vendors = vendors;
    }

    // ── Admin management ────────────────────────────────────────────

    public async Task<PagedResult<ProductListItemDto>> GetPagedAsync(int? websiteId, ProductFilter filter, GridQuery query, string? search, int? createdByMemberId = null, CancellationToken ct = default)
    {
        var q = _context.Products.AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.FeaturedImageFile)
            .Include(p => p.ProductVariants)
            .AsQueryable();

        if (websiteId is int wid)
            q = q.Where(p => p.WebsiteID == wid);
        if (filter.BrandID is int bid)
            q = q.Where(p => p.BrandID == bid);
        if (filter.Status is byte status)
            q = q.Where(p => p.Status == status);
        if (filter.IsActive is bool active)
            q = q.Where(p => p.IsActive == active);
        if (filter.ProductCategoryID is int cid)
            q = q.Where(p => p.ProductCategoryMaps.Any(m => m.ProductCategoryID == cid));
        var memberScope = createdByMemberId ?? filter.CreatedByMemberId;
        if (memberScope is int memberId)
            q = q.Where(p => p.CreatedByMemberID == memberId);
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(p => p.Title.Contains(search) || p.Slug.Contains(search));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Product.UpdatedAt), fallbackDescending: true)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        var dtos = items.Select(p => new ProductListItemDto
        {
            ProductID = p.ProductID,
            Title = p.Title,
            Slug = p.Slug,
            BrandName = p.Brand?.Name,
            Status = p.Status,
            IsActive = p.IsActive,
            HasVariants = p.HasVariants,
            ProductType = p.ProductType,
            RequiresShipping = p.RequiresShipping,
            MinPriceUsd = p.ProductVariants.Count == 0 ? null : p.ProductVariants.Min(v => v.ReferencePriceUsd > 0 ? v.ReferencePriceUsd : 0),
            MinPrice = p.ProductVariants.Count == 0 ? null : p.ProductVariants.Min(v => v.ReferencePrice > 0 ? v.ReferencePrice : v.ReferencePriceUsd),
            FeaturedImageUrl = p.FeaturedImageFile?.ThumbnailCDN ?? p.FeaturedImageFile?.CNDUrl,
            UpdatedAt = p.UpdatedAt,
        }).ToList();

        return new PagedResult<ProductListItemDto> { Items = dtos, TotalCount = total };
    }

    public async Task<Product?> GetByIdAsync(int productId, CancellationToken ct = default) =>
        await _context.Products
            .Include(p => p.ProductTranslations)
            .Include(p => p.ProductVariants).ThenInclude(v => v.VariantAttributeValues).ThenInclude(a => a.AttributeOption)
            .Include(p => p.ProductCategoryMaps)
            .Include(p => p.ProductMedia).ThenInclude(m => m.MediaSet).ThenInclude(ms => ms.MediaSetItems).ThenInclude(i => i.File)
            .Include(p => p.ProductAttributeValues).ThenInclude(v => v.AttributeDefinition)
            .Include(p => p.ProductAttributeValues).ThenInclude(v => v.AttributeOption)

            .Include(p => p.ProductWarnings).ThenInclude(w => w.ProductWarningTranslations)
            .Include(p => p.ProductWarranties).ThenInclude(w => w.Warranty)
            .Include(p => p.ProductRelationProducts).ThenInclude(r => r.RelatedProduct)
            .Include(p => p.FeaturedImageFile)
            .FirstOrDefaultAsync(p => p.ProductID == productId, ct);

    public async Task<int> CreateSimpleMediaSetAsync(int websiteId, int fileId, CancellationToken ct = default)
    {
        var set = new MediaSet { WebsiteID = websiteId, Name = "Product gallery item", IsShared = false, CreatedAt = DateTime.UtcNow };
        _context.MediaSets.Add(set);
        await _context.SaveChangesAsync(ct);
        _context.MediaSetItems.Add(new MediaSetItem { MediaSetID = set.MediaSetID, FileID = fileId, SortOrder = 0 });
        await _context.SaveChangesAsync(ct);
        return set.MediaSetID;
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
    {
        NormalizeProductFulfillment(product);
        product.CreatedAt = product.UpdatedAt = DateTime.UtcNow;
        _context.Products.Add(product);
        await _context.SaveChangesAsync(ct);
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        NormalizeProductFulfillment(product);
        product.UpdatedAt = DateTime.UtcNow;
        _context.Products.Update(product);
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Physical products always require shipping; digital types clear shipping and keep digital fields coherent.
    /// </summary>
    private static void NormalizeProductFulfillment(Product product)
    {
        if (product.ProductType == (byte)ProductType.Physical)
        {
            product.RequiresShipping = true;
            product.DigitalDownloadUrl = null;
            product.DigitalServiceUrl = null;
            product.DigitalDeliveryNote = null;
            return;
        }

        product.RequiresShipping = false;
        if (product.ProductType != (byte)ProductType.DigitalDownload)
            product.DigitalDownloadUrl = null;
        if (product.ProductType != (byte)ProductType.DigitalService)
            product.DigitalServiceUrl = null;
    }

    public async Task DeleteAsync(int productId, CancellationToken ct = default)
    {
        var product = await _context.Products
            .Include(p => p.ProductTranslations)
            .Include(p => p.ProductVariants)
            .Include(p => p.ProductCategoryMaps)
            .Include(p => p.ProductMedia)
            .Include(p => p.ProductAttributeValues)
            .Include(p => p.ProductWarnings)
            .Include(p => p.ProductWarranties)
            .Include(p => p.ProductRelationProducts)
            .Include(p => p.ProductRelationRelatedProducts)
            .Include(p => p.MenuItems)
            .FirstOrDefaultAsync(p => p.ProductID == productId, ct);
        if (product is null) return;

        foreach (var mi in product.MenuItems)
            mi.ProductID = null;

        // Price history is cascade-removed via variants; clear explicit FKs first when variants go.
        var variantIds = product.ProductVariants.Select(v => v.ProductVariantID).ToList();
        if (variantIds.Count > 0)
            _context.ProductVariantPriceHistories.RemoveRange(
                _context.ProductVariantPriceHistories.Where(h => variantIds.Contains(h.ProductVariantID)));

        _context.ProductRelations.RemoveRange(product.ProductRelationProducts);
        _context.ProductRelations.RemoveRange(product.ProductRelationRelatedProducts);
        _context.ProductWarnings.RemoveRange(product.ProductWarnings);
        _context.ProductWarranties.RemoveRange(product.ProductWarranties);
        _context.ProductAttributeValues.RemoveRange(product.ProductAttributeValues);
        _context.ProductMedia.RemoveRange(product.ProductMedia);
        _context.ProductCategoryMaps.RemoveRange(product.ProductCategoryMaps);
        _context.ProductVariants.RemoveRange(product.ProductVariants);
        _context.ProductTranslations.RemoveRange(product.ProductTranslations);
        _context.Products.Remove(product);
        await _context.SaveChangesAsync(ct);
    }

    public async Task SetActiveAsync(int productId, bool active, CancellationToken ct = default) =>
        await _context.Products.Where(p => p.ProductID == productId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsActive, active), ct);

    // ── Translations ────────────────────────────────────────────────

    public async Task<List<ProductTranslation>> GetTranslationsAsync(int productId, CancellationToken ct = default) =>
        await _context.ProductTranslations.AsNoTracking()
            .Where(t => t.ProductID == productId)
            .ToListAsync(ct);

    public async Task SetTranslationsAsync(int productId, IReadOnlyList<ProductTranslation> translations, CancellationToken ct = default)
    {
        var existing = await _context.ProductTranslations.Where(t => t.ProductID == productId).ToListAsync(ct);

        var keepLanguages = translations
            .Where(t => !string.IsNullOrWhiteSpace(t.Title))
            .Select(t => t.LanguageCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _context.ProductTranslations.RemoveRange(existing.Where(t => !keepLanguages.Contains(t.LanguageCode)));

        foreach (var t in translations)
        {
            if (string.IsNullOrWhiteSpace(t.Title)) continue;
            var current = existing.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, t.LanguageCode, StringComparison.OrdinalIgnoreCase));
            var slug = string.IsNullOrWhiteSpace(t.Slug) ? t.Title.Trim() : t.Slug.Trim();

            if (current is null)
                _context.ProductTranslations.Add(new ProductTranslation
                {
                    ProductID = productId,
                    LanguageCode = t.LanguageCode,
                    Title = t.Title.Trim(),
                    Slug = slug,
                    ShortDescription = t.ShortDescription,
                    Content = t.Content,
                    ExpertReview = t.ExpertReview,
                });
            else
            {
                current.Title = t.Title.Trim();
                current.Slug = slug;
                current.ShortDescription = t.ShortDescription;
                current.Content = t.Content;
                current.ExpertReview = t.ExpertReview;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    // ── Categories ───────────────────────────────────────────────────

    public async Task<List<int>> GetCategoryIdsAsync(int productId, CancellationToken ct = default) =>
        await _context.ProductCategoryMaps.AsNoTracking()
            .Where(m => m.ProductID == productId)
            .Select(m => m.ProductCategoryID)
            .ToListAsync(ct);

    public async Task SetCategoriesAsync(int productId, IReadOnlyList<int> categoryIds, int? primaryCategoryId, CancellationToken ct = default)
    {
        var existing = await _context.ProductCategoryMaps.Where(m => m.ProductID == productId).ToListAsync(ct);
        var wanted = categoryIds.Distinct().ToList();

        _context.ProductCategoryMaps.RemoveRange(existing.Where(m => !wanted.Contains(m.ProductCategoryID)));

        foreach (var categoryId in wanted)
        {
            var current = existing.FirstOrDefault(m => m.ProductCategoryID == categoryId);
            var isPrimary = primaryCategoryId == categoryId;
            if (current is null)
                _context.ProductCategoryMaps.Add(new ProductCategoryMap { ProductID = productId, ProductCategoryID = categoryId, IsPrimary = isPrimary });
            else
                current.IsPrimary = isPrimary;
        }

        await _context.SaveChangesAsync(ct);
    }

    // ── Variants ───────────────────────────────────────────────────────
    // Replace-all-children: ProductVariantID == 0 means insert, existing ids not present are removed.

    public async Task SetVariantsAsync(
        int productId,
        IReadOnlyList<ProductVariant> variants,
        int? changedByMemberId = null,
        IReadOnlyList<IReadOnlyList<(int AttributeDefinitionID, int AttributeOptionID)>>? variantAttributeOptions = null,
        CancellationToken ct = default)
    {
        var product = await _context.Products.AsNoTracking()
            .Where(p => p.ProductID == productId).Select(p => new { p.WebsiteID }).FirstOrDefaultAsync(ct);
        if (product is null) return;

        // Server-side guard so incomplete variants never hit the DB (UI should already block these).
        if (variants.Count == 0)
            throw new ArgumentException("At least one product variant with title, SKU and price is required.");

        var seenSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < variants.Count; i++)
        {
            var v = variants[i];
            if (string.IsNullOrWhiteSpace(v.Title))
                throw new ArgumentException($"Variant {i + 1}: title is required.");
            var title = v.Title.Trim();
            if (title.Length > 200)
                throw new ArgumentException($"Variant {i + 1}: title must be at most 200 characters.");

            if (string.IsNullOrWhiteSpace(v.Sku))
                throw new ArgumentException($"Variant {i + 1}: SKU is required.");
            var sku = v.Sku.Trim();
            if (sku.Length > 100)
                throw new ArgumentException($"Variant {i + 1}: SKU must be at most 100 characters.");
            if (!seenSkus.Add(sku))
                throw new ArgumentException($"Variant {i + 1}: duplicate SKU \"{sku}\".");
            // Site-currency price is authority; USD may be dual-stored or derived.
            if (v.ReferencePrice <= 0 && v.ReferencePriceUsd <= 0)
                throw new ArgumentException($"Variant {i + 1}: price must be greater than zero.");
            v.Title = title;
            v.Sku = sku;
        }

        await NormalizeVariantPricesAsync(product.WebsiteID, variants, ct);

        var existing = await _context.ProductVariants.Where(v => v.ProductID == productId).ToListAsync(ct);
        var wantedIds = variants.Where(v => v.ProductVariantID != 0).Select(v => v.ProductVariantID).ToHashSet();

        var toRemove = existing.Where(v => !wantedIds.Contains(v.ProductVariantID)).ToList();
        if (toRemove.Count > 0)
        {
            var removeIds = toRemove.Select(v => v.ProductVariantID).ToList();
            _context.ProductVariantPriceHistories.RemoveRange(
                _context.ProductVariantPriceHistories.Where(h => removeIds.Contains(h.ProductVariantID)));
            _context.VariantAttributeValues.RemoveRange(
                _context.VariantAttributeValues.Where(a => removeIds.Contains(a.ProductVariantID)));
            _context.ProductVariants.RemoveRange(toRemove);
        }

        var now = DateTime.UtcNow;
        var newVariantsNeedingHistory = new List<ProductVariant>();
        // Snapshot wanted option IDs before SaveChanges so we can sync after new variants get IDs.
        var pendingVariantAttributes = new List<(ProductVariant Target, List<(int AttributeDefinitionID, int AttributeOptionID)> Attrs)>();

        for (var i = 0; i < variants.Count; i++)
        {
            var v = variants[i];
            List<(int AttributeDefinitionID, int AttributeOptionID)> wantedAttrs;
            if (variantAttributeOptions is not null && i < variantAttributeOptions.Count && variantAttributeOptions[i] is not null)
                wantedAttrs = SnapshotVariantAttributeOptions(variantAttributeOptions[i]);
            else
                wantedAttrs = SnapshotVariantAttributeOptions(v.VariantAttributeValues);

            if (v.ProductVariantID == 0)
            {
                var created = new ProductVariant
                {
                    WebsiteID = product.WebsiteID,
                    ProductID = productId,
                    Sku = v.Sku,
                    Title = v.Title,
                    IsDefault = v.IsDefault,
                    ImageFileID = v.ImageFileID,
                    ReferencePrice = v.ReferencePrice,
                    CompareAtPrice = v.CompareAtPrice,
                    ReferencePriceUsd = v.ReferencePriceUsd,
                    CompareAtPriceUsd = v.CompareAtPriceUsd,
                    OverridePrice = v.OverridePrice,
                    Weight = v.Weight,
                    Barcode = v.Barcode,
                    IsActive = v.IsActive,
                    CreatedAt = now,
                };
                _context.ProductVariants.Add(created);
                newVariantsNeedingHistory.Add(created);
                pendingVariantAttributes.Add((created, wantedAttrs));
            }
            else
            {
                var current = existing.FirstOrDefault(x => x.ProductVariantID == v.ProductVariantID);
                if (current is null) continue;

                var priceChanged = current.ReferencePrice != v.ReferencePrice
                    || current.CompareAtPrice != v.CompareAtPrice
                    || current.ReferencePriceUsd != v.ReferencePriceUsd
                    || current.CompareAtPriceUsd != v.CompareAtPriceUsd;

                current.Sku = v.Sku;
                current.Title = v.Title;
                current.IsDefault = v.IsDefault;
                current.ImageFileID = v.ImageFileID;
                current.ReferencePrice = v.ReferencePrice;
                current.CompareAtPrice = v.CompareAtPrice;
                current.ReferencePriceUsd = v.ReferencePriceUsd;
                current.CompareAtPriceUsd = v.CompareAtPriceUsd;
                current.OverridePrice = v.OverridePrice;
                current.Weight = v.Weight;
                current.Barcode = v.Barcode;
                current.IsActive = v.IsActive;

                if (priceChanged)
                    AppendPriceHistory(current.ProductVariantID, v.ReferencePrice, v.CompareAtPrice, v.ReferencePriceUsd, v.CompareAtPriceUsd, now, changedByMemberId);

                pendingVariantAttributes.Add((current, wantedAttrs));
            }
        }

        await _context.SaveChangesAsync(ct);

        // New variants get an ID after SaveChanges — log their initial price and sync attribute options.
        foreach (var created in newVariantsNeedingHistory)
            AppendPriceHistory(created.ProductVariantID, created.ReferencePrice, created.CompareAtPrice, created.ReferencePriceUsd, created.CompareAtPriceUsd, now, changedByMemberId);

        await SyncVariantAttributeValuesAsync(pendingVariantAttributes, ct);

        if (newVariantsNeedingHistory.Count > 0 || pendingVariantAttributes.Count > 0)
            await _context.SaveChangesAsync(ct);

        // Soft retention: drop history older than 18 months for touched variants (keep ~12 months visible).
        var historyVariantIds = variants.Where(v => v.ProductVariantID != 0).Select(v => v.ProductVariantID)
            .Concat(newVariantsNeedingHistory.Select(v => v.ProductVariantID))
            .Distinct()
            .ToList();
        if (historyVariantIds.Count > 0)
        {
            var cutoff = now.AddMonths(-18);
            await _context.ProductVariantPriceHistories
                .Where(h => historyVariantIds.Contains(h.ProductVariantID) && h.RecordedAt < cutoff)
                .ExecuteDeleteAsync(ct);
        }
    }

    /// <summary>
    /// One option per attribute definition (last wins). Empty option IDs are dropped.
    /// </summary>
    private static List<(int AttributeDefinitionID, int AttributeOptionID)> SnapshotVariantAttributeOptions(
        IEnumerable<VariantAttributeValue>? values)
    {
        if (values is null) return new List<(int, int)>();
        return SnapshotVariantAttributeOptions(
            values.Select(a => (a.AttributeDefinitionID, a.AttributeOptionID)));
    }

    private static List<(int AttributeDefinitionID, int AttributeOptionID)> SnapshotVariantAttributeOptions(
        IEnumerable<(int AttributeDefinitionID, int AttributeOptionID)>? values)
    {
        if (values is null) return new List<(int, int)>();
        return values
            .Where(a => a.AttributeDefinitionID > 0 && a.AttributeOptionID > 0)
            .GroupBy(a => a.AttributeDefinitionID)
            .Select(g => g.Last())
            .ToList();
    }

    /// <summary>
    /// Replace-all attribute options for the given variants (composite key: variant + definition).
    /// </summary>
    private async Task SyncVariantAttributeValuesAsync(
        IReadOnlyList<(ProductVariant Target, List<(int AttributeDefinitionID, int AttributeOptionID)> Attrs)> pending,
        CancellationToken ct)
    {
        if (pending.Count == 0) return;

        var variantIds = pending.Select(p => p.Target.ProductVariantID).Where(id => id != 0).Distinct().ToList();
        if (variantIds.Count == 0) return;

        var existing = await _context.VariantAttributeValues
            .Where(a => variantIds.Contains(a.ProductVariantID))
            .ToListAsync(ct);

        foreach (var (target, attrs) in pending)
        {
            var variantId = target.ProductVariantID;
            if (variantId == 0) continue;

            var wantedByDef = attrs
                .GroupBy(a => a.AttributeDefinitionID)
                .ToDictionary(g => g.Key, g => g.Last().AttributeOptionID);

            var currentForVariant = existing.Where(a => a.ProductVariantID == variantId).ToList();

            foreach (var row in currentForVariant.Where(a => !wantedByDef.ContainsKey(a.AttributeDefinitionID)))
                _context.VariantAttributeValues.Remove(row);

            foreach (var (defId, optionId) in wantedByDef)
            {
                var row = currentForVariant.FirstOrDefault(a => a.AttributeDefinitionID == defId);
                if (row is null)
                {
                    _context.VariantAttributeValues.Add(new VariantAttributeValue
                    {
                        ProductVariantID = variantId,
                        AttributeDefinitionID = defId,
                        AttributeOptionID = optionId,
                    });
                }
                else if (row.AttributeOptionID != optionId)
                {
                    row.AttributeOptionID = optionId;
                }
            }
        }
    }

    public async Task<List<ProductVariantPriceHistoryDto>> GetVariantPriceHistoryAsync(
        int productVariantId, int months = 12, CancellationToken ct = default)
    {
        if (months < 1) months = 1;
        if (months > 36) months = 36;
        var from = DateTime.UtcNow.AddMonths(-months);

        return await _context.ProductVariantPriceHistories.AsNoTracking()
            .Where(h => h.ProductVariantID == productVariantId && h.RecordedAt >= from)
            .OrderByDescending(h => h.RecordedAt)
            .Select(h => new ProductVariantPriceHistoryDto
            {
                ProductVariantPriceHistoryID = h.ProductVariantPriceHistoryID,
                ProductVariantID = h.ProductVariantID,
                ReferencePrice = h.ReferencePrice,
                CompareAtPrice = h.CompareAtPrice,
                ReferencePriceUsd = h.ReferencePriceUsd,
                CompareAtPriceUsd = h.CompareAtPriceUsd,
                RecordedAt = h.RecordedAt,
                ChangedByMemberId = h.ChangedByMemberId,
            })
            .ToListAsync(ct);
    }

    /// <summary>
    /// Ensures site-currency price is set and USD dual is filled (always derived when missing so multi-currency works).
    /// </summary>
    private async Task NormalizeVariantPricesAsync(int websiteId, IReadOnlyList<ProductVariant> variants, CancellationToken ct)
    {
        decimal? usdToLocal = null;
        try
        {
            var (_, rate) = await _currency.GetActiveRateAsync(websiteId, null, ct);
            usdToLocal = rate <= 0 ? 1m : rate;
        }
        catch (InvalidOperationException)
        {
            usdToLocal = 1m;
        }

        var rateVal = usdToLocal.Value;
        foreach (var v in variants)
        {
            if (v.ReferencePrice <= 0 && v.ReferencePriceUsd > 0)
                v.ReferencePrice = Math.Round(v.ReferencePriceUsd * rateVal, 4, MidpointRounding.AwayFromZero);
            if (v.ReferencePriceUsd <= 0 && v.ReferencePrice > 0)
                v.ReferencePriceUsd = rateVal == 0 ? 0 : Math.Round(v.ReferencePrice / rateVal, 4, MidpointRounding.AwayFromZero);

            if (v.CompareAtPrice is null && v.CompareAtPriceUsd is decimal cmpUsd && cmpUsd > 0)
                v.CompareAtPrice = Math.Round(cmpUsd * rateVal, 4, MidpointRounding.AwayFromZero);
            if (v.CompareAtPriceUsd is null && v.CompareAtPrice is decimal cmpLocal && cmpLocal > 0)
                v.CompareAtPriceUsd = rateVal == 0 ? null : Math.Round(cmpLocal / rateVal, 4, MidpointRounding.AwayFromZero);
        }
    }

    private void AppendPriceHistory(
        int productVariantId,
        decimal referencePrice,
        decimal? compareAtPrice,
        decimal referencePriceUsd,
        decimal? compareAtPriceUsd,
        DateTime recordedAt,
        int? changedByMemberId)
    {
        _context.ProductVariantPriceHistories.Add(new ProductVariantPriceHistory
        {
            ProductVariantID = productVariantId,
            ReferencePrice = referencePrice,
            CompareAtPrice = compareAtPrice,
            ReferencePriceUsd = referencePriceUsd,
            CompareAtPriceUsd = compareAtPriceUsd,
            RecordedAt = recordedAt,
            ChangedByMemberId = changedByMemberId,
        });
    }

    // ── Media ──────────────────────────────────────────────────────────

    public async Task SetMediaAsync(int productId, IReadOnlyList<int> mediaSetIds, CancellationToken ct = default)
    {
        var existing = await _context.ProductMedia.Where(m => m.ProductID == productId).ToListAsync(ct);
        _context.ProductMedia.RemoveRange(existing);

        var sortOrder = 0;
        foreach (var mediaSetId in mediaSetIds.Distinct())
            _context.ProductMedia.Add(new ProductMedium { ProductID = productId, MediaSetID = mediaSetId, SortOrder = sortOrder++ });

        await _context.SaveChangesAsync(ct);
    }

    // ── Attribute values (product-level) ───────────────────────────────

    public async Task SetAttributeValuesAsync(int productId, IReadOnlyList<ProductAttributeValue> values, CancellationToken ct = default)
    {
        // Empty values mean "this attribute does not apply to the product" — do not persist them.
        var meaningful = values
            .Where(HasMeaningfulAttributeValue)
            .GroupBy(v => v.AttributeDefinitionID)
            .Select(g => g.Last())
            .ToList();

        var existing = await _context.ProductAttributeValues
            .Include(v => v.ProductAttributeValueTranslations)
            .Where(v => v.ProductID == productId).ToListAsync(ct);

        var wantedDefIds = meaningful.Select(v => v.AttributeDefinitionID).ToHashSet();
        var toRemove = existing.Where(v => !wantedDefIds.Contains(v.AttributeDefinitionID)).ToList();
        foreach (var r in toRemove)
            _context.ProductAttributeValueTranslations.RemoveRange(r.ProductAttributeValueTranslations);
        _context.ProductAttributeValues.RemoveRange(toRemove);

        var sortOrder = 0;
        foreach (var v in meaningful)
        {
            var current = existing.FirstOrDefault(x => x.AttributeDefinitionID == v.AttributeDefinitionID);
            if (current is null)
            {
                _context.ProductAttributeValues.Add(new ProductAttributeValue
                {
                    ProductID = productId,
                    AttributeDefinitionID = v.AttributeDefinitionID,
                    AttributeOptionID = NormalizeOptionId(v.AttributeOptionID),
                    CustomValue = NormalizeCustomValue(v.CustomValue),
                    NumericValue = v.NumericValue,
                    IsFeatured = v.IsFeatured,
                    SortOrder = sortOrder++,
                });
            }
            else
            {
                current.AttributeOptionID = NormalizeOptionId(v.AttributeOptionID);
                current.CustomValue = NormalizeCustomValue(v.CustomValue);
                current.NumericValue = v.NumericValue;
                current.IsFeatured = v.IsFeatured;
                current.SortOrder = sortOrder++;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    /// <summary>True when the value carries a real option / number / text (featured alone is not enough).</summary>
    internal static bool HasMeaningfulAttributeValue(ProductAttributeValue v)
    {
        if (v.AttributeDefinitionID <= 0) return false;
        if (v.AttributeOptionID is int oid && oid > 0) return true;
        if (v.NumericValue is not null) return true;
        if (!string.IsNullOrWhiteSpace(v.CustomValue)) return true;
        return false;
    }

    private static int? NormalizeOptionId(int? optionId) =>
        optionId is int id && id > 0 ? id : null;

    private static string? NormalizeCustomValue(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    // ── Warnings ─────────────────────────────────────────────────────────

    public async Task SetWarningsAsync(int productId, IReadOnlyList<ProductWarning> warnings, CancellationToken ct = default)
    {
        var existing = await _context.ProductWarnings
            .Include(w => w.ProductWarningTranslations)
            .Where(w => w.ProductID == productId).ToListAsync(ct);
        var wantedIds = warnings.Where(w => w.ProductWarningID != 0).Select(w => w.ProductWarningID).ToHashSet();

        var toRemove = existing.Where(w => !wantedIds.Contains(w.ProductWarningID)).ToList();
        foreach (var r in toRemove)
            _context.ProductWarningTranslations.RemoveRange(r.ProductWarningTranslations);
        _context.ProductWarnings.RemoveRange(toRemove);

        foreach (var w in warnings)
        {
            var severity = string.IsNullOrWhiteSpace(w.Severity) ? "info" : w.Severity;
            if (w.ProductWarningID == 0)
                _context.ProductWarnings.Add(new ProductWarning
                {
                    ProductID = productId,
                    Severity = severity,
                    Text = w.Text,
                    IsActive = w.IsActive,
                });
            else
            {
                var current = existing.FirstOrDefault(x => x.ProductWarningID == w.ProductWarningID);
                if (current is null) continue;
                current.Severity = severity;
                current.Text = w.Text;
                current.IsActive = w.IsActive;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    // ── Warranties ───────────────────────────────────────────────────────

    public async Task SetWarrantiesAsync(int productId, IReadOnlyList<ProductWarranty> warranties, CancellationToken ct = default)
    {
        var existing = await _context.ProductWarranties.Where(w => w.ProductID == productId).ToListAsync(ct);
        var wantedIds = warranties.Where(w => w.ProductWarrantyID != 0).Select(w => w.ProductWarrantyID).ToHashSet();

        _context.ProductWarranties.RemoveRange(existing.Where(w => !wantedIds.Contains(w.ProductWarrantyID)));

        var sort = 0;
        foreach (var w in warranties)
        {
            // Skip empty rows (neither catalog pick nor custom title).
            var hasCatalog = w.WarrantyID is > 0;
            var hasCustom = !string.IsNullOrWhiteSpace(w.CustomTitle) || !string.IsNullOrWhiteSpace(w.CustomDescription);
            if (!hasCatalog && !hasCustom) continue;

            if (w.ProductWarrantyID == 0)
            {
                _context.ProductWarranties.Add(new ProductWarranty
                {
                    ProductID = productId,
                    WarrantyID = hasCatalog ? w.WarrantyID : null,
                    CustomTitle = string.IsNullOrWhiteSpace(w.CustomTitle) ? null : w.CustomTitle.Trim(),
                    CustomDescription = string.IsNullOrWhiteSpace(w.CustomDescription) ? null : w.CustomDescription.Trim(),
                    SortOrder = sort++,
                    IsActive = w.IsActive,
                });
            }
            else
            {
                var current = existing.FirstOrDefault(x => x.ProductWarrantyID == w.ProductWarrantyID);
                if (current is null) continue;
                current.WarrantyID = hasCatalog ? w.WarrantyID : null;
                current.CustomTitle = string.IsNullOrWhiteSpace(w.CustomTitle) ? null : w.CustomTitle.Trim();
                current.CustomDescription = string.IsNullOrWhiteSpace(w.CustomDescription) ? null : w.CustomDescription.Trim();
                current.SortOrder = sort++;
                current.IsActive = w.IsActive;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    // ── Related products ─────────────────────────────────────────────────

    public async Task SetRelatedProductsAsync(int productId, IReadOnlyList<(int RelatedProductID, byte RelationType, int SortOrder)> related, CancellationToken ct = default)
    {
        var existing = await _context.ProductRelations.Where(r => r.ProductID == productId).ToListAsync(ct);
        var wanted = related.Select(r => r.RelatedProductID).Distinct().ToHashSet();

        _context.ProductRelations.RemoveRange(existing.Where(r => !wanted.Contains(r.RelatedProductID)));

        foreach (var r in related)
        {
            var current = existing.FirstOrDefault(x => x.RelatedProductID == r.RelatedProductID);
            if (current is null)
                _context.ProductRelations.Add(new ProductRelation
                {
                    ProductID = productId,
                    RelatedProductID = r.RelatedProductID,
                    RelationType = r.RelationType,
                    SortOrder = r.SortOrder,
                });
            else
            {
                current.RelationType = r.RelationType;
                current.SortOrder = r.SortOrder;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    // ── Public read ─────────────────────────────────────────────────

    public async Task<PagedResult<ProductSummaryDto>> GetPublishedAsync(
        int websiteId, string? categorySlug, string? brandSlug, string? search,
        decimal? minPriceUsd, decimal? maxPriceUsd,
        int pageIndex, int pageSize, string? languageCode = null, string? currencyCode = null,
        bool? inStock = null, IReadOnlyList<int>? attributeOptionIds = null, CancellationToken ct = default)
    {
        // Own products of the host site (include inventory for availability).
        IQueryable<Product> q = PublishedQuery(websiteId)
            .Include(p => p.ProductVariants).ThenInclude(v => v.InventoryItems);

        if (!string.IsNullOrWhiteSpace(categorySlug))
            q = q.Where(p => p.ProductCategoryMaps.Any(m =>
                m.ProductCategory.Slug == categorySlug || m.ProductCategory.ProductCategoryTranslations.Any(t => t.Slug == categorySlug)));
        if (!string.IsNullOrWhiteSpace(brandSlug))
            q = q.Where(p => p.Brand != null && p.Brand.Slug == brandSlug);
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(p => p.Title.Contains(search) || p.ProductTranslations.Any(t => t.Title.Contains(search)));
        if (minPriceUsd is decimal min)
            q = q.Where(p => p.ProductVariants.Any(v => v.IsActive && v.ReferencePriceUsd >= min));
        if (maxPriceUsd is decimal max)
            q = q.Where(p => p.ProductVariants.Any(v => v.IsActive && v.ReferencePriceUsd <= max));

        q = ApplyAttributeOptionFilter(q, attributeOptionIds);

        var localProducts = await q
            .OrderByDescending(p => p.SortOrder).ThenByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        // Site-linked vendors: only products owned by the linked website (never re-shared imports).
        var linked = await LoadLinkedSiteProductsAsync(websiteId, categorySlug, brandSlug, search, minPriceUsd, maxPriceUsd, attributeOptionIds, ct);

        var combined = new List<(Product Product, Vendor? Vendor)>(localProducts.Count + linked.Count);
        combined.AddRange(localProducts.Select(p => ((Product Product, Vendor? Vendor))(p, null)));
        combined.AddRange(linked.Select(x => ((Product Product, Vendor? Vendor))(x.Product, x.Vendor)));

        // Availability totals for filtering and summary projection.
        var stockByKey = await ResolveListingStockBulkAsync(websiteId, combined, ct);
        if (inStock is bool wantInStock)
        {
            combined = combined
                .Where(x =>
                {
                    var key = ListingKey(x.Product.ProductID, x.Vendor?.VendorID);
                    var available = stockByKey.GetValueOrDefault(key);
                    return wantInStock ? StockDisplay.IsInStock(available) : !StockDisplay.IsInStock(available);
                })
                .ToList();
        }

        var total = combined.Count;
        var take = pageSize < 1 ? 12 : pageSize;
        var skip = (pageIndex < 1 ? 0 : pageIndex - 1) * take;
        var page = combined.Skip(skip).Take(take).ToList();

        var items = new List<ProductSummaryDto>(page.Count);
        foreach (var (p, vendor) in page)
        {
            var available = stockByKey.GetValueOrDefault(ListingKey(p.ProductID, vendor?.VendorID));
            var dto = await ProjectSummaryAsync(p, languageCode, currencyCode, ct, vendor, available);
            items.Add(dto);
        }

        return new PagedResult<ProductSummaryDto> { Items = items, TotalCount = total };
    }

    public async Task<ProductDetailDto?> GetBySlugAsync(int websiteId, string slug, string? languageCode = null, string? currencyCode = null, CancellationToken ct = default)
    {
        // Prefer host-owned product.
        var product = await DetailQuery(websiteId)
            .FirstOrDefaultAsync(p => p.Slug == slug || p.ProductTranslations.Any(t => t.Slug == slug), ct);
        Vendor? vendor = null;

        if (product is null)
        {
            // Linked-site product: slug may be "vendorSlug--productSlug" or raw product slug on source.
            var siteVendors = await _vendors.GetActiveSiteLinksAsync(websiteId, ct);
            foreach (var v in siteVendors)
            {
                if (v.LinkedWebsiteID is not int lid) continue;
                string productSlug = slug;
                if (slug.StartsWith(v.Slug + "--", StringComparison.OrdinalIgnoreCase))
                    productSlug = slug[(v.Slug.Length + 2)..];

                product = await DetailQuery(lid)
                    .FirstOrDefaultAsync(p => p.Slug == productSlug || p.ProductTranslations.Any(t => t.Slug == productSlug), ct);
                if (product is not null)
                {
                    // Hard rule: only products owned by the linked website itself.
                    if (product.WebsiteID != lid) { product = null; continue; }
                    vendor = v;
                    break;
                }
            }
        }

        if (product is null) return null;
        return await ProjectDetailAsync(product, languageCode, currencyCode, ct, vendor, websiteId);
    }

    public async Task<List<ProductRefDto>> GetRelatedAsync(int websiteId, string slug, int take, string? languageCode = null, string? currencyCode = null, CancellationToken ct = default)
    {
        var product = await PublishedQuery(websiteId)
            .Include(p => p.ProductRelationProducts).ThenInclude(r => r.RelatedProduct).ThenInclude(rp => rp.FeaturedImageFile)
            .Include(p => p.ProductRelationProducts).ThenInclude(r => r.RelatedProduct).ThenInclude(rp => rp.ProductVariants)
            .FirstOrDefaultAsync(p => p.Slug == slug || p.ProductTranslations.Any(t => t.Slug == slug), ct);
        if (product is null) return new List<ProductRefDto>();

        var relations = product.ProductRelationProducts
            .Where(r => r.RelatedProduct.IsActive && r.RelatedProduct.Status == PublishedStatus)
            .OrderBy(r => r.SortOrder)
            .Take(take < 1 ? 8 : take)
            .ToList();

        var result = new List<ProductRefDto>(relations.Count);
        foreach (var r in relations)
            result.Add(await ProjectRefAsync(r.RelatedProduct, r.RelationType, languageCode, currencyCode, ct));
        return result;
    }

    private IQueryable<Product> PublishedQuery(int websiteId) =>
        _context.Products.AsNoTracking()
            .Where(p => p.WebsiteID == websiteId && p.IsActive && p.Status == PublishedStatus)
            .Include(p => p.Brand)
            .Include(p => p.FeaturedImageFile)
            .Include(p => p.ProductTranslations)
            .Include(p => p.ProductVariants);

    private IQueryable<Product> DetailQuery(int websiteId) =>
        PublishedQuery(websiteId)
            .Include(p => p.ProductVariants).ThenInclude(v => v.ImageFile)
            .Include(p => p.ProductVariants).ThenInclude(v => v.InventoryItems)
            .Include(p => p.ProductVariants).ThenInclude(v => v.VariantAttributeValues).ThenInclude(a => a.AttributeDefinition).ThenInclude(a => a.AttributeDefinitionTranslations)
            .Include(p => p.ProductVariants).ThenInclude(v => v.VariantAttributeValues).ThenInclude(a => a.AttributeOption).ThenInclude(o => o.AttributeOptionTranslations)
            .Include(p => p.ProductCategoryMaps).ThenInclude(m => m.ProductCategory).ThenInclude(c => c.ProductCategoryTranslations)
            .Include(p => p.ProductAttributeValues).ThenInclude(v => v.AttributeDefinition).ThenInclude(a => a.AttributeDefinitionTranslations)
            .Include(p => p.ProductAttributeValues).ThenInclude(v => v.AttributeOption).ThenInclude(o => o!.AttributeOptionTranslations)
            .Include(p => p.ProductAttributeValues).ThenInclude(v => v.ProductAttributeValueTranslations)
            .Include(p => p.ProductWarnings).ThenInclude(w => w.ProductWarningTranslations)
            .Include(p => p.ProductWarranties).ThenInclude(w => w.Warranty!).ThenInclude(w => w.WarrantyTranslations)
            .Include(p => p.ProductRelationProducts).ThenInclude(r => r.RelatedProduct).ThenInclude(rp => rp.FeaturedImageFile)
            .Include(p => p.ProductRelationProducts).ThenInclude(r => r.RelatedProduct).ThenInclude(rp => rp.ProductVariants)
            .Include(p => p.ProductMedia).ThenInclude(m => m.MediaSet).ThenInclude(ms => ms.MediaSetItems).ThenInclude(i => i.File);

    /// <summary>
    /// Products owned by linked websites (Product.WebsiteID == LinkedWebsiteID only — never re-share).
    /// </summary>
    private async Task<List<(Product Product, Vendor Vendor)>> LoadLinkedSiteProductsAsync(
        int hostWebsiteId, string? categorySlug, string? brandSlug, string? search,
        decimal? minPriceUsd, decimal? maxPriceUsd,
        IReadOnlyList<int>? attributeOptionIds,
        CancellationToken ct)
    {
        var siteVendors = await _vendors.GetActiveSiteLinksAsync(hostWebsiteId, ct);
        var result = new List<(Product, Vendor)>();
        foreach (var vendor in siteVendors)
        {
            if (vendor.LinkedWebsiteID is not int lid) continue;
            IQueryable<Product> q = PublishedQuery(lid)
                .Include(p => p.ProductVariants).ThenInclude(v => v.InventoryItems);
            // Ownership guard: only source-owned products.
            q = q.Where(p => p.WebsiteID == lid);

            if (!string.IsNullOrWhiteSpace(categorySlug))
                q = q.Where(p => p.ProductCategoryMaps.Any(m =>
                    m.ProductCategory.Slug == categorySlug || m.ProductCategory.ProductCategoryTranslations.Any(t => t.Slug == categorySlug)));
            if (!string.IsNullOrWhiteSpace(brandSlug))
                q = q.Where(p => p.Brand != null && p.Brand.Slug == brandSlug);
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(p => p.Title.Contains(search) || p.ProductTranslations.Any(t => t.Title.Contains(search)));
            if (minPriceUsd is decimal min)
                q = q.Where(p => p.ProductVariants.Any(v => v.IsActive && v.ReferencePriceUsd >= min));
            if (maxPriceUsd is decimal max)
                q = q.Where(p => p.ProductVariants.Any(v => v.IsActive && v.ReferencePriceUsd <= max));

            q = ApplyAttributeOptionFilter(q, attributeOptionIds);

            var products = await q
                .OrderByDescending(p => p.SortOrder).ThenByDescending(p => p.CreatedAt)
                .ToListAsync(ct);
            foreach (var p in products)
                result.Add((p, vendor));
        }
        return result;
    }

    /// <summary>
    /// AND filter: product must match every option id via product-level or variant attribute values.
    /// Options for the same definition still stack as AND (caller should pass one option per facet).
    /// </summary>
    private static IQueryable<Product> ApplyAttributeOptionFilter(
        IQueryable<Product> q,
        IReadOnlyList<int>? attributeOptionIds)
    {
        if (attributeOptionIds is null || attributeOptionIds.Count == 0)
            return q;

        foreach (var optionId in attributeOptionIds.Where(id => id > 0).Distinct())
        {
            var oid = optionId;
            q = q.Where(p =>
                p.ProductAttributeValues.Any(a => a.AttributeOptionID == oid)
                || p.ProductVariants.Any(v => v.IsActive && v.VariantAttributeValues.Any(va => va.AttributeOptionID == oid)));
        }

        return q;
    }

    private static string ListingKey(int productId, int? vendorId) => $"{productId}:{vendorId?.ToString() ?? "0"}";

    /// <summary>
    /// Total available units for a catalog listing: sum of store listing available stock only.
    /// Products without an active store listing (or with zero available) are not sellable.
    /// </summary>
    private async Task<Dictionary<string, int>> ResolveListingStockBulkAsync(
        int hostWebsiteId, List<(Product Product, Vendor? Vendor)> listings, CancellationToken ct)
    {
        var result = new Dictionary<string, int>(StringComparer.Ordinal);
        if (listings.Count == 0) return result;

        var hostProductIds = listings.Where(x => x.Vendor is null).Select(x => x.Product.ProductID).Distinct().ToList();
        var variantIdsByProduct = listings
            .SelectMany(x => x.Product.ProductVariants.Where(v => v.IsActive).Select(v => (x.Product.ProductID, v.ProductVariantID)))
            .GroupBy(x => x.ProductID)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ProductVariantID).Distinct().ToList());

        // Marketplace seller available stocks on host (Display/Member/Site). Inventory total is the sum of these.
        var hostVariantIds = hostProductIds
            .SelectMany(pid => variantIdsByProduct.GetValueOrDefault(pid) ?? new List<int>())
            .Distinct()
            .ToList();
        var sellerStockByVariant = hostVariantIds.Count == 0
            ? new Dictionary<int, int>()
            : (await _context.VendorProducts.AsNoTracking()
                .Where(vp => vp.WebsiteID == hostWebsiteId && vp.IsActive
                             && hostVariantIds.Contains(vp.ProductVariantID))
                .Select(vp => new { vp.ProductVariantID, vp.StockQuantity, vp.QuantityReserved })
                .ToListAsync(ct))
              .GroupBy(x => x.ProductVariantID)
              .ToDictionary(
                  g => g.Key,
                  g => StockDisplay.Aggregate(g.Select(x =>
                      x.StockQuantity < 0 ? -1 : Math.Max(0, x.StockQuantity - x.QuantityReserved))));

        // Site-linked vendor listing stocks (host VendorProduct rows) — use available (on-hand − reserved).
        var siteVendorIds = listings.Where(x => x.Vendor is not null).Select(x => x.Vendor!.VendorID).Distinct().ToList();
        var siteListingStocks = siteVendorIds.Count == 0
            ? new List<(int VendorID, int ProductVariantID, int Available)>()
            : (await _context.VendorProducts.AsNoTracking()
                .Where(vp => siteVendorIds.Contains(vp.VendorID) && vp.IsActive)
                .Select(vp => new { vp.VendorID, vp.ProductVariantID, vp.StockQuantity, vp.QuantityReserved })
                .ToListAsync(ct))
              .Select(x => (
                  VendorID: x.VendorID,
                  ProductVariantID: x.ProductVariantID,
                  Available: x.StockQuantity < 0 ? -1 : Math.Max(0, x.StockQuantity - x.QuantityReserved)))
              .ToList();

        foreach (var (product, vendor) in listings)
        {
            var key = ListingKey(product.ProductID, vendor?.VendorID);
            var activeVariants = product.ProductVariants.Where(v => v.IsActive).ToList();
            if (activeVariants.Count == 0)
            {
                result[key] = 0;
                continue;
            }

            if (vendor is null)
            {
                // Host catalog: sellable stock is only the sum of store listing available quantities.
                // No store listing / zero stock ⇒ not for sale. Unlimited (-1) wins over finite sums.
                result[key] = StockDisplay.Aggregate(
                    activeVariants.Select(v => sellerStockByVariant.GetValueOrDefault(v.ProductVariantID)));
            }
            else
            {
                // Site-linked row: only that vendor's listing available stock (no warehouse fallback).
                result[key] = StockDisplay.Aggregate(
                    siteListingStocks
                        .Where(s => s.VendorID == vendor.VendorID
                                    && activeVariants.Any(v => v.ProductVariantID == s.ProductVariantID))
                        .Select(s => s.Available));
            }
        }

        return result;
    }

    // ── Projection helpers ──────────────────────────────────────────

    private async Task<ProductSummaryDto> ProjectSummaryAsync(
        Product p, string? lang, string? currencyCode, CancellationToken ct, Vendor? vendor = null, int? availableStock = null)
    {
        var (title, slug, shortDescription) = LocalizedCore(p, lang);
        var activeVariants = p.ProductVariants.Where(v => v.IsActive).ToList();
        var defaultVariant = activeVariants.FirstOrDefault(v => v.IsDefault) ?? activeVariants.FirstOrDefault();

        // Host display currency; pricing conversion uses the host website when sold via a vendor.
        var priceWebsiteId = vendor?.WebsiteID ?? p.WebsiteID;
        if (vendor is not null)
            slug = $"{vendor.Slug}--{slug}";

        var stock = availableStock ?? 0;
        MoneyDto? minPrice = null;
        if (activeVariants.Count > 0)
        {
            var minLocal = activeVariants.Min(v => v.ReferencePrice > 0 ? v.ReferencePrice : 0);
            var minUsd = activeVariants.Min(v => v.ReferencePriceUsd);
            if (minLocal <= 0 && minUsd > 0)
                minPrice = await _currency.ToDisplayAsync(priceWebsiteId, minUsd, currencyCode, ct);
            else
                minPrice = await _currency.ToDisplayFromLocalAsync(priceWebsiteId, minLocal, null, currencyCode, minUsd > 0 ? minUsd : null, ct);
        }

        return new ProductSummaryDto
        {
            ProductID = p.ProductID, Slug = slug, Title = title, ShortDescription = shortDescription,
            FeaturedImageUrl = p.FeaturedImageFile?.ThumbnailCDN ?? p.FeaturedImageFile?.CNDUrl,
            BrandName = p.Brand?.Name,
            DefaultSku = defaultVariant?.Sku,
            MinPrice = minPrice!,
            AvgRating = p.AvgRating, RatingCount = p.RatingCount, HasVariants = p.HasVariants,
            ProductType = p.ProductType,
            RequiresShipping = p.RequiresShipping,
            IsUnlimitedStock = StockDisplay.IsUnlimited(stock),
            IsInStock = StockDisplay.IsInStock(stock),
            StockQuantity = stock,
            DisplayStockQuantity = StockDisplay.ExactCountOrNull(stock),
            VendorID = vendor?.VendorID,
            VendorName = vendor?.Name,
        };
    }

    private async Task<ProductRefDto> ProjectRefAsync(Product p, byte relationType, string? lang, string? currencyCode, CancellationToken ct)
    {
        var (title, slug, _) = LocalizedCore(p, lang);
        var activeVariants = p.ProductVariants.Where(v => v.IsActive).ToList();
        MoneyDto? minPrice = null;
        if (activeVariants.Count > 0)
        {
            var minLocal = activeVariants.Min(v => v.ReferencePrice > 0 ? v.ReferencePrice : 0);
            var minUsd = activeVariants.Min(v => v.ReferencePriceUsd);
            if (minLocal <= 0 && minUsd > 0)
                minPrice = await _currency.ToDisplayAsync(p.WebsiteID, minUsd, currencyCode, ct);
            else
                minPrice = await _currency.ToDisplayFromLocalAsync(p.WebsiteID, minLocal, null, currencyCode, minUsd > 0 ? minUsd : null, ct);
        }

        return new ProductRefDto
        {
            ProductID = p.ProductID, Slug = slug, Title = title,
            ImageUrl = p.FeaturedImageFile?.ThumbnailCDN ?? p.FeaturedImageFile?.CNDUrl,
            MinPrice = minPrice, RelationType = relationType,
        };
    }

    private async Task<ProductDetailDto> ProjectDetailAsync(
        Product p, string? lang, string? currencyCode, CancellationToken ct, Vendor? vendor = null, int? hostWebsiteId = null)
    {
        var priceWebsiteId = hostWebsiteId ?? vendor?.WebsiteID ?? p.WebsiteID;
        var hostId = hostWebsiteId ?? vendor?.WebsiteID ?? p.WebsiteID;

        var categories = p.ProductCategoryMaps
            .Where(m => m.ProductCategory is not null)
            .Select(m => LocalizeCategory(m.ProductCategory, lang))
            .ToList();

        // Optional single-vendor overrides (site-linked product page).
        Dictionary<int, VendorProduct>? focusedListings = null;
        if (vendor is not null)
        {
            focusedListings = await _context.VendorProducts.AsNoTracking()
                .Where(vp => vp.VendorID == vendor.VendorID && vp.IsActive)
                .ToDictionaryAsync(vp => vp.ProductVariantID, ct);
        }

        // Marketplace sellers on the host for this product — only in-stock (available) listings are returned.
        var variantIds = p.ProductVariants.Where(v => v.IsActive).Select(v => v.ProductVariantID).ToList();
        // StockQuantity < 0 = unlimited digital; otherwise available = on-hand − reserved.
        var sellerRows = vendor is null && variantIds.Count > 0
            ? await _context.VendorProducts.AsNoTracking()
                .Include(vp => vp.Vendor)
                .Include(vp => vp.ProductVariant)
                .Where(vp => vp.WebsiteID == hostId
                             && vp.IsActive
                             && (vp.StockQuantity < 0 || vp.StockQuantity - vp.QuantityReserved > 0)
                             && variantIds.Contains(vp.ProductVariantID)
                             && vp.Vendor.IsActive)
                .OrderBy(vp => vp.Vendor.Name)
                .ThenBy(vp => vp.ProductVariant.Title)
                .ToListAsync(ct)
            : new List<VendorProduct>();

        // When viewing a single site-linked vendor, only that vendor's in-stock offers.
        if (vendor is not null && focusedListings is not null)
        {
            sellerRows = focusedListings.Values
                .Where(vp => vp.IsActive && IVendorProductService.Available(vp) > 0)
                .ToList();
            // Attach navigation if missing (from dictionary query without Include).
            if (sellerRows.Count > 0 && sellerRows[0].Vendor is null)
            {
                var reloaded = await _context.VendorProducts.AsNoTracking()
                    .Include(vp => vp.Vendor)
                    .Include(vp => vp.ProductVariant)
                    .Where(vp => vp.VendorID == vendor.VendorID && vp.IsActive
                                 && (vp.StockQuantity < 0 || vp.StockQuantity - vp.QuantityReserved > 0)
                                 && variantIds.Contains(vp.ProductVariantID))
                    .ToListAsync(ct);
                sellerRows = reloaded;
            }
        }

        var sellers = new List<ProductSellerOfferDto>(sellerRows.Count);
        foreach (var vp in sellerRows)
        {
            var unitLocal = vp.OverridePriceLocal ?? (vp.ReferencePrice > 0 ? vp.ReferencePrice : 0);
            var unitUsd = vp.OverridePrice ?? vp.ReferencePriceUsd;
            var stock = PublicStock(IVendorProductService.Available(vp));
            MoneyDto price;
            if (unitLocal > 0)
                price = await _currency.ToDisplayFromLocalAsync(priceWebsiteId, unitLocal, null, currencyCode, unitUsd > 0 ? unitUsd : null, ct);
            else
                price = await _currency.ToDisplayAsync(priceWebsiteId, unitUsd, currencyCode, ct);
            sellers.Add(new ProductSellerOfferDto
            {
                VendorID = vp.VendorID,
                VendorName = vp.Vendor?.Name ?? string.Empty,
                VendorSlug = vp.Vendor?.Slug ?? string.Empty,
                VendorType = vp.Vendor?.VendorType ?? 0,
                VendorProductID = vp.VendorProductID,
                ProductVariantID = vp.ProductVariantID,
                VariantTitle = vp.ProductVariant?.Title ?? string.Empty,
                Sku = vp.ProductVariant?.Sku ?? string.Empty,
                Price = price,
                StockQuantity = stock,
                IsInStock = StockDisplay.IsInStock(stock),
                DisplayStockQuantity = StockDisplay.ExactCountOrNull(stock),
                DeliveryDays = vp.DeliveryDays,
            });
        }

        var sellerStockByVariant = sellers
            .GroupBy(s => s.ProductVariantID)
            .ToDictionary(g => g.Key, g => StockDisplay.Aggregate(g.Select(x => x.StockQuantity)));

        var variants = new List<ProductVariantDto>();
        foreach (var v in p.ProductVariants.Where(v => v.IsActive))
        {
            VendorProduct? listing = null;
            focusedListings?.TryGetValue(v.ProductVariantID, out listing);

            // Sellable only via store listings — no warehouse-only path.
            int stock = vendor is not null
                ? (listing is not null ? PublicStock(IVendorProductService.Available(listing)) : 0)
                : sellerStockByVariant.GetValueOrDefault(v.ProductVariantID);

            // Hide fully unavailable variants when other sellable options exist; keep at least one row for OOS products.
            // Always include variants that have stock; OOS variants are omitted so they are not offered for sale.
            if (!StockDisplay.IsInStock(stock))
                continue;

            var unitLocal = listing?.OverridePriceLocal
                ?? (listing is not null && listing.ReferencePrice > 0 ? listing.ReferencePrice : (v.ReferencePrice > 0 ? v.ReferencePrice : 0));
            var unitUsd = listing?.OverridePrice ?? listing?.ReferencePriceUsd ?? v.ReferencePriceUsd;
            MoneyDto price;
            if (unitLocal > 0)
                price = await _currency.ToDisplayFromLocalAsync(priceWebsiteId, unitLocal, null, currencyCode, unitUsd > 0 ? unitUsd : null, ct);
            else
                price = await _currency.ToDisplayAsync(priceWebsiteId, unitUsd, currencyCode, ct);

            MoneyDto? compareAt = null;
            if (v.CompareAtPrice is decimal cmpLocal && cmpLocal > 0)
                compareAt = await _currency.ToDisplayFromLocalAsync(priceWebsiteId, cmpLocal, null, currencyCode, v.CompareAtPriceUsd, ct);
            else if (v.CompareAtPriceUsd is decimal cmpUsd)
                compareAt = await _currency.ToDisplayAsync(priceWebsiteId, cmpUsd, currencyCode, ct);

            variants.Add(new ProductVariantDto
            {
                ProductVariantID = v.ProductVariantID, Sku = v.Sku, Title = v.Title, IsDefault = v.IsDefault,
                ImageUrl = v.ImageFile?.ThumbnailCDN ?? v.ImageFile?.CNDUrl,
                Price = price,
                CompareAtPrice = compareAt,
                Weight = v.Weight, Barcode = v.Barcode, IsActive = v.IsActive,
                StockQuantity = stock,
                IsInStock = true,
                DisplayStockQuantity = StockDisplay.ExactCountOrNull(stock),
                VendorProductID = listing?.VendorProductID,
                VendorID = vendor?.VendorID,
                Attributes = v.VariantAttributeValues.Select(a => new ProductAttributeValueDto
                {
                    AttributeDefinitionID = a.AttributeDefinitionID,
                    AttributeName = LocalizedName(a.AttributeDefinition, lang),
                    Unit = a.AttributeDefinition?.Unit,
                    AttributeOptionID = a.AttributeOptionID,
                    OptionValue = LocalizedOptionValue(a.AttributeOption, lang),
                    ColorHex = a.AttributeOption?.ColorHex,
                }).ToList(),
            });
        }

        // Product-level stock = best available across remaining variants / sellers.
        var productStock = StockDisplay.Max(
            variants.Count == 0 ? 0 : StockDisplay.Aggregate(variants.Select(v => v.StockQuantity)),
            sellers.Count == 0 ? 0 : StockDisplay.Aggregate(sellers.Select(s => s.StockQuantity)));

        var summary = await ProjectSummaryAsync(p, lang, currencyCode, ct, vendor, productStock);

        var attributes = p.ProductAttributeValues.OrderBy(a => a.SortOrder).Select(a => new ProductAttributeValueDto
        {
            AttributeDefinitionID = a.AttributeDefinitionID,
            AttributeName = LocalizedName(a.AttributeDefinition, lang),
            Unit = a.AttributeDefinition?.Unit,
            AttributeOptionID = a.AttributeOptionID,
            OptionValue = LocalizedOptionValue(a.AttributeOption, lang),
            ColorHex = a.AttributeOption?.ColorHex,
            CustomValue = LocalizedCustomValue(a, lang),
            NumericValue = a.NumericValue,
            IsFeatured = a.IsFeatured,
        }).ToList();

        var warnings = p.ProductWarnings.Where(w => w.IsActive).Select(w => new ProductWarningDto
        {
            ProductWarningID = w.ProductWarningID, Severity = w.Severity, Text = LocalizedWarningText(w, lang),
        }).ToList();

        var warranties = p.ProductWarranties.Where(w => w.IsActive).OrderBy(w => w.SortOrder)
            .Select(w => ResolveWarrantyDto(w, lang))
            .Where(w => !string.IsNullOrWhiteSpace(w.Title) || !string.IsNullOrWhiteSpace(w.Description))
            .ToList();

        var related = new List<ProductRefDto>();
        foreach (var r in p.ProductRelationProducts.Where(r => r.RelatedProduct.IsActive).OrderBy(r => r.SortOrder))
            related.Add(await ProjectRefAsync(r.RelatedProduct, r.RelationType, lang, currencyCode, ct));

        var gallery = p.ProductMedia
            .OrderBy(m => m.SortOrder)
            .SelectMany(m => m.MediaSet.MediaSetItems.OrderBy(i => i.SortOrder))
            .Select(i => i.File?.ThumbnailCDN ?? i.File?.CNDUrl)
            .Where(u => !string.IsNullOrEmpty(u))
            .Select(u => u!)
            .ToList();

        return new ProductDetailDto
        {
            ProductID = summary.ProductID, Slug = summary.Slug, Title = summary.Title, ShortDescription = summary.ShortDescription,
            FeaturedImageUrl = summary.FeaturedImageUrl, BrandName = summary.BrandName, DefaultSku = summary.DefaultSku,
            MinPrice = summary.MinPrice, AvgRating = summary.AvgRating, RatingCount = summary.RatingCount, HasVariants = summary.HasVariants,
            ProductType = summary.ProductType, RequiresShipping = summary.RequiresShipping, IsUnlimitedStock = summary.IsUnlimitedStock,
            IsInStock = summary.IsInStock, StockQuantity = summary.StockQuantity, DisplayStockQuantity = summary.DisplayStockQuantity,
            VendorID = summary.VendorID, VendorName = summary.VendorName, VendorProductID = summary.VendorProductID,
            Categories = categories, Variants = variants, Sellers = sellers, Attributes = attributes,
            Content = LocalizedContent(p, lang),
            ExpertReview = LocalizedExpertReview(p, lang),
            Warnings = warnings, Warranties = warranties, RelatedProducts = related, GalleryImageUrls = gallery,
            DigitalDownloadUrl = p.DigitalDownloadUrl,
            DigitalServiceUrl = p.DigitalServiceUrl,
            DigitalDeliveryNote = p.DigitalDeliveryNote,
        };
    }

    /// <summary>Maps internal available stock (int.MaxValue for unlimited) to public -1 convention.</summary>
    private static int PublicStock(int available) => available == int.MaxValue ? -1 : available;

    private static ProductWarrantyDto ResolveWarrantyDto(ProductWarranty w, string? lang)
    {
        string? title = null;
        string? description = null;
        string? provider = null;
        int? duration = null;
        int? catalogId = w.WarrantyID;

        if (w.Warranty is { } cat && cat.IsActive)
        {
            duration = cat.DurationMonths;
            title = cat.Title;
            description = cat.Description;
            provider = cat.ProviderName;
            if (!string.IsNullOrWhiteSpace(lang))
            {
                var t = cat.WarrantyTranslations.FirstOrDefault(x =>
                    string.Equals(x.LanguageCode, lang, StringComparison.OrdinalIgnoreCase));
                if (t is not null)
                {
                    if (!string.IsNullOrWhiteSpace(t.Title)) title = t.Title;
                    if (!string.IsNullOrWhiteSpace(t.Description)) description = t.Description;
                    if (!string.IsNullOrWhiteSpace(t.ProviderName)) provider = t.ProviderName;
                }
            }
        }

        // Custom fields win for title/description when provided (one-off seller warranties or notes).
        if (!string.IsNullOrWhiteSpace(w.CustomTitle)) title = w.CustomTitle;
        if (!string.IsNullOrWhiteSpace(w.CustomDescription))
            description = string.IsNullOrWhiteSpace(description)
                ? w.CustomDescription
                : $"{description}\n{w.CustomDescription}";

        return new ProductWarrantyDto
        {
            ProductWarrantyID = w.ProductWarrantyID,
            WarrantyID = catalogId,
            Title = title ?? string.Empty,
            Description = description,
            ProviderName = provider,
            DurationMonths = duration,
        };
    }

    private static (string Title, string Slug, string? ShortDescription) LocalizedCore(Product p, string? lang)
    {
        if (!string.IsNullOrWhiteSpace(lang))
        {
            var t = p.ProductTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, lang, StringComparison.OrdinalIgnoreCase));
            if (t is not null && !string.IsNullOrWhiteSpace(t.Title))
                return (t.Title, string.IsNullOrWhiteSpace(t.Slug) ? p.Slug : t.Slug,
                    string.IsNullOrWhiteSpace(t.ShortDescription) ? p.ShortDescription : t.ShortDescription);
        }
        return (p.Title, p.Slug, p.ShortDescription);
    }

    private static ProductCategoryDto LocalizeCategory(ProductCategory c, string? lang)
    {
        var name = c.Name; var slug = c.Slug;
        if (!string.IsNullOrWhiteSpace(lang))
        {
            var t = c.ProductCategoryTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, lang, StringComparison.OrdinalIgnoreCase));
            if (t is not null && !string.IsNullOrWhiteSpace(t.Name)) { name = t.Name; slug = string.IsNullOrWhiteSpace(t.Slug) ? c.Slug : t.Slug; }
        }
        return new ProductCategoryDto { ProductCategoryID = c.ProductCategoryID, ParentCategoryID = c.ParentCategoryID, Name = name, Slug = slug, SortOrder = c.SortOrder };
    }

    private static string LocalizedName(AttributeDefinition? a, string? lang)
    {
        if (a is null) return string.Empty;
        if (!string.IsNullOrWhiteSpace(lang))
        {
            var t = a.AttributeDefinitionTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, lang, StringComparison.OrdinalIgnoreCase));
            if (t is not null && !string.IsNullOrWhiteSpace(t.Name)) return t.Name;
        }
        return a.Name;
    }

    private static string? LocalizedOptionValue(AttributeOption? o, string? lang)
    {
        if (o is null) return null;
        if (!string.IsNullOrWhiteSpace(lang))
        {
            var t = o.AttributeOptionTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, lang, StringComparison.OrdinalIgnoreCase));
            if (t is not null && !string.IsNullOrWhiteSpace(t.Value)) return t.Value;
        }
        return o.Value;
    }

    private static string? LocalizedCustomValue(ProductAttributeValue a, string? lang)
    {
        if (!string.IsNullOrWhiteSpace(lang))
        {
            var t = a.ProductAttributeValueTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, lang, StringComparison.OrdinalIgnoreCase));
            if (t is not null && !string.IsNullOrWhiteSpace(t.CustomValue)) return t.CustomValue;
        }
        return a.CustomValue;
    }

    private static string? LocalizedContent(Product p, string? lang)
    {
        if (!string.IsNullOrWhiteSpace(lang))
        {
            var t = p.ProductTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, lang, StringComparison.OrdinalIgnoreCase));
            if (t is not null && !string.IsNullOrWhiteSpace(t.Content)) return t.Content;
        }
        return p.Content;
    }

    private static string? LocalizedExpertReview(Product p, string? lang)
    {
        if (!string.IsNullOrWhiteSpace(lang))
        {
            var t = p.ProductTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, lang, StringComparison.OrdinalIgnoreCase));
            if (t is not null && !string.IsNullOrWhiteSpace(t.ExpertReview)) return t.ExpertReview;
        }
        return p.ExpertReview;
    }

    private static string LocalizedWarningText(ProductWarning w, string? lang)
    {
        if (!string.IsNullOrWhiteSpace(lang))
        {
            var t = w.ProductWarningTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, lang, StringComparison.OrdinalIgnoreCase));
            if (t is not null && !string.IsNullOrWhiteSpace(t.Text)) return t.Text;
        }
        return w.Text;
    }
}
