using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Caching;

namespace Dotnetable.Infrastructure.Services.Caching;

/// <summary>
/// Caches the public listing reads of <see cref="PostService"/>; any write drops the whole
/// <c>post</c> tag (see <see cref="CachedMenuService"/> for the rationale). <see cref="GetBySlugAsync"/>
/// is intentionally left uncached — it increments the post's view count as a side effect, and
/// caching it would suppress that on every hit.
/// </summary>
public class CachedPostService : IPostService
{
    private const string Tag = "post";

    private readonly PostService _inner;
    private readonly ICacheService _cache;
    private readonly ICacheInvalidationNotifier _notifier;
    private readonly TimeSpan _ttl;

    public CachedPostService(PostService inner, ICacheService cache, ICacheInvalidationNotifier notifier, CacheOptions options)
    {
        _inner = inner;
        _cache = cache;
        _notifier = notifier;
        _ttl = options.DefaultTtl;
    }

    public Task<PagedResult<Post>> GetPagedAsync(int? websiteId, PostFilter filter, GridQuery query, CancellationToken ct = default) =>
        _inner.GetPagedAsync(websiteId, filter, query, ct);

    public Task<Post?> GetByIdAsync(int postId, CancellationToken ct = default) =>
        _inner.GetByIdAsync(postId, ct);

    public async Task<Post> CreateAsync(Post post, CancellationToken ct = default)
    {
        var result = await _inner.CreateAsync(post, ct);
        await InvalidateAsync(ct);
        return result;
    }

    public async Task UpdateAsync(Post post, CancellationToken ct = default)
    {
        await _inner.UpdateAsync(post, ct);
        await InvalidateAsync(ct);
    }

    public async Task DeleteAsync(int postId, CancellationToken ct = default)
    {
        await _inner.DeleteAsync(postId, ct);
        await InvalidateAsync(ct);
    }

    public async Task SetActiveAsync(int postId, bool active, CancellationToken ct = default)
    {
        await _inner.SetActiveAsync(postId, active, ct);
        await InvalidateAsync(ct);
    }

    public Task<List<PostTranslation>> GetTranslationsAsync(int postId, CancellationToken ct = default) =>
        _inner.GetTranslationsAsync(postId, ct);

    public async Task SetTranslationsAsync(int postId, IReadOnlyList<PostTranslation> translations, CancellationToken ct = default)
    {
        await _inner.SetTranslationsAsync(postId, translations, ct);
        await InvalidateAsync(ct);
    }

    public Task<List<int>> GetCategoryIdsAsync(int postId, CancellationToken ct = default) =>
        _inner.GetCategoryIdsAsync(postId, ct);

    public async Task SetCategoriesAsync(int postId, IReadOnlyList<int> categoryIds, int? primaryCategoryId, CancellationToken ct = default)
    {
        await _inner.SetCategoriesAsync(postId, categoryIds, primaryCategoryId, ct);
        await InvalidateAsync(ct);
    }

    public Task<List<int>> GetTagIdsAsync(int postId, CancellationToken ct = default) =>
        _inner.GetTagIdsAsync(postId, ct);

    public async Task SetTagsAsync(int postId, IReadOnlyList<int> tagIds, CancellationToken ct = default)
    {
        await _inner.SetTagsAsync(postId, tagIds, ct);
        await InvalidateAsync(ct);
    }

    public Task<PagedResult<PostSummaryDto>> GetPublishedAsync(
        int websiteId, string? postTypeSlug, string? categorySlug, string? tagSlug,
        int pageIndex, int pageSize, string? languageCode = null, CancellationToken ct = default) =>
        _cache.GetOrCreateAsync(
            $"post:published:{websiteId}:{postTypeSlug}:{categorySlug}:{tagSlug}:{pageIndex}:{pageSize}:{languageCode}",
            [Tag],
            _ttl,
            () => _inner.GetPublishedAsync(websiteId, postTypeSlug, categorySlug, tagSlug, pageIndex, pageSize, languageCode, ct));

    public Task<PostDetailDto?> GetBySlugAsync(int websiteId, string slug, string? languageCode = null, CancellationToken ct = default) =>
        _inner.GetBySlugAsync(websiteId, slug, languageCode, ct);

    public Task<List<PostSummaryDto>> GetFeaturedAsync(int websiteId, int take, string? languageCode = null, CancellationToken ct = default) =>
        _cache.GetOrCreateAsync(
            $"post:featured:{websiteId}:{take}:{languageCode}",
            [Tag],
            _ttl,
            () => _inner.GetFeaturedAsync(websiteId, take, languageCode, ct));

    private Task InvalidateAsync(CancellationToken ct)
    {
        _cache.RemoveByTag(Tag);
        return _notifier.NotifyAsync(Tag, ct);
    }
}
