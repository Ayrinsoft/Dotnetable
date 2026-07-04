using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Caching;

namespace Dotnetable.Infrastructure.Services.Caching;

/// <summary>Caches the public read projections of <see cref="CategoryService"/>; any write drops
/// the whole <c>category</c> tag (see <see cref="CachedMenuService"/> for the rationale).</summary>
public class CachedCategoryService : ICategoryService
{
    private const string Tag = "category";

    private readonly CategoryService _inner;
    private readonly ICacheService _cache;
    private readonly ICacheInvalidationNotifier _notifier;
    private readonly TimeSpan _ttl;

    public CachedCategoryService(CategoryService inner, ICacheService cache, ICacheInvalidationNotifier notifier, CacheOptions options)
    {
        _inner = inner;
        _cache = cache;
        _notifier = notifier;
        _ttl = options.DefaultTtl;
    }

    public Task<List<Category>> GetAllAsync(int? websiteId, CancellationToken ct = default) =>
        _inner.GetAllAsync(websiteId, ct);

    public Task<Category?> GetByIdAsync(int categoryId, CancellationToken ct = default) =>
        _inner.GetByIdAsync(categoryId, ct);

    public async Task<Category> CreateAsync(Category category, CancellationToken ct = default)
    {
        var result = await _inner.CreateAsync(category, ct);
        await InvalidateAsync(ct);
        return result;
    }

    public async Task UpdateAsync(Category category, CancellationToken ct = default)
    {
        await _inner.UpdateAsync(category, ct);
        await InvalidateAsync(ct);
    }

    public async Task DeleteAsync(int categoryId, CancellationToken ct = default)
    {
        await _inner.DeleteAsync(categoryId, ct);
        await InvalidateAsync(ct);
    }

    public Task<List<CategoryTranslation>> GetTranslationsAsync(int categoryId, CancellationToken ct = default) =>
        _inner.GetTranslationsAsync(categoryId, ct);

    public async Task SetTranslationsAsync(int categoryId, IReadOnlyDictionary<string, (string Name, string Slug)> byLanguage, CancellationToken ct = default)
    {
        await _inner.SetTranslationsAsync(categoryId, byLanguage, ct);
        await InvalidateAsync(ct);
    }

    public Task<List<CategoryDto>> GetTreeAsync(int websiteId, int? postTypeId = null, string? languageCode = null, CancellationToken ct = default) =>
        _cache.GetOrCreateAsync(
            $"category:tree:{websiteId}:{postTypeId}:{languageCode}",
            [Tag],
            _ttl,
            () => _inner.GetTreeAsync(websiteId, postTypeId, languageCode, ct));

    public Task<CategoryDto?> GetBySlugAsync(int websiteId, string slug, string? languageCode = null, CancellationToken ct = default) =>
        _cache.GetOrCreateAsync(
            $"category:slug:{websiteId}:{slug}:{languageCode}",
            [Tag],
            _ttl,
            () => _inner.GetBySlugAsync(websiteId, slug, languageCode, ct));

    private Task InvalidateAsync(CancellationToken ct)
    {
        _cache.RemoveByTag(Tag);
        return _notifier.NotifyAsync(Tag, ct);
    }
}
