using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Products with variants, categories, media, attributes, content, expert review, warnings,
/// warranties, price history and related products. Admin management plus localized/priced read
/// projections for the public site. Scoped per website.
/// </summary>
public interface IProductService
{
    // ── Admin management ────────────────────────────────────────────
    /// <param name="createdByMemberId">When set (vendor member login), only products created by that member are returned.</param>
    Task<PagedResult<ProductListItemDto>> GetPagedAsync(int? websiteId, ProductFilter filter, GridQuery query, string? search, int? createdByMemberId = null, CancellationToken ct = default);

    /// <summary>A single product with everything loaded (variants, categories, media, attributes, content, warnings, relations, translations) for the edit form.</summary>
    Task<Product?> GetByIdAsync(int productId, CancellationToken ct = default);

    Task<Product> CreateAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(int productId, CancellationToken ct = default);
    Task SetActiveAsync(int productId, bool active, CancellationToken ct = default);

    // ── Translations ────────────────────────────────────────────────
    Task<List<ProductTranslation>> GetTranslationsAsync(int productId, CancellationToken ct = default);
    Task SetTranslationsAsync(int productId, IReadOnlyList<ProductTranslation> translations, CancellationToken ct = default);

    // ── Categories ───────────────────────────────────────────────────
    Task<List<int>> GetCategoryIdsAsync(int productId, CancellationToken ct = default);
    Task SetCategoriesAsync(int productId, IReadOnlyList<int> categoryIds, int? primaryCategoryId, CancellationToken ct = default);

    // ── Variants (replace-all-children pattern; ProductVariantID == 0 means insert) ──
    /// <summary>
    /// Replaces product variants. Optionally syncs variant attribute options (one option per definition).
    /// When <paramref name="variantAttributeOptions"/> is provided, it is aligned by index with
    /// <paramref name="variants"/>; each entry is a list of (AttributeDefinitionID, AttributeOptionID).
    /// When null, falls back to each variant's <see cref="ProductVariant.VariantAttributeValues"/> collection.
    /// Empty / zero option IDs are ignored; omitted definitions are removed for that variant.
    /// </summary>
    /// <param name="changedByMemberId">Optional admin member that applied the price change (recorded in price history).</param>
    Task SetVariantsAsync(
        int productId,
        IReadOnlyList<ProductVariant> variants,
        int? changedByMemberId = null,
        IReadOnlyList<IReadOnlyList<(int AttributeDefinitionID, int AttributeOptionID)>>? variantAttributeOptions = null,
        CancellationToken ct = default);

    /// <summary>
    /// Price history for a variant, newest first. Defaults to the last 12 months.
    /// </summary>
    Task<List<ProductVariantPriceHistoryDto>> GetVariantPriceHistoryAsync(
        int productVariantId, int months = 12, CancellationToken ct = default);

    // ── Media (ordered ProductMedium rows referencing MediaSet ids) ──
    Task SetMediaAsync(int productId, IReadOnlyList<int> mediaSetIds, CancellationToken ct = default);

    /// <summary>Wraps a single already-uploaded file in a new single-item MediaSet, for use as a gallery entry.</summary>
    Task<int> CreateSimpleMediaSetAsync(int websiteId, int fileId, CancellationToken ct = default);

    // ── Attribute values (product-level, non-variant) ────────────────
    Task SetAttributeValuesAsync(int productId, IReadOnlyList<ProductAttributeValue> values, CancellationToken ct = default);

    // ── Warnings ───────────────────────────────────────────────────────
    Task SetWarningsAsync(int productId, IReadOnlyList<ProductWarning> warnings, CancellationToken ct = default);

    // ── Warranties (catalog pick and/or free-text custom) ───────────────
    Task SetWarrantiesAsync(int productId, IReadOnlyList<ProductWarranty> warranties, CancellationToken ct = default);

    // ── Related products ────────────────────────────────────────────────
    Task SetRelatedProductsAsync(int productId, IReadOnlyList<(int RelatedProductID, byte RelationType, int SortOrder)> related, CancellationToken ct = default);

    // ── Public read ─────────────────────────────────────────────────

    /// <summary>
    /// Published/active products for a website (paged), optionally filtered by category/brand slug, search,
    /// price, and availability. Priced in the requested display currency.
    /// </summary>
    /// <param name="inStock">
    /// When true, only products with available store-listing stock &gt; 0.
    /// When false, only out-of-stock products. When null, no availability filter.
    /// </param>
    Task<PagedResult<ProductSummaryDto>> GetPublishedAsync(
        int websiteId, string? categorySlug, string? brandSlug, string? search,
        decimal? minPriceUsd, decimal? maxPriceUsd,
        int pageIndex, int pageSize, string? languageCode = null, string? currencyCode = null,
        bool? inStock = null, IReadOnlyList<int>? attributeOptionIds = null, CancellationToken ct = default);

    /// <summary>A single published product by slug (base or translated), fully detailed and priced.</summary>
    Task<ProductDetailDto?> GetBySlugAsync(int websiteId, string slug, string? languageCode = null, string? currencyCode = null, CancellationToken ct = default);

    /// <summary>
    /// Admin product preview: full detail projection by product id, including drafts / inactive /
    /// out-of-stock variants (not filtered by publish status).
    /// </summary>
    Task<ProductDetailDto?> GetDetailByIdAsync(int productId, string? languageCode = null, string? currencyCode = null, CancellationToken ct = default);

    /// <summary>Products explicitly related to (or auto-derived from the category of) the given product.</summary>
    Task<List<ProductRefDto>> GetRelatedAsync(int websiteId, string slug, int take, string? languageCode = null, string? currencyCode = null, CancellationToken ct = default);
}
