using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Post categories (optionally nested and tied to a post type) with per-language translations.
/// Admin management plus a localized read projection for the public site. Scoped per website.
/// </summary>
public interface ICategoryService
{
    // ── Admin management ────────────────────────────────────────────
    Task<List<Category>> GetAllAsync(int? websiteId, CancellationToken ct = default);
    Task<PagedResult<Category>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);
    Task<Category?> GetByIdAsync(int categoryId, CancellationToken ct = default);
    Task<Category> CreateAsync(Category category, CancellationToken ct = default);
    Task UpdateAsync(Category category, CancellationToken ct = default);
    Task DeleteAsync(int categoryId, CancellationToken ct = default);

    // ── Translations ────────────────────────────────────────────────
    Task<List<CategoryTranslation>> GetTranslationsAsync(int categoryId, CancellationToken ct = default);
    /// <summary>Saves name + slug per language; existing translated summaries are left as they are.</summary>
    Task SetTranslationsAsync(int categoryId, IReadOnlyDictionary<string, (string Name, string Slug)> byLanguage, CancellationToken ct = default);

    /// <summary>Saves name, slug and summary per language (key = language code).</summary>
    Task SaveTranslationsAsync(int categoryId, IReadOnlyDictionary<string, CategoryTranslationInput> byLanguage, CancellationToken ct = default);

    // ── Public read ─────────────────────────────────────────────────

    /// <summary>Active categories for a website as a localized tree, optionally filtered by post type.</summary>
    Task<List<CategoryDto>> GetTreeAsync(int websiteId, int? postTypeId = null, string? languageCode = null, CancellationToken ct = default);

    /// <summary>A single active category by slug (matches the base slug or any translated slug).</summary>
    Task<CategoryDto?> GetBySlugAsync(int websiteId, string slug, string? languageCode = null, CancellationToken ct = default);
}
