using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Caching;

namespace Dotnetable.Infrastructure.Services.Caching;

/// <summary>
/// Caches the public read projections of <see cref="SlideshowService"/>. Any write (slideshow or
/// slide) drops the whole <c>slideshow</c> tag — slideshows change rarely and are cheap to rebuild,
/// so a single coarse tag keeps invalidation trivially correct instead of tracking per-website keys.
/// </summary>
public class CachedSlideshowService : ISlideshowService
{
    private const string Tag = "slideshow";

    private readonly SlideshowService _inner;
    private readonly ICacheService _cache;
    private readonly ICacheInvalidationNotifier _notifier;
    private readonly TimeSpan _ttl;

    public CachedSlideshowService(SlideshowService inner, ICacheService cache, ICacheInvalidationNotifier notifier, CacheOptions options)
    {
        _inner = inner;
        _cache = cache;
        _notifier = notifier;
        _ttl = options.DefaultTtl;
    }

    // ── Admin management (pass-through + invalidate) ────────────────

    public Task<List<Slideshow>> GetSlideshowsAsync(int? websiteId, CancellationToken ct = default) =>
        _inner.GetSlideshowsAsync(websiteId, ct);

    public Task<PagedResult<Slideshow>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default) =>
        _inner.GetPagedAsync(websiteId, query, ct);

    public Task<Slideshow?> GetSlideshowAsync(int slideshowId, CancellationToken ct = default) =>
        _inner.GetSlideshowAsync(slideshowId, ct);

    public async Task<Slideshow> CreateSlideshowAsync(Slideshow slideshow, CancellationToken ct = default)
    {
        var result = await _inner.CreateSlideshowAsync(slideshow, ct);
        await InvalidateAsync(ct);
        return result;
    }

    public async Task UpdateSlideshowAsync(Slideshow slideshow, CancellationToken ct = default)
    {
        await _inner.UpdateSlideshowAsync(slideshow, ct);
        await InvalidateAsync(ct);
    }

    public async Task DeleteSlideshowAsync(int slideshowId, CancellationToken ct = default)
    {
        await _inner.DeleteSlideshowAsync(slideshowId, ct);
        await InvalidateAsync(ct);
    }

    public Task<List<SlideshowSlide>> GetSlidesAsync(int slideshowId, CancellationToken ct = default) =>
        _inner.GetSlidesAsync(slideshowId, ct);

    public Task<SlideshowSlide?> GetSlideAsync(int slideId, CancellationToken ct = default) =>
        _inner.GetSlideAsync(slideId, ct);

    public async Task<SlideshowSlide> CreateSlideAsync(SlideshowSlide slide, CancellationToken ct = default)
    {
        var result = await _inner.CreateSlideAsync(slide, ct);
        await InvalidateAsync(ct);
        return result;
    }

    public async Task UpdateSlideAsync(SlideshowSlide slide, CancellationToken ct = default)
    {
        await _inner.UpdateSlideAsync(slide, ct);
        await InvalidateAsync(ct);
    }

    public async Task DeleteSlideAsync(int slideId, CancellationToken ct = default)
    {
        await _inner.DeleteSlideAsync(slideId, ct);
        await InvalidateAsync(ct);
    }

    // ── Public read (cached) ─────────────────────────────────────────

    public Task<SlideshowDto?> GetByPlacementAsync(int websiteId, string placementKey, CancellationToken ct = default) =>
        _cache.GetOrCreateAsync(
            $"slideshow:placement:{websiteId}:{placementKey}",
            [Tag],
            _ttl,
            () => _inner.GetByPlacementAsync(websiteId, placementKey, ct));

    public Task<SlideshowDto?> GetByIdAsync(int websiteId, int slideshowId, CancellationToken ct = default) =>
        _cache.GetOrCreateAsync(
            $"slideshow:id:{websiteId}:{slideshowId}",
            [Tag],
            _ttl,
            () => _inner.GetByIdAsync(websiteId, slideshowId, ct));

    private Task InvalidateAsync(CancellationToken ct)
    {
        _cache.RemoveByTag(Tag);
        return _notifier.NotifyAsync(Tag, ct);
    }
}
