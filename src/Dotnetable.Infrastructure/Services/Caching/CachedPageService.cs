using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Caching;

namespace Dotnetable.Infrastructure.Services.Caching;

/// <summary>Caches the public read projections of <see cref="PageService"/>; any write drops
/// the whole <c>page</c> tag (see <see cref="CachedMenuService"/> for the rationale).</summary>
public class CachedPageService : IPageService
{
    private const string Tag = "page";

    private readonly PageService _inner;
    private readonly ICacheService _cache;
    private readonly ICacheInvalidationNotifier _notifier;
    private readonly TimeSpan _ttl;

    public CachedPageService(PageService inner, ICacheService cache, ICacheInvalidationNotifier notifier, CacheOptions options)
    {
        _inner = inner;
        _cache = cache;
        _notifier = notifier;
        _ttl = options.DefaultTtl;
    }

    public Task<PagedResult<Page>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default) =>
        _inner.GetPagedAsync(websiteId, query, ct);

    public Task<List<Page>> GetAllAsync(int? websiteId, CancellationToken ct = default) =>
        _inner.GetAllAsync(websiteId, ct);

    public Task<Page?> GetByIdAsync(int pageId, CancellationToken ct = default) =>
        _inner.GetByIdAsync(pageId, ct);

    public async Task<Page> CreateAsync(Page page, CancellationToken ct = default)
    {
        var result = await _inner.CreateAsync(page, ct);
        await InvalidateAsync(ct);
        return result;
    }

    public async Task UpdateAsync(Page page, CancellationToken ct = default)
    {
        await _inner.UpdateAsync(page, ct);
        await InvalidateAsync(ct);
    }

    public async Task DeleteAsync(int pageId, CancellationToken ct = default)
    {
        await _inner.DeleteAsync(pageId, ct);
        await InvalidateAsync(ct);
    }

    public async Task SetActiveAsync(int pageId, bool active, CancellationToken ct = default)
    {
        await _inner.SetActiveAsync(pageId, active, ct);
        await InvalidateAsync(ct);
    }

    public Task<List<PageTranslation>> GetTranslationsAsync(int pageId, CancellationToken ct = default) =>
        _inner.GetTranslationsAsync(pageId, ct);

    public async Task SetTranslationsAsync(int pageId, IReadOnlyList<PageTranslation> translations, CancellationToken ct = default)
    {
        await _inner.SetTranslationsAsync(pageId, translations, ct);
        await InvalidateAsync(ct);
    }

    public Task<List<PageDto>> GetTreeAsync(int websiteId, string? languageCode = null, CancellationToken ct = default) =>
        _cache.GetOrCreateAsync(
            $"page:tree:{websiteId}:{languageCode}",
            [Tag],
            _ttl,
            () => _inner.GetTreeAsync(websiteId, languageCode, ct));

    public Task<PageDto?> GetBySlugAsync(int websiteId, string slug, string? languageCode = null, CancellationToken ct = default) =>
        _cache.GetOrCreateAsync(
            $"page:slug:{websiteId}:{slug}:{languageCode}",
            [Tag],
            _ttl,
            () => _inner.GetBySlugAsync(websiteId, slug, languageCode, ct));

    public Task<PageDto?> GetHomepageAsync(int websiteId, string? languageCode = null, CancellationToken ct = default) =>
        _cache.GetOrCreateAsync(
            $"page:home:{websiteId}:{languageCode}",
            [Tag],
            _ttl,
            () => _inner.GetHomepageAsync(websiteId, languageCode, ct));

    private Task InvalidateAsync(CancellationToken ct)
    {
        _cache.RemoveByTag(Tag);
        return _notifier.NotifyAsync(Tag, ct);
    }
}
