using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Navigation menus and their (optionally nested) items. The admin panel uses the management
/// methods directly; the public website consumes the read projections through the API.
/// All menus are scoped to a single <see cref="Website"/>.
/// </summary>
public interface IMenuService
{
    // ── Menus (admin management) ────────────────────────────────────

    /// <summary>All menus for a website (or every website when <paramref name="websiteId"/> is null — master only).</summary>
    Task<List<Menu>> GetMenusAsync(int? websiteId, CancellationToken ct = default);

    /// <summary>Paged/filtered/sorted menus for the grid (or every website when <paramref name="websiteId"/> is null — master only).</summary>
    Task<PagedResult<Menu>> GetMenusPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);

    Task<Menu?> GetMenuAsync(int menuId, CancellationToken ct = default);
    Task<Menu> CreateMenuAsync(Menu menu, CancellationToken ct = default);
    Task UpdateMenuAsync(Menu menu, CancellationToken ct = default);
    Task DeleteMenuAsync(int menuId, CancellationToken ct = default);

    // ── Items (admin management) ────────────────────────────────────

    /// <summary>Every item of a menu, ordered by parent then sort order (flat list, includes inactive).</summary>
    Task<List<MenuItem>> GetItemsAsync(int menuId, CancellationToken ct = default);

    Task<MenuItem?> GetItemAsync(int itemId, CancellationToken ct = default);
    Task<MenuItem> CreateItemAsync(MenuItem item, CancellationToken ct = default);
    Task UpdateItemAsync(MenuItem item, CancellationToken ct = default);
    Task DeleteItemAsync(int itemId, CancellationToken ct = default);

    // ── Per-item translations (admin management) ────────────────────

    Task<List<MenuItemTranslation>> GetItemTranslationsAsync(int itemId, CancellationToken ct = default);

    /// <summary>Replaces the item's translations with the supplied language→title map
    /// (blank titles remove that language's translation).</summary>
    Task SetItemTranslationsAsync(int itemId, IReadOnlyDictionary<string, string> titlesByLanguage, CancellationToken ct = default);

    // ── Public read (website rendering) ─────────────────────────────

    /// <summary>Active menu for a location, projected to a render-ready tree and localized to
    /// <paramref name="languageCode"/> when translations exist. Null when no active menu is set.</summary>
    Task<MenuDto?> GetByLocationAsync(int websiteId, MenuLocation location, string? languageCode = null, CancellationToken ct = default);

    /// <summary>Every active menu for a website, projected and localized — one per location.</summary>
    Task<List<MenuDto>> GetActiveMenusAsync(int websiteId, string? languageCode = null, CancellationToken ct = default);
}
