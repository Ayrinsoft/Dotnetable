using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Caching;

namespace Dotnetable.Infrastructure.Services.Caching;

/// <summary>
/// Caches the public read projections of <see cref="AdvertisementService"/>. Any write drops the
/// whole <c>advertisement</c> tag — ads change rarely and are cheap to rebuild.
/// </summary>
public class CachedAdvertisementService : IAdvertisementService
{
    private const string Tag = "advertisement";

    private readonly AdvertisementService _inner;
    private readonly ICacheService _cache;
    private readonly ICacheInvalidationNotifier _notifier;
    private readonly TimeSpan _ttl;

    public CachedAdvertisementService(AdvertisementService inner, ICacheService cache, ICacheInvalidationNotifier notifier, CacheOptions options)
    {
        _inner = inner;
        _cache = cache;
        _notifier = notifier;
        _ttl = options.DefaultTtl;
    }

    public Task<List<Advertisement>> GetAllAsync(int? websiteId, CancellationToken ct = default) =>
        _inner.GetAllAsync(websiteId, ct);

    public Task<PagedResult<Advertisement>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default) =>
        _inner.GetPagedAsync(websiteId, query, ct);

    public Task<Advertisement?> GetByIdAsync(int advertisementId, CancellationToken ct = default) =>
        _inner.GetByIdAsync(advertisementId, ct);

    public async Task<Advertisement> CreateAsync(Advertisement advertisement, CancellationToken ct = default)
    {
        var result = await _inner.CreateAsync(advertisement, ct);
        await InvalidateAsync(ct);
        return result;
    }

    public async Task UpdateAsync(Advertisement advertisement, CancellationToken ct = default)
    {
        await _inner.UpdateAsync(advertisement, ct);
        await InvalidateAsync(ct);
    }

    public async Task DeleteAsync(int advertisementId, CancellationToken ct = default)
    {
        await _inner.DeleteAsync(advertisementId, ct);
        await InvalidateAsync(ct);
    }

    public Task<List<AdvertisementTranslation>> GetTranslationsAsync(int advertisementId, CancellationToken ct = default) =>
        _inner.GetTranslationsAsync(advertisementId, ct);

    public async Task SetTranslationsAsync(int advertisementId, IReadOnlyDictionary<string, (string Keyword, string? Url)> byLanguage, CancellationToken ct = default)
    {
        await _inner.SetTranslationsAsync(advertisementId, byLanguage, ct);
        await InvalidateAsync(ct);
    }

    public Task<List<AdvertisementDto>> GetByLocationAsync(int websiteId, AdvertisementLocation location, string? languageCode = null, CancellationToken ct = default) =>
        _cache.GetOrCreateAsync(
            $"advertisement:location:{websiteId}:{(byte)location}:{languageCode ?? ""}",
            [Tag],
            _ttl,
            () => _inner.GetByLocationAsync(websiteId, location, languageCode, ct));

    private Task InvalidateAsync(CancellationToken ct)
    {
        _cache.RemoveByTag(Tag);
        return _notifier.NotifyAsync(Tag, ct);
    }
}
