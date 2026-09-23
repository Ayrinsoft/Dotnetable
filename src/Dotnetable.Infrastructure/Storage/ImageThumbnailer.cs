using SkiaSharp;

namespace Dotnetable.Infrastructure.Storage;

/// <summary>Generates downscaled WebP thumbnails for image uploads using SkiaSharp.</summary>
public static class ImageThumbnailer
{
    /// <summary>
    /// Longest edge of a thumbnail. Large enough to stand in for the original inside an article column,
    /// so the full-size file is only fetched when a visitor opens it.
    /// </summary>
    public const int MaxEdge = 640;

    /// <summary>Lossy WebP quality for thumbnails (balanced size vs clarity in the media grid).</summary>
    private const int WebpQuality = 72;

    /// <summary>Stored extensions that get a thumbnail. Video, audio, documents, SVG and GIF never do.</summary>
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp",
    };

    /// <summary>True when a file stored under <paramref name="storedName"/> should get a thumbnail.</summary>
    public static bool Supports(string? storedName) =>
        !string.IsNullOrWhiteSpace(storedName) && Extensions.Contains(Path.GetExtension(storedName));

    /// <summary>
    /// Deterministic thumbnail key for an original: <c>{name}_thumb.webp</c> next to it. Being derivable
    /// from the original's name, a site can build the thumbnail URL from any original URL it has.
    /// </summary>
    public static string NameFor(string storedName) =>
        Path.GetFileNameWithoutExtension(storedName) + "_thumb.webp";

    /// <summary>
    /// Returns a WebP thumbnail stream for <paramref name="source"/>, or null when the bytes are not a
    /// decodable image. EXIF orientation is applied to the pixels. The source stream position is reset on entry.
    /// </summary>
    public static Task<MemoryStream?> TryCreateAsync(Stream source, CancellationToken ct = default)
    {
        if (source.CanSeek) source.Position = 0;
        try
        {
            using var original = ImageDecoder.DecodeOriented(source);
            if (original is null) return Task.FromResult<MemoryStream?>(null);

            var scale = Math.Min(1f, Math.Min((float)MaxEdge / original.Width, (float)MaxEdge / original.Height));
            var width = Math.Max(1, (int)Math.Round(original.Width * scale));
            var height = Math.Max(1, (int)Math.Round(original.Height * scale));

            using var resized = original.Resize(new SKImageInfo(width, height), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
            if (resized is null) return Task.FromResult<MemoryStream?>(null);

            using var image = SKImage.FromBitmap(resized);

            SKData? encoded = null;
            using (var pixmap = image.PeekPixels())
            {
                if (pixmap is not null)
                    encoded = pixmap.Encode(new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossy, WebpQuality));
            }
            encoded ??= image.Encode(SKEncodedImageFormat.Webp, WebpQuality);
            if (encoded is null) return Task.FromResult<MemoryStream?>(null);

            var output = new MemoryStream();
            using (encoded)
                encoded.SaveTo(output);
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
