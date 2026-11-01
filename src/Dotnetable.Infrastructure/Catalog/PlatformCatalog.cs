using Dotnetable.Application;
using Dotnetable.Application.Catalog;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Catalog;

/// <summary>Resolves the website that owns the product catalog.</summary>
public static class PlatformCatalog
{
    public static async Task<bool> IsPlatformAsync(AppDbContext context, CancellationToken ct = default)
    {
        return await context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == AppConstants.MasterWebsiteId)
            .Select(w => (bool?)w.IsPlatform)
            .FirstOrDefaultAsync(ct) == true;
    }

    public static async Task<int> CatalogWebsiteIdAsync(AppDbContext context, int websiteId, CancellationToken ct = default)
    {
        if (websiteId == AppConstants.MasterWebsiteId) return websiteId;
        return await IsPlatformAsync(context, ct) ? AppConstants.MasterWebsiteId : websiteId;
    }

    public static async Task EnsureOwnerAsync(AppDbContext context, int websiteId, CancellationToken ct = default)
    {
        if (websiteId == AppConstants.MasterWebsiteId) return;
        if (await IsPlatformAsync(context, ct))
            throw new CatalogLockedException();
    }
}
