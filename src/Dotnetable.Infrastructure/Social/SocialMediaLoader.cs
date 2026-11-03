using Dotnetable.Application.DTOs;
using Dotnetable.Application.Instagram;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Instagram;
using Dotnetable.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace Dotnetable.Infrastructure.Social;

internal static class SocialMediaLoader
{
    public const int MaxImageBytes = 8 * 1024 * 1024;
    public const int MaxVideoBytes = 50 * 1024 * 1024;
    public const int XImageBytes = 5 * 1024 * 1024;

    public static async Task<TelegramOutbound> LoadAsync(
        AppDbContext context, IInstagramMediaHost host, int websiteId, string? siteAddress, InstagramMediaItem item, CancellationToken ct)
    {
        var video = item.Kind == InstagramMediaKind.Video;
        var source = await SourceAsync(context, host, websiteId, siteAddress, item, video ? MaxVideoBytes : MaxImageBytes, ct);
        if (video)
            return new TelegramOutbound(true, source, string.IsNullOrWhiteSpace(item.FileName) ? "clip.mp4" : item.FileName, item.MimeType);

        await using var input = new MemoryStream(source, writable: false);
        var jpeg = await ImageProcessor.TryProcessAsync(input, new ImageProcessingOptions { Crop = item.Crop }, SKEncodedImageFormat.Jpeg);
        if (jpeg is null)
            throw new SocialPublishException("That file is not a photo Telegram or X can use.");
        using (jpeg)
            return new TelegramOutbound(false, jpeg.ToArray(), "photo.jpg");
    }

    private static async Task<byte[]> SourceAsync(
        AppDbContext context, IInstagramMediaHost host, int websiteId, string? siteAddress, InstagramMediaItem item, int maxBytes, CancellationToken ct)
    {
        if (item.Content is { Length: > 0 })
        {
            if (item.Content.Length > maxBytes)
                throw new SocialPublishException(maxBytes > MaxImageBytes ? "Each video must be under 50 MB." : "Each photo must be under 8 MB.");
            return item.Content;
        }

        string? url;
        if (item.FileRecordId is int fileId)
        {
            var file = await context.FileRecords.AsNoTracking()
                .FirstOrDefaultAsync(f => f.FileRecordID == fileId && f.WebsiteID == websiteId && !f.IsDeleted, ct)
                ?? throw new SocialPublishException("One of the selected files is no longer in the library.");
            url = InstagramUrls.Absolute(file.CNDUrl, siteAddress);
        }
        else
        {
            url = InstagramUrls.Absolute(item.Url, siteAddress);
        }

        if (!InstagramUrls.IsPublicHttps(url))
            throw new SocialPublishException("Telegram and X need a public https address for a library file or a pasted link, unless you upload the file in the window.");
        return await host.DownloadAsync(url!, maxBytes, ct);
    }
}
