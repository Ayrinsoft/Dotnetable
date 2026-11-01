using Dotnetable.Application;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Catalog;

/// <summary>
/// How one storefront should see website-1 catalog images while the installation is a platform.
/// <see cref="Rewrite"/> is false for website 1 and for every site when platform mode is off.
/// </summary>
public readonly record struct PlatformImageAudience(bool Rewrite, string? WebsiteAddress, bool Watermark, string Version)
{
    public static PlatformImageAudience Direct { get; } = new(false, null, false, "0");
}

/// <summary>Picks the image URL a storefront is allowed to put in HTML or a product feed.</summary>
public static class PlatformProductImage
{
    public static async Task<PlatformImageAudience> LoadAsync(AppDbContext context, int viewerWebsiteId, CancellationToken ct = default)
    {
        if (viewerWebsiteId == AppConstants.MasterWebsiteId)
            return PlatformImageAudience.Direct;
        if (!await PlatformCatalog.IsPlatformAsync(context, ct))
            return PlatformImageAudience.Direct;

        var address = await context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == viewerWebsiteId)
            .Select(w => w.WebsiteAddress)
            .FirstOrDefaultAsync(ct);

        var mark = await context.WebsiteWatermarkSettings.AsNoTracking()
            .Where(s => s.WebsiteID == viewerWebsiteId)
            .Select(s => new { s.Enabled, s.WatermarkFileID, s.Position, s.SizePercent, s.Opacity })
            .FirstOrDefaultAsync(ct);

        var on = mark is { Enabled: true, WatermarkFileID: not null };
        var version = on
            ? $"{mark!.WatermarkFileID}.{mark.Position}.{mark.SizePercent}.{mark.Opacity}"
            : "0";
        return new PlatformImageAudience(true, address, on, version);
    }

    /// <summary>
    /// Website 1 keeps the burned-in CDN url. A child with no watermark gets the clean CDN url.
    /// A child with its own watermark gets <c>/media/i/{id}</c> and never the clean CDN address.
    /// A legacy file with no clean sibling keeps the burned url so it is not stamped a second time.
    /// </summary>
    public static string? ForViewer(FileRecord? file, PlatformImageAudience audience, bool thumbnail)
    {
        if (file is null || file.IsDeleted) return null;

        var burned = thumbnail ? First(file.ThumbnailCDN, file.CNDUrl) : First(file.CNDUrl, file.ThumbnailCDN);
        if (!audience.Rewrite || file.WebsiteID != AppConstants.MasterWebsiteId)
            return burned;

        var raw = thumbnail ? First(file.RawThumbnailCDN, file.RawCdnUrl) : First(file.RawCdnUrl, file.RawThumbnailCDN);
        if (!audience.Watermark || !HasClean(file))
            return raw ?? burned;

        var kind = thumbnail ? "t" : "f";
        var path = $"/media/i/{file.FileRecordID}?s={kind}&v={Uri.EscapeDataString(audience.Version)}";
        return Absolute(audience.WebsiteAddress, path);
    }

    public static bool HasClean(FileRecord file) =>
        !string.IsNullOrWhiteSpace(file.RawCdnUrl) || !string.IsNullOrWhiteSpace(file.RawThumbnailCDN);

    public static string Absolute(string? websiteAddress, string path)
    {
        var address = (websiteAddress ?? "").Trim().TrimEnd('/');
        if (address.Length == 0) return path;
        if (!address.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            && !address.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            address = "https://" + address;
        return address + path;
    }

    private static string? First(string? primary, string? fallback)
    {
        if (!string.IsNullOrWhiteSpace(primary)) return primary.Trim();
        if (!string.IsNullOrWhiteSpace(fallback)) return fallback.Trim();
        return null;
    }
}
