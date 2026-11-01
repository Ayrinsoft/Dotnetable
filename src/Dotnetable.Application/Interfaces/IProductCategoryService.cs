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

    /// <summary>One level of the tree. Does not load the rest of the catalog.</summary>
    Task<IReadOnlyList<ProductCategoryPickDto>> GetChildrenAsync(
        int websiteId, int? parentCategoryId, string? search, int take = 40, CancellationToken ct = default);

    /// <summary>Name search with a breadcrumb, capped so a 5-level taxonomy stays usable.</summary>
    Task<IReadOnlyList<ProductCategoryPickDto>> SearchAsync(
        int websiteId, string term, int take = 25, CancellationToken ct = default);

    Task<IReadOnlyList<ProductCategoryPickDto>> GetAncestorChainAsync(int productCategoryId, CancellationToken ct = default);

    Task<int> CountAsync(int websiteId, bool activeOnly = true, CancellationToken ct = default);

    Task SetActiveAsync(int productCategoryId, bool isActive, CancellationToken ct = default);

    /// <summary>Removes the category from its parent and leaves it as its own root.</summary>
    Task DetachFromParentAsync(int productCategoryId, CancellationToken ct = default);

    /// <summary>Deletes the category and every category under it when no product uses them.</summary>
    Task DeleteBranchAsync(int productCategoryId, CancellationToken ct = default);

    /// <summary>Whether this storefront limits its catalog, and which category ids are checked.</summary>
    Task<WebsiteCategoryVisibilityDto> GetVisibilityAsync(int websiteId, CancellationToken ct = default);

    /// <summary>
    /// Saves the storefront allow-list. <paramref name="limitToSelection"/> false shows the whole catalog.
    /// Ids that are not on this website's catalog are ignored.
    /// </summary>
    Task SetVisibilityAsync(int websiteId, bool limitToSelection, IReadOnlyList<int> categoryIds, CancellationToken ct = default);

    /// <summary>The category and every active category under it, for a "show this branch" action.</summary>
    Task<IReadOnlyList<int>> GetBranchIdsAsync(int websiteId, int categoryId, CancellationToken ct = default);
}
