using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ThemeService : IThemeService
{
    private readonly AppDbContext _context;

    public ThemeService(AppDbContext context) => _context = context;

    public async Task<List<WebsiteTheme>> GetThemesAsync(int? websiteId, CancellationToken ct = default)
    {
        var q = _context.WebsiteThemes.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(t => t.WebsiteID == wid);
        return await q.OrderByDescending(t => t.IsActive).ThenBy(t => t.Name).ToListAsync(ct);
    }

    public async Task<PagedResult<WebsiteTheme>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.WebsiteThemes.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(t => t.WebsiteID == wid);

        if (query.GetSearch(nameof(WebsiteTheme.Name)) is string name)
            q = q.Where(t => t.Name.Contains(name));
        if (query.GetSearch(nameof(WebsiteTheme.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(t => t.IsActive == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(WebsiteTheme.Name))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<WebsiteTheme> { Items = items, TotalCount = total };
    }

    public async Task<WebsiteTheme?> GetThemeAsync(int themeId, CancellationToken ct = default) =>
        await _context.WebsiteThemes.FindAsync([themeId], ct);

    public async Task<WebsiteTheme> CreateThemeAsync(WebsiteTheme theme, CancellationToken ct = default)
    {
        _context.WebsiteThemes.Add(theme);
        await _context.SaveChangesAsync(ct);
        if (theme.IsActive)
            await ActivateThemeAsync(theme.WebsiteThemeID, ct);
        return theme;
    }

    public async Task UpdateThemeAsync(WebsiteTheme theme, CancellationToken ct = default)
    {
        _context.WebsiteThemes.Update(theme);
        await _context.SaveChangesAsync(ct);
        if (theme.IsActive)
            await ActivateThemeAsync(theme.WebsiteThemeID, ct);
    }

    public async Task DeleteThemeAsync(int themeId, CancellationToken ct = default)
    {
        var theme = await _context.WebsiteThemes.FindAsync([themeId], ct);
        if (theme is null) return;

        _context.WebsiteThemes.Remove(theme);
        await _context.SaveChangesAsync(ct);
    }

    public async Task ActivateThemeAsync(int themeId, CancellationToken ct = default)
    {
        var theme = await _context.WebsiteThemes.FirstOrDefaultAsync(t => t.WebsiteThemeID == themeId, ct);
        if (theme is null) return;

        var siblings = await _context.WebsiteThemes
            .Where(t => t.WebsiteID == theme.WebsiteID && t.WebsiteThemeID != themeId && t.IsActive)
            .ToListAsync(ct);
        foreach (var sibling in siblings)
            sibling.IsActive = false;

        theme.IsActive = true;
        await _context.SaveChangesAsync(ct);
    }

    public async Task<WebsiteTheme?> GetActiveThemeAsync(int websiteId, CancellationToken ct = default) =>
        await _context.WebsiteThemes.AsNoTracking()
            .FirstOrDefaultAsync(t => t.WebsiteID == websiteId && t.IsActive, ct);
}
