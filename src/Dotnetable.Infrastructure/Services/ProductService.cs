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
            q = q.Where(p => p.CreatedByMemberId == memberId);
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
            MinPriceUsd = p.ProductVariants.Count == 0 ? null : p.ProductVariants.Min(v => v.ReferencePriceUsd),
            FeaturedImageUrl = p.FeaturedImageFile?.ThumbnailCDN ?? p.FeaturedImageFile?.CNDUrl,
            UpdatedAt = p.UpdatedAt,
        }).ToList();

        return new PagedResult<ProductListItemDto> { Items = dtos, TotalCount = total };
    }

    public async Task<Product?> GetByIdAsync(int productId, CancellationToken ct = default) =>
        await _context.Products
            .Include(p => p.ProductTranslations)
            .Include(p => p.ProductVariants).ThenInclude(v => v.VariantAttributeValues)
            .Include(p => p.ProductCategoryMaps)
            .Include(p => p.ProductMedia).ThenInclude(m => m.MediaSet).ThenInclude(ms => ms.MediaSetItems).ThenInclude(i => i.File)
            .Include(p => p.ProductAttributeValues).ThenInclude(v => v.AttributeDefinition)
            .Include(p => p.ProductAttributeValues).ThenInclude(v => v.AttributeOption)
            .Include(p => p.ProductContentSections).ThenInclude(s => s.ProductContentSectionTranslations)
            .Include(p => p.ProductContentSections).ThenInclude(s => s.File)
            .Include(p => p.ProductWarnings).ThenInclude(w => w.ProductWarningTranslations)
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
        product.CreatedAt = product.UpdatedAt = DateTime.UtcNow;
        _context.Products.Add(product);
        await _context.SaveChangesAsync(ct);
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        product.UpdatedAt = DateTime.UtcNow;
        _context.Products.Update(product);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int productId, CancellationToken ct = default)
    {
        var product = await _context.Products
            .Include(p => p.ProductTranslations)
            .Include(p => p.ProductVariants)
            .Include(p => p.ProductCategoryMaps)
            .Include(p => p.ProductMedia)
            .Include(p => p.ProductAttributeValues)
            .Include(p => p.ProductContentSections)
            .Include(p => p.ProductWarnings)
            .Include(p => p.ProductRelationProducts)
            .Include(p => p.ProductRelationRelatedProducts)
            .Include(p => p.MenuItems)
            .FirstOrDefaultAsync(p => p.ProductID == productId, ct);
        if (product is null) return;

        foreach (var mi in product.MenuItems)
            mi.ProductID = null;

        _context.ProductRelations.RemoveRange(product.ProductRelationProducts);
        _context.ProductRelations.RemoveRange(product.ProductRelationRelatedProducts);
        _context.ProductWarnings.RemoveRange(product.ProductWarnings);
        _context.ProductContentSections.RemoveRange(product.ProductContentSections);
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
                });
            else
            {
                current.Title = t.Title.Trim();
                current.Slug = slug;
                current.ShortDescription = t.ShortDescription;
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

    public async Task SetVariantsAsync(int productId, IReadOnlyList<ProductVariant> variants, CancellationToken ct = default)
    {
        var product = await _context.Products.AsNoTracking()
            .Where(p => p.ProductID == productId).Select(p => new { p.WebsiteID }).FirstOrDefaultAsync(ct);
        if (product is null) return;

        var existing = await _context.ProductVariants.Where(v => v.ProductID == productId).ToListAsync(ct);
        var wantedIds = variants.Where(v => v.ProductVariantID != 0).Select(v => v.ProductVariantID).ToHashSet();

        var toRemove = existing.Where(v => !wantedIds.Contains(v.ProductVariantID)).ToList();
        if (toRemove.Count > 0)
        {
            var removeIds = toRemove.Select(v => v.ProductVariantID).ToList();
            _context.VariantAttributeValues.RemoveRange(
                _context.VariantAttributeValues.Where(a => removeIds.Contains(a.ProductVariantID)));
            _context.ProductVariants.RemoveRange(toRemove);
        }

        foreach (var v in variants)
        {
            if (v.ProductVariantID == 0)
                _context.ProductVariants.Add(new ProductVariant
                {
                    WebsiteID = product.WebsiteID,
                    ProductID = productId,
                    Sku = v.Sku,
                    IsDefault = v.IsDefault,
                    ImageFileID = v.ImageFileID,
                    ReferencePriceUsd = v.ReferencePriceUsd,
                    CompareAtPriceUsd = v.CompareAtPriceUsd,
                    OverridePrice = v.OverridePrice,
                    Weight = v.Weight,
                    Barcode = v.Barcode,
                    IsActive = v.IsActive,
                    CreatedAt = DateTime.UtcNow,
                });
            else
            {
                var current = existing.FirstOrDefault(x => x.ProductVariantID == v.ProductVariantID);
                if (current is null) continue;
                current.Sku = v.Sku;
                current.IsDefault = v.IsDefault;
                current.ImageFileID = v.ImageFileID;
                current.ReferencePriceUsd = v.ReferencePriceUsd;
                current.CompareAtPriceUsd = v.CompareAtPriceUsd;
                current.OverridePrice = v.OverridePrice;
                current.Weight = v.Weight;
                current.Barcode = v.Barcode;
                current.IsActive = v.IsActive;
            }
        }

        await _context.SaveChangesAsync(ct);
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
        var existing = await _context.ProductAttributeValues
            .Include(v => v.ProductAttributeValueTranslations)
            .Where(v => v.ProductID == productId).ToListAsync(ct);
        var wantedIds = values.Where(v => v.ProductAttributeValueID != 0).Select(v => v.ProductAttributeValueID).ToHashSet();

        var toRemove = existing.Where(v => !wantedIds.Contains(v.ProductAttributeValueID)).ToList();
        foreach (var r in toRemove)
            _context.ProductAttributeValueTranslations.RemoveRange(r.ProductAttributeValueTranslations);
        _context.ProductAttributeValues.RemoveRange(toRemove);

        var sortOrder = 0;
        foreach (var v in values)
        {
            if (v.ProductAttributeValueID == 0)
                _context.ProductAttributeValues.Add(new ProductAttributeValue
                {
                    ProductID = productId,
                    AttributeDefinitionID = v.AttributeDefinitionID,
                    AttributeOptionID = v.AttributeOptionID,
                    CustomValue = v.CustomValue,
                    NumericValue = v.NumericValue,
                    IsFeatured = v.IsFeatured,
                    SortOrder = sortOrder++,
                });
            else
            {
                var current = existing.FirstOrDefault(x => x.ProductAttributeValueID == v.ProductAttributeValueID);
                if (current is null) continue;
                current.AttributeDefinitionID = v.AttributeDefinitionID;
                current.AttributeOptionID = v.AttributeOptionID;
                current.CustomValue = v.CustomValue;
                current.NumericValue = v.NumericValue;
                current.IsFeatured = v.IsFeatured;
                current.SortOrder = sortOrder++;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    // ── Content sections ─────────────────────────────────────────────────

    public async Task SetContentSectionsAsync(int productId, IReadOnlyList<ProductContentSection> sections, CancellationToken ct = default)
    {
        var existing = await _context.ProductContentSections
            .Include(s => s.ProductContentSectionTranslations)
            .Where(s => s.ProductID == productId).ToListAsync(ct);
        var wantedIds = sections.Where(s => s.ProductContentSectionID != 0).Select(s => s.ProductContentSectionID).ToHashSet();

        var toRemove = existing.Where(s => !wantedIds.Contains(s.ProductContentSectionID)).ToList();
        foreach (var r in toRemove)
            _context.ProductContentSectionTranslations.RemoveRange(r.ProductContentSectionTranslations);
        _context.ProductContentSections.RemoveRange(toRemove);

        var sortOrder = 0;
        foreach (var s in sections)
        {
            if (s.ProductContentSectionID == 0)
                _context.ProductContentSections.Add(new ProductContentSection
                {
                    ProductID = productId,
                    SectionType = s.SectionType,
                    HtmlContent = s.HtmlContent,
                    FileId = s.FileId,
                    MediaSetID = s.MediaSetID,
                    SortOrder = sortOrder++,
                    IsActive = s.IsActive,
                });
            else
            {
                var current = existing.FirstOrDefault(x => x.ProductContentSectionID == s.ProductContentSectionID);
                if (current is null) continue;
                current.SectionType = s.SectionType;
                current.HtmlContent = s.HtmlContent;
                current.FileId = s.FileId;
                current.MediaSetID = s.MediaSetID;
                current.SortOrder = sortOrder++;
                current.IsActive = s.IsActive;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

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
            if (w.ProductWarningID == 0)
                _context.ProductWarnings.Add(new ProductWarning
                {
                    ProductID = productId,
                    Severity = w.Severity,
                    Text = w.Text,
                    IsActive = w.IsActive,
                });
            else
            {
                var current = existing.FirstOrDefault(x => x.ProductWarningID == w.ProductWarningID);
                if (current is null) continue;
                current.Severity = w.Severity;
                current.Text = w.Text;
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
        int pageIndex, int pageSize, string? languageCode = null, string? currencyCode = null, CancellationToken ct = default)
    {
        // Own products of the host site.
        var q = PublishedQuery(websiteId);

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

        var localProducts = await q
            .OrderByDescending(p => p.SortOrder).ThenByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        // Site-linked vendors: only products owned by the linked website (never re-shared imports).
        var linked = await LoadLinkedSiteProductsAsync(websiteId, categorySlug, brandSlug, search, minPriceUsd, maxPriceUsd, ct);

        var combined = new List<(Product Product, Vendor? Vendor)>(localProducts.Count + linked.Count);
        combined.AddRange(localProducts.Select(p => ((Product Product, Vendor? Vendor))(p, null)));
        combined.AddRange(linked.Select(x => ((Product Product, Vendor? Vendor))(x.Product, x.Vendor)));

        var total = combined.Count;
        var take = pageSize < 1 ? 12 : pageSize;
        var skip = (pageIndex < 1 ? 0 : pageIndex - 1) * take;
        var page = combined.Skip(skip).Take(take).ToList();

        var items = new List<ProductSummaryDto>(page.Count);
        foreach (var (p, vendor) in page)
        {
            var dto = await ProjectSummaryAsync(p, languageCode, currencyCode, ct, vendor);
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
            .Include(p => p.ProductContentSections).ThenInclude(s => s.ProductContentSectionTranslations)
            .Include(p => p.ProductContentSections).ThenInclude(s => s.File)
            .Include(p => p.ProductWarnings).ThenInclude(w => w.ProductWarningTranslations)
            .Include(p => p.ProductRelationProducts).ThenInclude(r => r.RelatedProduct).ThenInclude(rp => rp.FeaturedImageFile)
            .Include(p => p.ProductRelationProducts).ThenInclude(r => r.RelatedProduct).ThenInclude(rp => rp.ProductVariants)
            .Include(p => p.ProductMedia).ThenInclude(m => m.MediaSet).ThenInclude(ms => ms.MediaSetItems).ThenInclude(i => i.File);

    /// <summary>
    /// Products owned by linked websites (Product.WebsiteID == LinkedWebsiteID only — never re-share).
    /// </summary>
    private async Task<List<(Product Product, Vendor Vendor)>> LoadLinkedSiteProductsAsync(
        int hostWebsiteId, string? categorySlug, string? brandSlug, string? search,
        decimal? minPriceUsd, decimal? maxPriceUsd, CancellationToken ct)
    {
        var siteVendors = await _vendors.GetActiveSiteLinksAsync(hostWebsiteId, ct);
        var result = new List<(Product, Vendor)>();
        foreach (var vendor in siteVendors)
        {
            if (vendor.LinkedWebsiteID is not int lid) continue;
            var q = PublishedQuery(lid);
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

            var products = await q
                .OrderByDescending(p => p.SortOrder).ThenByDescending(p => p.CreatedAt)
                .ToListAsync(ct);
            foreach (var p in products)
                result.Add((p, vendor));
        }
        return result;
    }

    // ── Projection helpers ──────────────────────────────────────────

    private async Task<ProductSummaryDto> ProjectSummaryAsync(Product p, string? lang, string? currencyCode, CancellationToken ct, Vendor? vendor = null)
    {
        var (title, slug, shortDescription) = LocalizedCore(p, lang);
        var activeVariants = p.ProductVariants.Where(v => v.IsActive).ToList();
        var minUsd = activeVariants.Count == 0 ? 0m : activeVariants.Min(v => v.ReferencePriceUsd);
        var defaultVariant = activeVariants.FirstOrDefault(v => v.IsDefault) ?? activeVariants.FirstOrDefault();

        // Host display currency; pricing conversion uses the host website when sold via a vendor.
        var priceWebsiteId = vendor?.WebsiteID ?? p.WebsiteID;
        if (vendor is not null)
            slug = $"{vendor.Slug}--{slug}";

        return new ProductSummaryDto
        {
            ProductID = p.ProductID, Slug = slug, Title = title, ShortDescription = shortDescription,
            FeaturedImageUrl = p.FeaturedImageFile?.ThumbnailCDN ?? p.FeaturedImageFile?.CNDUrl,
            BrandName = p.Brand?.Name,
            DefaultSku = defaultVariant?.Sku,
            MinPrice = await _currency.ToDisplayAsync(priceWebsiteId, minUsd, currencyCode, ct),
            AvgRating = p.AvgRating, RatingCount = p.RatingCount, HasVariants = p.HasVariants,
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
            minPrice = await _currency.ToDisplayAsync(p.WebsiteID, activeVariants.Min(v => v.ReferencePriceUsd), currencyCode, ct);

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
        var summary = await ProjectSummaryAsync(p, lang, currencyCode, ct, vendor);
        var priceWebsiteId = hostWebsiteId ?? vendor?.WebsiteID ?? p.WebsiteID;

        var categories = p.ProductCategoryMaps
            .Where(m => m.ProductCategory is not null)
            .Select(m => LocalizeCategory(m.ProductCategory, lang))
            .ToList();

        // Optional host vendor-product overrides for price/stock.
        Dictionary<int, VendorProduct>? listings = null;
        if (vendor is not null)
        {
            listings = await _context.VendorProducts.AsNoTracking()
                .Where(vp => vp.VendorID == vendor.VendorID && vp.IsActive)
                .ToDictionaryAsync(vp => vp.ProductVariantID, ct);
        }

        var variants = new List<ProductVariantDto>(p.ProductVariants.Count);
        foreach (var v in p.ProductVariants.Where(v => v.IsActive))
        {
            VendorProduct? listing = null;
            if (listings is not null)
                listings.TryGetValue(v.ProductVariantID, out listing);
            var unitUsd = listing?.OverridePrice ?? listing?.ReferencePriceUsd ?? v.ReferencePriceUsd;
            var stock = listing is not null
                ? listing.StockQuantity
                : v.InventoryItems.Where(i => i.WebsiteID == p.WebsiteID).Sum(i => i.QuantityOnHand - i.QuantityReserved);

            variants.Add(new ProductVariantDto
            {
                ProductVariantID = v.ProductVariantID, Sku = v.Sku, IsDefault = v.IsDefault,
                ImageUrl = v.ImageFile?.ThumbnailCDN ?? v.ImageFile?.CNDUrl,
                Price = await _currency.ToDisplayAsync(priceWebsiteId, unitUsd, currencyCode, ct),
                CompareAtPrice = v.CompareAtPriceUsd is decimal cmp ? await _currency.ToDisplayAsync(priceWebsiteId, cmp, currencyCode, ct) : null,
                Weight = v.Weight, Barcode = v.Barcode, IsActive = v.IsActive,
                StockQuantity = stock,
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

        var contentSections = p.ProductContentSections.Where(s => s.IsActive).OrderBy(s => s.SortOrder).Select(s => new ProductContentSectionDto
        {
            ProductContentSectionID = s.ProductContentSectionID, SectionType = s.SectionType,
            HtmlContent = LocalizedHtml(s, lang),
            FileUrl = s.File?.ThumbnailCDN ?? s.File?.CNDUrl,
            MediaSetID = s.MediaSetID, SortOrder = s.SortOrder,
        }).ToList();

        var warnings = p.ProductWarnings.Where(w => w.IsActive).Select(w => new ProductWarningDto
        {
            ProductWarningID = w.ProductWarningID, Severity = w.Severity, Text = LocalizedWarningText(w, lang),
        }).ToList();

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
            Categories = categories, Variants = variants, Attributes = attributes,
            ContentSections = contentSections, Warnings = warnings, RelatedProducts = related, GalleryImageUrls = gallery,
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

    private static string? LocalizedHtml(ProductContentSection s, string? lang)
    {
        if (!string.IsNullOrWhiteSpace(lang))
        {
            var t = s.ProductContentSectionTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, lang, StringComparison.OrdinalIgnoreCase));
            if (t is not null && !string.IsNullOrWhiteSpace(t.HtmlContent)) return t.HtmlContent;
        }
        return s.HtmlContent;
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
