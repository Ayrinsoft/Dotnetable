using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class WebsiteSettingService : IWebsiteSettingService
{
    private readonly AppDbContext _context;

    public WebsiteSettingService(AppDbContext context) => _context = context;

    // ── IPs ──────────────────────────────────────────────────────────

    public async Task<PagedResult<WebsiteIP>> GetIPsPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.WebsiteIPs.AsNoTracking().Where(x => x.WebsiteID == websiteId);

        if (query.GetSearch(nameof(WebsiteIP.Label)) is string label)
            q = q.Where(x => x.Label.Contains(label));
        if (query.GetSearch(nameof(WebsiteIP.StartIP)) is string ip)
            q = q.Where(x => x.StartIP.Contains(ip));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(WebsiteIP.WebsiteIPID))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<WebsiteIP> { Items = items, TotalCount = total };
    }

    public async Task<int> GetIPCountAsync(int websiteId, CancellationToken ct = default) =>
        await _context.WebsiteIPs.CountAsync(x => x.WebsiteID == websiteId, ct);

    public async Task<WebsiteIP?> GetIPByIdAsync(int id, CancellationToken ct = default) =>
        await _context.WebsiteIPs.FindAsync([id], ct);

    public async Task<WebsiteIP> CreateIPAsync(WebsiteIP ip, CancellationToken ct = default)
    {
        _context.WebsiteIPs.Add(ip);
        await _context.SaveChangesAsync(ct);
        return ip;
    }

    public async Task UpdateIPAsync(WebsiteIP ip, CancellationToken ct = default)
    {
        _context.WebsiteIPs.Update(ip);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteIPAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.WebsiteIPs.FindAsync([id], ct);
        if (entity is null) return;
        _context.WebsiteIPs.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task SetIPActiveAsync(int id, bool active, CancellationToken ct = default) =>
        await _context.WebsiteIPs.Where(x => x.WebsiteIPID == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Active, active), ct);

    // ── Scripts ──────────────────────────────────────────────────────

    public async Task<PagedResult<WebsiteScript>> GetScriptsPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.WebsiteScripts.AsNoTracking().Where(x => x.WebsiteID == websiteId);

        if (query.GetSearch(nameof(WebsiteScript.Name)) is string name)
            q = q.Where(x => x.Name.Contains(name));
        if (query.GetSearch(nameof(WebsiteScript.Active)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(x => x.Active == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(WebsiteScript.Priority))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<WebsiteScript> { Items = items, TotalCount = total };
    }

    public async Task<WebsiteScript?> GetScriptByIdAsync(int id, CancellationToken ct = default) =>
        await _context.WebsiteScripts.FindAsync([id], ct);

    public async Task<WebsiteScript> CreateScriptAsync(WebsiteScript script, CancellationToken ct = default)
    {
        script.LogTime = DateTime.UtcNow;
        _context.WebsiteScripts.Add(script);
        await _context.SaveChangesAsync(ct);
        return script;
    }

    public async Task UpdateScriptAsync(WebsiteScript script, CancellationToken ct = default)
    {
        _context.WebsiteScripts.Update(script);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteScriptAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.WebsiteScripts.FindAsync([id], ct);
        if (entity is null) return;
        _context.WebsiteScripts.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task SetScriptActiveAsync(int id, bool active, CancellationToken ct = default) =>
        await _context.WebsiteScripts.Where(x => x.WebsiteScriptID == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Active, active), ct);

    // ── SEO ──────────────────────────────────────────────────────────

    public async Task<WebsiteSeoSetting?> GetSeoSettingAsync(int websiteId, CancellationToken ct = default) =>
        await _context.WebsiteSeoSettings.FirstOrDefaultAsync(x => x.WebsiteID == websiteId, ct);

    public async Task SaveSeoSettingAsync(WebsiteSeoSetting setting, CancellationToken ct = default)
    {
        var existing = await _context.WebsiteSeoSettings
            .FirstOrDefaultAsync(x => x.WebsiteID == setting.WebsiteID, ct);

        if (existing is null)
        {
            _context.WebsiteSeoSettings.Add(setting);
        }
        else
        {
            existing.DefaultMetaTitle = setting.DefaultMetaTitle;
            existing.TitleSeparator = setting.TitleSeparator;
            existing.DefaultMetaDescription = setting.DefaultMetaDescription;
            existing.SitemapEnabled = setting.SitemapEnabled;
            existing.RobotsEnabled = setting.RobotsEnabled;
            existing.CustomRobotsTxt = setting.CustomRobotsTxt;
        }

        await _context.SaveChangesAsync(ct);
    }

    // ── Social Links ─────────────────────────────────────────────────

    public async Task<PagedResult<WebsiteSocialLink>> GetSocialLinksPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.WebsiteSocialLinks.AsNoTracking().Where(x => x.WebsiteID == websiteId);

        if (query.GetSearch(nameof(WebsiteSocialLink.SocialName)) is string name)
            q = q.Where(x => x.SocialName != null && x.SocialName.Contains(name));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(WebsiteSocialLink.WebsiteSocialLinkID))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<WebsiteSocialLink> { Items = items, TotalCount = total };
    }

    public async Task<WebsiteSocialLink?> GetSocialLinkByIdAsync(int id, CancellationToken ct = default) =>
        await _context.WebsiteSocialLinks.FindAsync([id], ct);

    public async Task<WebsiteSocialLink> CreateSocialLinkAsync(WebsiteSocialLink link, CancellationToken ct = default)
    {
        _context.WebsiteSocialLinks.Add(link);
        await _context.SaveChangesAsync(ct);
        return link;
    }

    public async Task UpdateSocialLinkAsync(WebsiteSocialLink link, CancellationToken ct = default)
    {
        _context.WebsiteSocialLinks.Update(link);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteSocialLinkAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.WebsiteSocialLinks.FindAsync([id], ct);
        if (entity is null) return;
        _context.WebsiteSocialLinks.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
