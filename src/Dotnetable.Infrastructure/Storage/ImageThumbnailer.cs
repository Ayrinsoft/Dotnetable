using SkiaSharp;

namespace Dotnetable.Infrastructure.Storage;

/// <summary>Generates downscaled JPEG thumbnails for image uploads using SkiaSharp.</summary>
public static class ImageThumbnailer
{
    public const int MaxEdge = 320;

    /// <summary>
    /// Returns a thumbnail stream for <paramref name="source"/>, or null when the bytes are not a
    /// decodable image. The source stream position is reset on entry.
    /// </summary>
    public static Task<MemoryStream?> TryCreateAsync(Stream source, CancellationToken ct = default)
    {
        if (source.CanSeek) source.Position = 0;
        try
        {
            using var original = SKBitmap.Decode(source);
            if (original is null) return Task.FromResult<MemoryStream?>(null);

            var scale = Math.Min(1f, Math.Min((float)MaxEdge / original.Width, (float)MaxEdge / original.Height));
            var width = Math.Max(1, (int)Math.Round(original.Width * scale));
            var height = Math.Max(1, (int)Math.Round(original.Height * scale));

            using var resized = original.Resize(new SKImageInfo(width, height), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
            if (resized is null) return Task.FromResult<MemoryStream?>(null);

            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);

            var output = new MemoryStream();
            data.SaveTo(output);
            output.Position = 0;
            return Task.FromResult<MemoryStream?>(output);
        }
        catch
        {
            return Task.FromResult<MemoryStream?>(null);
        }
        finally
        {
            if (source.CanSeek) source.Position = 0;
        }
    }
}
