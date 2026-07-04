using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Products with variants, categories, media, attributes, content sections, warnings and related
/// products. Admin management plus localized/priced read projections for the public site. Scoped per website.
/// </summary>
public interface IProductService
{
    // ── Admin management ────────────────────────────────────────────
    Task<PagedResult<ProductListItemDto>> GetPagedAsync(int? websiteId, ProductFilter filter, GridQuery query, string? search, CancellationToken ct = default);

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
    Task SetVariantsAsync(int productId, IReadOnlyList<ProductVariant> variants, CancellationToken ct = default);

    // ── Media (ordered ProductMedium rows referencing MediaSet ids) ──
    Task SetMediaAsync(int productId, IReadOnlyList<int> mediaSetIds, CancellationToken ct = default);

    /// <summary>Wraps a single already-uploaded file in a new single-item MediaSet, for use as a gallery entry.</summary>
    Task<int> CreateSimpleMediaSetAsync(int websiteId, int fileId, CancellationToken ct = default);

    // ── Attribute values (product-level, non-variant) ────────────────
    Task SetAttributeValuesAsync(int productId, IReadOnlyList<ProductAttributeValue> values, CancellationToken ct = default);

    // ── Content sections ──────────────────────────────────────────────
    Task SetContentSectionsAsync(int productId, IReadOnlyList<ProductContentSection> sections, CancellationToken ct = default);

    // ── Warnings ───────────────────────────────────────────────────────
    Task SetWarningsAsync(int productId, IReadOnlyList<ProductWarning> warnings, CancellationToken ct = default);

    // ── Related products ────────────────────────────────────────────────
    Task SetRelatedProductsAsync(int productId, IReadOnlyList<(int RelatedProductID, byte RelationType, int SortOrder)> related, CancellationToken ct = default);

    // ── Public read ─────────────────────────────────────────────────

    /// <summary>Published/active products for a website (paged), optionally filtered by category/brand slug or search text, priced in the requested display currency.</summary>
    Task<PagedResult<ProductSummaryDto>> GetPublishedAsync(
        int websiteId, string? categorySlug, string? brandSlug, string? search,
        decimal? minPriceUsd, decimal? maxPriceUsd,
        int pageIndex, int pageSize, string? languageCode = null, string? currencyCode = null, CancellationToken ct = default);

    /// <summary>A single published product by slug (base or translated), fully detailed and priced.</summary>
    Task<ProductDetailDto?> GetBySlugAsync(int websiteId, string slug, string? languageCode = null, string? currencyCode = null, CancellationToken ct = default);

    /// <summary>Products explicitly related to (or auto-derived from the category of) the given product.</summary>
    Task<List<ProductRefDto>> GetRelatedAsync(int websiteId, string slug, int take, string? languageCode = null, string? currencyCode = null, CancellationToken ct = default);
}
