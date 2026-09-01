using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class MenuService : IMenuService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public MenuService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    // ── Menus ───────────────────────────────────────────────────────

    public async Task<List<Menu>> GetMenusAsync(int? websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.Menus.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(m => m.WebsiteID == wid);
        return await q.OrderBy(m => m.Location).ThenBy(m => m.Name).ToListAsync(ct);
    }

    public async Task<PagedResult<Menu>> GetMenusPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.Menus.AsNoTracking().Include(m => m.MenuItems).AsQueryable();
        if (websiteId is int wid)
            q = q.Where(m => m.WebsiteID == wid);

        if (query.GetSearch(nameof(Menu.Name)) is string name)
            q = q.Where(m => m.Name.Contains(name));
        if (query.GetSearch(nameof(Menu.Location)) is string loc && byte.TryParse(loc, out var location))
            q = q.Where(m => m.Location == location);
        if (query.GetSearch(nameof(Menu.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(m => m.IsActive == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Menu.MenuID))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Menu> { Items = items, TotalCount = total };
    }

    public async Task<Menu?> GetMenuAsync(int menuId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.Menus.FindAsync([menuId], ct);
    }

    public async Task<Menu> CreateMenuAsync(Menu menu, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        _context.Menus.Add(menu);
        await _context.SaveChangesAsync(ct);
        return menu;
    }

    public async Task UpdateMenuAsync(Menu menu, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        _context.Menus.Update(menu);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteMenuAsync(int menuId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var menu = await _context.Menus
            .Include(m => m.MenuItems).ThenInclude(i => i.MenuItemTranslations)
            .FirstOrDefaultAsync(m => m.MenuID == menuId, ct);
        if (menu is null) return;

        // Children reference their parent item (self FK), so clear translations, then items, then the menu.
        foreach (var item in menu.MenuItems)
            _context.MenuItemTranslations.RemoveRange(item.MenuItemTranslations);
        _context.MenuItems.RemoveRange(menu.MenuItems);
        _context.Menus.Remove(menu);
        await _context.SaveChangesAsync(ct);
    }

    // ── Items ───────────────────────────────────────────────────────

    public async Task<List<MenuItem>> GetItemsAsync(int menuId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.MenuItems.AsNoTracking()
            .Where(i => i.MenuID == menuId)
            .OrderBy(i => i.SortOrder).ThenBy(i => i.MenuItemID)
            .ToListAsync(ct);
    }

    public async Task<MenuItem?> GetItemAsync(int itemId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.MenuItems.FindAsync([itemId], ct);
    }

    public async Task<MenuItem> CreateItemAsync(MenuItem item, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync(ct);
        return item;
    }

    public async Task UpdateItemAsync(MenuItem item, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        _context.MenuItems.Update(item);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteItemAsync(int itemId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var item = await _context.MenuItems
            .Include(i => i.MenuItemTranslations)
            .Include(i => i.InverseParentItem)
            .FirstOrDefaultAsync(i => i.MenuItemID == itemId, ct);
        if (item is null) return;

        // Promote any children to this item's parent so the FK never dangles.
        foreach (var child in item.InverseParentItem)
            child.ParentItemID = item.ParentItemID;

        _context.MenuItemTranslations.RemoveRange(item.MenuItemTranslations);
        _context.MenuItems.Remove(item);
        await _context.SaveChangesAsync(ct);
    }

    // ── Translations ────────────────────────────────────────────────

    public async Task<List<MenuItemTranslation>> GetItemTranslationsAsync(int itemId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.MenuItemTranslations.AsNoTracking()
            .Where(t => t.MenuItemID == itemId)
            .ToListAsync(ct);
    }

    public async Task SetItemTranslationsAsync(int itemId, IReadOnlyDictionary<string, string> titlesByLanguage, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.MenuItemTranslations
            .Where(t => t.MenuItemID == itemId)
            .ToListAsync(ct);

        foreach (var (languageCode, title) in titlesByLanguage)
        {
            var current = existing.FirstOrDefault(t =>
                string.Equals(t.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(title))
            {
                if (current is not null) _context.MenuItemTranslations.Remove(current);
                continue;
            }

            if (current is null)
                _context.MenuItemTranslations.Add(new MenuItemTranslation
                {
                    MenuItemID = itemId,
                    LanguageCode = languageCode,
                    Title = title.Trim(),
                });
            else
                current.Title = title.Trim();
        }

        await _context.SaveChangesAsync(ct);
    }

    // ── Public read ─────────────────────────────────────────────────

    public async Task<MenuDto?> GetByLocationAsync(int websiteId, MenuLocation location, string? languageCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var menu = await LoadActiveMenusQuery(_context, websiteId)
            .FirstOrDefaultAsync(m => m.Location == (byte)location, ct);
        return menu is null ? null : Project(menu, languageCode);
    }

    public async Task<List<MenuDto>> GetActiveMenusAsync(int websiteId, string? languageCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var menus = await LoadActiveMenusQuery(_context, websiteId).ToListAsync(ct);
        return menus.Select(m => Project(m, languageCode)).ToList();
    }

    // Takes the caller's context: an IQueryable is only valid while the context that built it is
    // alive, so it must not open one of its own.
    private static IQueryable<Menu> LoadActiveMenusQuery(AppDbContext _context, int websiteId) =>
        _context.Menus.AsNoTracking()
            .Where(m => m.WebsiteID == websiteId && m.IsActive)
            .Include(m => m.MenuItems.Where(i => i.IsActive))
                .ThenInclude(i => i.MenuItemTranslations)
            .OrderBy(m => m.Location);

    // ── Projection helpers ──────────────────────────────────────────

    private static MenuDto Project(Menu menu, string? languageCode)
    {
        var items = menu.MenuItems.Where(i => i.IsActive).ToList();
        var byParent = items.ToLookup(i => i.ParentItemID);

        return new MenuDto
        {
            MenuID = menu.MenuID,
            Name = menu.Name,
            Location = (MenuLocation)menu.Location,
            Items = BuildChildren(null, byParent, languageCode),
        };
    }

    private static IReadOnlyList<MenuItemDto> BuildChildren(
        int? parentId,
        ILookup<int?, MenuItem> byParent,
        string? languageCode)
    {
        var children = byParent[parentId].OrderBy(i => i.SortOrder).ThenBy(i => i.MenuItemID);

        return children.Select(item => new MenuItemDto
        {
            MenuItemID = item.MenuItemID,
            Title = LocalizedTitle(item, languageCode),
            Url = ResolveUrl(item),
            Icon = item.Icon,
            CssClass = item.CssClass,
            OpenInNewTab = item.OpenInNewTab,
            Children = BuildChildren(item.MenuItemID, byParent, languageCode),
        }).ToList();
    }

    private static string LocalizedTitle(MenuItem item, string? languageCode)
    {
        if (!string.IsNullOrWhiteSpace(languageCode))
        {
            var t = item.MenuItemTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));
            if (t is not null && !string.IsNullOrWhiteSpace(t.Title)) return t.Title;
        }
        return item.Title;
    }

    /// <summary>
    /// Final href for an item. A custom Url always wins (it's also the admin override for typed items);
    /// otherwise a conventional path is built from the type's target id.
    /// </summary>
    private static string ResolveUrl(MenuItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.Url)) return item.Url!;

        return (MenuItemType)item.ItemType switch
        {
            MenuItemType.Page when item.PageID is int p => $"/page/{p}",
            MenuItemType.Post when item.PostID is int p => $"/blog/post/{p}",
            MenuItemType.Category when item.CategoryID is int c => $"/blog/category/{c}",
            MenuItemType.Product when item.ProductID is int p => $"/product/{p}",
            MenuItemType.ProductCategory when item.ProductCategoryID is int c => $"/shop/category/{c}",
            MenuItemType.Brand when item.BrandID is int b => $"/brand/{b}",
            MenuItemType.Vendor when item.VendorID is int v => $"/vendor/{v}",
            _ => "#",
        };
    }

}
