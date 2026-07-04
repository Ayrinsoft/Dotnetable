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
    Task<ProductCategory?> GetByIdAsync(int productCategoryId, CancellationToken ct = default);
    Task<ProductCategory> CreateAsync(ProductCategory category, CancellationToken ct = default);
    Task UpdateAsync(ProductCategory category, CancellationToken ct = default);
    Task DeleteAsync(int productCategoryId, CancellationToken ct = default);

    // ── Translations ────────────────────────────────────────────────
    Task<List<ProductCategoryTranslation>> GetTranslationsAsync(int productCategoryId, CancellationToken ct = default);
    Task SetTranslationsAsync(int productCategoryId, IReadOnlyDictionary<string, (string Name, string Slug)> byLanguage, CancellationToken ct = default);

    // ── Public read ─────────────────────────────────────────────────

    /// <summary>Active categories for a website as a localized tree.</summary>
    Task<List<ProductCategoryDto>> GetTreeAsync(int websiteId, string? languageCode = null, CancellationToken ct = default);

    /// <summary>A single active category by slug (matches the base slug or any translated slug).</summary>
    Task<ProductCategoryDto?> GetBySlugAsync(int websiteId, string slug, string? languageCode = null, CancellationToken ct = default);
}
