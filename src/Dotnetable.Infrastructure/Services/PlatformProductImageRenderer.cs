using Dotnetable.Application;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Catalog;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SkiaSharp;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Streams a website-1 catalog image for a subsidiary site. The clean object stays unwatermarked.
/// A destination watermark is applied in memory and the response is never a redirect to the CDN.
/// </summary>
public sealed class PlatformProductImageRenderer : IPlatformProductImageRenderer
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration? _configuration;

    public PlatformProductImageRenderer(
        IDbContextFactory<AppDbContext> contextFactory,
        IHttpClientFactory httpClientFactory,
        IConfiguration? configuration = null)
    {
        _contextFactory = contextFactory;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<PlatformProductImageBytes?> RenderAsync(int viewerWebsiteId, int fileId, bool thumbnail, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        if (!await PlatformCatalog.IsPlatformAsync(context, ct))
            return null;

        var file = await context.FileRecords.AsNoTracking()
            .FirstOrDefaultAsync(f => f.FileRecordID == fileId && !f.IsDeleted, ct);
        if (file is null || file.WebsiteID != AppConstants.MasterWebsiteId || file.FileCategory != (byte)FileCategory.Image)
            return null;

        var hasClean = PlatformProductImage.HasClean(file);
        var sourceUrl = hasClean
            ? (thumbnail ? First(file.RawThumbnailCDN, file.RawCdnUrl) : First(file.RawCdnUrl, file.RawThumbnailCDN))
            : (thumbnail ? First(file.ThumbnailCDN, file.CNDUrl) : First(file.CNDUrl, file.ThumbnailCDN));
        var bytes = await DownloadAsync(sourceUrl, ct);
        if (bytes is null || bytes.Length == 0)
            return null;

        // Legacy files have no clean sibling. Their public object may already carry website 1's mark.
        var stamp = hasClean && viewerWebsiteId != AppConstants.MasterWebsiteId
            ? await LoadWatermarkAsync(context, viewerWebsiteId, ct)
            : null;
        if (stamp is null)
            return new PlatformProductImageBytes(bytes, ContentTypeFor(sourceUrl, file.MimeType));

        await using var source = new MemoryStream(bytes);
        var processed = await ImageProcessor.TryProcessAsync(
            source,
            new ImageProcessingOptions { Watermark = stamp },
            SKEncodedImageFormat.Webp,
            maxOutputBytes: null,
            ct);
        if (processed is null)
            return new PlatformProductImageBytes(bytes, MimeOrDefault(file.MimeType));

        await using (processed)
            return new PlatformProductImageBytes(processed.ToArray(), "image/webp");
    }

    private async Task<WatermarkOptions?> LoadWatermarkAsync(AppDbContext context, int websiteId, CancellationToken ct)
    {
        var setting = await context.WebsiteWatermarkSettings.AsNoTracking()
            .Include(s => s.WatermarkFile)
            .FirstOrDefaultAsync(s => s.WebsiteID == websiteId, ct);
        if (setting is not { Enabled: true, WatermarkFile.CNDUrl: not null })
            return null;

        var mark = await DownloadAsync(setting.WatermarkFile!.CNDUrl, ct);
        if (mark is null || mark.Length == 0)
            return null;

        return new WatermarkOptions
        {
            ImageBytes = mark,
            Position = (WatermarkPosition)setting.Position,
            SizePercent = setting.SizePercent,
            Opacity = setting.Opacity,
        };
    }

    private async Task<byte[]?> DownloadAsync(string? url, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        if (url.StartsWith('/'))
        {
            var apiBase = (_configuration?["Api:BaseUrl"] ?? "").TrimEnd('/');
            if (string.IsNullOrEmpty(apiBase)) return null;
            url = apiBase + url;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return null;
        try
        {
            var client = _httpClientFactory.CreateClient();
            return await client.GetByteArrayAsync(uri, ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    private static string? First(string? primary, string? fallback)
    {
        if (!string.IsNullOrWhiteSpace(primary)) return primary.Trim();
        if (!string.IsNullOrWhiteSpace(fallback)) return fallback.Trim();
        return null;
    }

    private static string MimeOrDefault(string? mime) =>
        string.IsNullOrWhiteSpace(mime) ? "application/octet-stream" : mime;

    private static string ContentTypeFor(string? url, string? mime)
    {
        var ext = Path.GetExtension((url ?? "").Split('?', '#')[0]);
        if (ext.Equals(".webp", StringComparison.OrdinalIgnoreCase)) return "image/webp";
        if (ext.Equals(".png", StringComparison.OrdinalIgnoreCase)) return "image/png";
        if (ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
            return "image/jpeg";
        if (ext.Equals(".gif", StringComparison.OrdinalIgnoreCase)) return "image/gif";
        return MimeOrDefault(mime);
    }
}
