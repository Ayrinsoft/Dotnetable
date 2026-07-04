using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Caching;

namespace Dotnetable.Infrastructure.Services.Caching;

/// <summary>
/// Caches the public read projections of <see cref="MenuService"/>. Any write (menu, item or
/// translation) drops the whole <c>menu</c> tag — menus change rarely and are cheap to rebuild, so a
/// single coarse tag keeps invalidation trivially correct instead of tracking per-website keys.
/// </summary>
public class CachedMenuService : IMenuService
{
    private const string Tag = "menu";

    private readonly MenuService _inner;
    private readonly ICacheService _cache;
    private readonly ICacheInvalidationNotifier _notifier;
    private readonly TimeSpan _ttl;

    public CachedMenuService(MenuService inner, ICacheService cache, ICacheInvalidationNotifier notifier, CacheOptions options)
    {
        _inner = inner;
        _cache = cache;
        _notifier = notifier;
        _ttl = options.DefaultTtl;
    }

    // ── Admin management (pass-through + invalidate) ────────────────

    public Task<List<Menu>> GetMenusAsync(int? websiteId, CancellationToken ct = default) =>
        _inner.GetMenusAsync(websiteId, ct);

    public Task<Menu?> GetMenuAsync(int menuId, CancellationToken ct = default) =>
        _inner.GetMenuAsync(menuId, ct);

    public async Task<Menu> CreateMenuAsync(Menu menu, CancellationToken ct = default)
    {
        var result = await _inner.CreateMenuAsync(menu, ct);
        await InvalidateAsync(ct);
        return result;
    }

    public async Task UpdateMenuAsync(Menu menu, CancellationToken ct = default)
    {
        await _inner.UpdateMenuAsync(menu, ct);
        await InvalidateAsync(ct);
    }

    public async Task DeleteMenuAsync(int menuId, CancellationToken ct = default)
    {
        await _inner.DeleteMenuAsync(menuId, ct);
        await InvalidateAsync(ct);
    }

    public Task<List<MenuItem>> GetItemsAsync(int menuId, CancellationToken ct = default) =>
        _inner.GetItemsAsync(menuId, ct);

    public Task<MenuItem?> GetItemAsync(int itemId, CancellationToken ct = default) =>
        _inner.GetItemAsync(itemId, ct);

    public async Task<MenuItem> CreateItemAsync(MenuItem item, CancellationToken ct = default)
    {
        var result = await _inner.CreateItemAsync(item, ct);
        await InvalidateAsync(ct);
        return result;
    }

    public async Task UpdateItemAsync(MenuItem item, CancellationToken ct = default)
    {
        await _inner.UpdateItemAsync(item, ct);
        await InvalidateAsync(ct);
    }

    public async Task DeleteItemAsync(int itemId, CancellationToken ct = default)
    {
        await _inner.DeleteItemAsync(itemId, ct);
        await InvalidateAsync(ct);
    }

    public Task<List<MenuItemTranslation>> GetItemTranslationsAsync(int itemId, CancellationToken ct = default) =>
        _inner.GetItemTranslationsAsync(itemId, ct);

    public async Task SetItemTranslationsAsync(int itemId, IReadOnlyDictionary<string, string> titlesByLanguage, CancellationToken ct = default)
    {
        await _inner.SetItemTranslationsAsync(itemId, titlesByLanguage, ct);
        await InvalidateAsync(ct);
    }

    // ── Public read (cached) ─────────────────────────────────────────

    public Task<MenuDto?> GetByLocationAsync(int websiteId, MenuLocation location, string? languageCode = null, CancellationToken ct = default) =>
        _cache.GetOrCreateAsync(
            $"menu:location:{websiteId}:{location}:{languageCode}",
            [Tag],
            _ttl,
            () => _inner.GetByLocationAsync(websiteId, location, languageCode, ct));

    public Task<List<MenuDto>> GetActiveMenusAsync(int websiteId, string? languageCode = null, CancellationToken ct = default) =>
        _cache.GetOrCreateAsync(
            $"menu:active:{websiteId}:{languageCode}",
            [Tag],
            _ttl,
            () => _inner.GetActiveMenusAsync(websiteId, languageCode, ct));

    private Task InvalidateAsync(CancellationToken ct)
    {
        _cache.RemoveByTag(Tag);
        return _notifier.NotifyAsync(Tag, ct);
    }
}
