using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Posts with per-language translations, category and tag assignments. Admin management plus
/// localized read projections for the public site. Scoped per website.
/// </summary>
public interface IPostService
{
    // ── Admin management ────────────────────────────────────────────
    Task<PagedResult<Post>> GetPagedAsync(int? websiteId, PostFilter filter, GridQuery query, CancellationToken ct = default);

    /// <summary>A single post with its translations, categories and tags loaded (for the edit form).</summary>
    Task<Post?> GetByIdAsync(int postId, CancellationToken ct = default);

    Task<Post> CreateAsync(Post post, CancellationToken ct = default);
    Task UpdateAsync(Post post, CancellationToken ct = default);
    Task DeleteAsync(int postId, CancellationToken ct = default);
    Task SetActiveAsync(int postId, bool active, CancellationToken ct = default);

    // ── Translations ────────────────────────────────────────────────
    Task<List<PostTranslation>> GetTranslationsAsync(int postId, CancellationToken ct = default);
    Task SetTranslationsAsync(int postId, IReadOnlyList<PostTranslation> translations, CancellationToken ct = default);

    // ── Category / tag assignment ───────────────────────────────────
    Task<List<int>> GetCategoryIdsAsync(int postId, CancellationToken ct = default);
    Task SetCategoriesAsync(int postId, IReadOnlyList<int> categoryIds, int? primaryCategoryId, CancellationToken ct = default);
    Task<List<int>> GetTagIdsAsync(int postId, CancellationToken ct = default);
    Task SetTagsAsync(int postId, IReadOnlyList<int> tagIds, CancellationToken ct = default);

    // ── Public read ─────────────────────────────────────────────────

    /// <summary>Published posts for a website (paged, newest first), optionally filtered by post type,
    /// category slug or tag slug — all localized to <paramref name="languageCode"/>.</summary>
    Task<PagedResult<PostSummaryDto>> GetPublishedAsync(
        int websiteId, string? postTypeSlug, string? categorySlug, string? tagSlug,
        int pageIndex, int pageSize, string? languageCode = null, CancellationToken ct = default);

    /// <summary>A single published post by slug (base slug or a translated slug). Increments the view count.</summary>
    Task<PostDetailDto?> GetBySlugAsync(int websiteId, string slug, string? languageCode = null, CancellationToken ct = default);

    /// <summary>Featured published posts for a website (newest first).</summary>
    Task<List<PostSummaryDto>> GetFeaturedAsync(int websiteId, int take, string? languageCode = null, CancellationToken ct = default);
}
