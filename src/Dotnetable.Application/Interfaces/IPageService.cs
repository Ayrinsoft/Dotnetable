using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// CMS pages (optionally nested) with per-language translations. Admin management plus
/// localized read projections for the public site. Scoped per website.
/// </summary>
public interface IPageService
{
    // ── Admin management ────────────────────────────────────────────
    Task<PagedResult<Page>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);
    Task<List<Page>> GetAllAsync(int? websiteId, CancellationToken ct = default);
    Task<Page?> GetByIdAsync(int pageId, CancellationToken ct = default);
    Task<Page> CreateAsync(Page page, CancellationToken ct = default);
    Task UpdateAsync(Page page, CancellationToken ct = default);
    Task DeleteAsync(int pageId, CancellationToken ct = default);
    Task SetActiveAsync(int pageId, bool active, CancellationToken ct = default);

    // ── Translations ────────────────────────────────────────────────
    Task<List<PageTranslation>> GetTranslationsAsync(int pageId, CancellationToken ct = default);
    Task SetTranslationsAsync(int pageId, IReadOnlyList<PageTranslation> translations, CancellationToken ct = default);

    // ── Public read ─────────────────────────────────────────────────
    Task<List<PageDto>> GetTreeAsync(int websiteId, string? languageCode = null, CancellationToken ct = default);
    Task<PageDto?> GetBySlugAsync(int websiteId, string slug, string? languageCode = null, CancellationToken ct = default);
    Task<PageDto?> GetHomepageAsync(int websiteId, string? languageCode = null, CancellationToken ct = default);
}
