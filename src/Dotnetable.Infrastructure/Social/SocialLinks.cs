using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Social;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Social;

internal static class SocialLinks
{
    public static async Task<string?> MaybeShortAsync(
        IShortLinkService? shortLinks, int websiteId, string? link, bool shorten, CancellationToken ct)
    {
        if (!shorten || shortLinks is null || string.IsNullOrWhiteSpace(link)) return link;
        return await shortLinks.ToPublicAsync(websiteId, link, ct);
    }

    public static async Task<string?> ForContentAsync(
        AppDbContext db, int websiteId, bool product, string? slug, CancellationToken ct)
    {
        var row = await db.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId)
            .Select(w => new { w.WebsiteAddress, w.SocialLinkBaseUrl, w.SocialPostPath, w.SocialProductPath })
            .FirstOrDefaultAsync(ct);
        if (row is null) return null;

        return SocialLinkPaths.Build(
            row.WebsiteAddress,
            row.SocialLinkBaseUrl,
            product ? row.SocialProductPath : row.SocialPostPath,
            product ? SocialLinkPaths.DefaultProduct : SocialLinkPaths.DefaultPost,
            slug);
    }
}
