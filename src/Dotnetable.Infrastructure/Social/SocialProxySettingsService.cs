using Dotnetable.Application.Instagram;
using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Social;

public sealed class SocialProxySettingsService : ISocialProxySettings
{
    private readonly IDbContextFactory<AppDbContext> _contexts;

    public SocialProxySettingsService(IDbContextFactory<AppDbContext> contexts) => _contexts = contexts;

    public async Task<SocialProxyStatus> GetAsync(int websiteId, CancellationToken ct = default)
    {
        await using var context = await _contexts.CreateDbContextAsync(ct);
        var row = await context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId)
            .Select(w => new { w.SocialProxyUrl, w.SocialProxyKey })
            .FirstOrDefaultAsync(ct);
        return new SocialProxyStatus
        {
            BaseUrl = row?.SocialProxyUrl,
            HasKey = !string.IsNullOrWhiteSpace(row?.SocialProxyKey),
        };
    }

    public async Task SaveAsync(int websiteId, string? baseUrl, string? key, bool clearKey, CancellationToken ct = default)
    {
        var normalized = SocialProxyAddress.NormalizeBase(baseUrl);
        await using var context = await _contexts.CreateDbContextAsync(ct);
        var row = await context.Websites.FirstOrDefaultAsync(w => w.WebsiteID == websiteId, ct)
            ?? throw new SocialPublishException("Website was not found.");
        row.SocialProxyUrl = normalized;
        if (normalized is null || clearKey)
            row.SocialProxyKey = null;
        else if (!string.IsNullOrWhiteSpace(key))
            row.SocialProxyKey = key.Trim();
        await context.SaveChangesAsync(ct);
    }
}
