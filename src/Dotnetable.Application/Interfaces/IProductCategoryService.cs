using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Product categories (nested tree) with per-language translations. Admin management plus a
/// localized read projection for the public site. Scoped per website.
/// </summary>
public interface IProductCategoryService
{
    // ── Admin management ────────────────────────────────────────────
    Task<List<ProductCategory>> GetAllAsync(int? websiteId, CancellationToken ct = default);
    Task<PagedResult<ProductCategory>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);
    Task<ProductCategory?> GetByIdAsync(int productCategoryId, CancellationToken ct = default);
    Task<ProductCategory> CreateAsync(ProductCategory category, CancellationToken ct = default);
    Task UpdateAsync(ProductCategory category, CancellationToken ct = default);
    Task DeleteAsync(int productCategoryId, CancellationToken ct = default);

    // ── Translations ────────────────────────────────────────────────
    Task<List<ProductCategoryTranslation>> GetTranslationsAsync(int productCategoryId, CancellationToken ct = default);
    Task SetTranslationsAsync(int productCategoryId, IReadOnlyDictionary<string, (string Name, string Slug)> byLanguage, CancellationToken ct = default);

    // ── Category ↔ attribute definitions ────────────────────────────

    /// <summary>
    /// Attribute definition IDs mapped to this category only (no ancestors), ordered by SortOrder.
    /// </summary>
    Task<List<int>> GetAttributeDefinitionIdsAsync(int productCategoryId, CancellationToken ct = default);

    /// <summary>
    /// Replace the attribute set for a category. IDs are stored in the given order as SortOrder.
    /// Only product-level (non-variant) attributes should be assigned.
    /// </summary>
    Task SetAttributeDefinitionIdsAsync(int productCategoryId, IReadOnlyList<int> attributeDefinitionIds, CancellationToken ct = default);

    /// <summary>
    /// Product-level attribute definitions for a category, optionally merged with parent categories
    /// (child mappings override sort; parents fill in missing ones). Used by product edit and storefront filters.
    /// </summary>
    Task<List<AttributeDefinition>> GetAttributesForCategoryAsync(
        int productCategoryId, bool includeAncestors = true, CancellationToken ct = default);

    // ── Public read ─────────────────────────────────────────────────

    /// <summary>Active categories for a website as a localized tree.</summary>
    Task<List<ProductCategoryDto>> GetTreeAsync(int websiteId, string? languageCode = null, CancellationToken ct = default);

    /// <summary>A single active category by slug (matches the base slug or any translated slug).</summary>
    Task<ProductCategoryDto?> GetBySlugAsync(int websiteId, string slug, string? languageCode = null, CancellationToken ct = default);

    /// <summary>
    /// Filterable product-level attributes (with options) for a category slug — for storefront facet filters.
    /// Includes ancestor category attributes.
    /// </summary>
    Task<List<CategoryAttributeFilterDto>> GetFilterableAttributesBySlugAsync(
        int websiteId, string categorySlug, string? languageCode = null, CancellationToken ct = default);
}
