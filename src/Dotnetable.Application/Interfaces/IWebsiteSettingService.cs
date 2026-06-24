using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface IWebsiteSettingService
{
    // IP Management (max 5 per website)
    Task<PagedResult<WebsiteIP>> GetIPsPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default);
    Task<int> GetIPCountAsync(int websiteId, CancellationToken ct = default);
    Task<WebsiteIP?> GetIPByIdAsync(int id, CancellationToken ct = default);
    Task<WebsiteIP> CreateIPAsync(WebsiteIP ip, CancellationToken ct = default);
    Task UpdateIPAsync(WebsiteIP ip, CancellationToken ct = default);
    Task DeleteIPAsync(int id, CancellationToken ct = default);
    Task SetIPActiveAsync(int id, bool active, CancellationToken ct = default);

    // Scripts
    Task<PagedResult<WebsiteScript>> GetScriptsPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default);
    Task<WebsiteScript?> GetScriptByIdAsync(int id, CancellationToken ct = default);
    Task<WebsiteScript> CreateScriptAsync(WebsiteScript script, CancellationToken ct = default);
    Task UpdateScriptAsync(WebsiteScript script, CancellationToken ct = default);
    Task DeleteScriptAsync(int id, CancellationToken ct = default);
    Task SetScriptActiveAsync(int id, bool active, CancellationToken ct = default);

    // SEO Settings (one row per website)
    Task<WebsiteSeoSetting?> GetSeoSettingAsync(int websiteId, CancellationToken ct = default);
    Task SaveSeoSettingAsync(WebsiteSeoSetting setting, CancellationToken ct = default);

    // Social Links
    Task<PagedResult<WebsiteSocialLink>> GetSocialLinksPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default);
    Task<WebsiteSocialLink?> GetSocialLinkByIdAsync(int id, CancellationToken ct = default);
    Task<WebsiteSocialLink> CreateSocialLinkAsync(WebsiteSocialLink link, CancellationToken ct = default);
    Task UpdateSocialLinkAsync(WebsiteSocialLink link, CancellationToken ct = default);
    Task DeleteSocialLinkAsync(int id, CancellationToken ct = default);
}
