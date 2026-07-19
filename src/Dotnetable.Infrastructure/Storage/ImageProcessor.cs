using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Enums;
using SkiaSharp;

namespace Dotnetable.Infrastructure.Storage;

/// <summary>Options describing how <see cref="ImageProcessor"/> should transform an uploaded image.</summary>
public sealed class ImageProcessingOptions
{
    public ImageCropRect? Crop { get; init; }
    public int? ResizeWidth { get; init; }
    public int? ResizeHeight { get; init; }
    public bool Grayscale { get; init; }
    public WatermarkOptions? Watermark { get; init; }

    public bool HasWork => Crop is not null || ResizeWidth is not null || ResizeHeight is not null || Grayscale || Watermark is not null;
}

/// <summary>The watermark image bytes and how to place them, resolved from <c>WebsiteWatermarkSetting</c>.</summary>
public sealed class WatermarkOptions
{
    public required byte[] ImageBytes { get; init; }
    public WatermarkPosition Position { get; init; } = WatermarkPosition.BottomRight;
    public int SizePercent { get; init; } = 20;
    public int Opacity { get; init; } = 80;
}

/// <summary>Applies crop/resize/grayscale/watermark transforms to an uploaded image using SkiaSharp.</summary>
public static class ImageProcessor
{
    private const int MarginPx = 12;

    /// <summary>
    /// Starting WebP lossy quality. High enough for photos, low enough that well-compressed
    /// JPEGs do not balloon after re-encode.
    /// </summary>
    private const int WebpStartQuality = 80;

    /// <summary>Floor quality when stepping down to meet a size budget.</summary>
    private const int WebpMinQuality = 40;

    /// <summary>
    /// Returns a transformed image stream, or null when the source is not a decodable image.
    /// The source stream position is reset on entry.
    /// When <paramref name="maxOutputBytes"/> is set (typically the original file length), WebP
    /// encoding steps quality down so the result stays at or under that budget when possible.
    /// </summary>
    public static Task<MemoryStream?> TryProcessAsync(
        Stream source,
        ImageProcessingOptions options,
        SKEncodedImageFormat format,
        long? maxOutputBytes = null,
        CancellationToken ct = default)
    {
        if (source.CanSeek) source.Position = 0;
        try
        {
            using var decoded = SKBitmap.Decode(source);
            if (decoded is null) return Task.FromResult<MemoryStream?>(null);

            SKBitmap current = decoded;
            var owned = new List<SKBitmap>();

            try
            {
                if (options.Crop is { } crop)
                {
                    var cropped = ApplyCrop(current, crop);
                    if (cropped is not null) { owned.Add(cropped); current = cropped; }
                }

                if (options.ResizeWidth is not null || options.ResizeHeight is not null)
                {
                    var resized = ApplyResize(current, options.ResizeWidth, options.ResizeHeight);
                    if (resized is not null) { owned.Add(resized); current = resized; }
                }

                if (options.Grayscale)
                {
                    var gray = ApplyGrayscale(current);
                    owned.Add(gray);
                    current = gray;
                }

                if (options.Watermark is { } wm)
                {
                    var stamped = ApplyWatermark(current, wm);
                    if (stamped is not null) { owned.Add(stamped); current = stamped; }
                }

                using var image = SKImage.FromBitmap(current);
                using var data = Encode(image, format, maxOutputBytes);

                var output = new MemoryStream();
                data.SaveTo(output);
                output.Position = 0;
                return Task.FromResult<MemoryStream?>(output);
            }
            finally
            {
                foreach (var bmp in owned) bmp.Dispose();
            }
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

    /// <summary>
    /// Encodes with max PNG compression (zlib level 9, all filters) so output size stays close to source.
    /// WebP always uses lossy compression (never lossless — that path often inflates photos from
    /// hundreds of KB into multi‑MB files). Quality starts at 80 and steps down to meet
    /// <paramref name="maxOutputBytes"/> when provided. Other formats keep quality 90.
    /// </summary>
    private static SKData Encode(SKImage image, SKEncodedImageFormat format, long? maxOutputBytes)
    {
        if (format == SKEncodedImageFormat.Png)
        {
            using var pixmap = image.PeekPixels();
            if (pixmap is not null)
            {
                var options = new SKPngEncoderOptions(SKPngEncoderFilterFlags.AllFilters, zLibLevel: 9);
                var encoded = pixmap.Encode(options);
                if (encoded is not null) return encoded;
            }
        }

        if (format == SKEncodedImageFormat.Webp)
            return EncodeWebpLossy(image, maxOutputBytes);

        return image.Encode(format, 90);
    }

    /// <summary>
    /// Lossy WebP only. Transparency is preserved by the lossy encoder when present; we never
    /// fall back to lossless WebP because that regularly produces files several times larger
    /// than a well-compressed JPEG/PNG source.
    /// </summary>
    private static SKData EncodeWebpLossy(SKImage image, long? maxOutputBytes)
    {
        SKData? best = null;
        try
        {
            for (var quality = WebpStartQuality; quality >= WebpMinQuality; quality -= 10)
            {
                var candidate = EncodeWebpAtQuality(image, quality);
                if (candidate is null) continue;

                if (best is null || candidate.Size < best.Size)
                {
                    best?.Dispose();
                    best = candidate;
                }
                else
                {
                    candidate.Dispose();
                }

                // Stay under the original upload size when possible.
                if (maxOutputBytes is long budget && budget > 0 && best.Size <= budget)
                    break;

                // Without a budget, quality 80 is enough — do not keep stepping for smaller size only.
                if (maxOutputBytes is null)
                    break;
            }

            // Last resort if every attempt failed.
            return best ?? image.Encode(SKEncodedImageFormat.Webp, WebpStartQuality)
                ?? throw new InvalidOperationException("WebP encode failed.");
        }
        catch
        {
            best?.Dispose();
            throw;
        }
    }

    private static SKData? EncodeWebpAtQuality(SKImage image, int quality)
    {
        using var pixmap = image.PeekPixels();
        if (pixmap is not null)
        {
            var options = new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossy, quality);
            var encoded = pixmap.Encode(options);
            if (encoded is not null) return encoded;
        }

        return image.Encode(SKEncodedImageFormat.Webp, quality);
    }

    private static SKBitmap? ApplyCrop(SKBitmap source, ImageCropRect crop)
    {
        var x = (int)Math.Round(Math.Clamp(crop.X, 0, 1) * source.Width);
        var y = (int)Math.Round(Math.Clamp(crop.Y, 0, 1) * source.Height);
        var w = (int)Math.Round(Math.Clamp(crop.Width, 0, 1) * source.Width);
        var h = (int)Math.Round(Math.Clamp(crop.Height, 0, 1) * source.Height);
        w = Math.Clamp(w, 1, source.Width - x);
        h = Math.Clamp(h, 1, source.Height - y);
        if (w <= 0 || h <= 0) return null;

        var rect = new SKRectI(x, y, x + w, y + h);
        var subset = new SKBitmap(w, h);
        if (!source.ExtractSubset(subset, rect))
        {
            subset.Dispose();
            return null;
        }
        return subset;
    }

    private static SKBitmap? ApplyResize(SKBitmap source, int? targetWidth, int? targetHeight)
    {
        var (width, height) = ResolveDimensions(source.Width, source.Height, targetWidth, targetHeight);
        if (width == source.Width && height == source.Height) return null;
        return source.Resize(new SKImageInfo(width, height), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
    }

    private static (int Width, int Height) ResolveDimensions(int sourceWidth, int sourceHeight, int? targetWidth, int? targetHeight)
    {
        if (targetWidth is int w && targetHeight is int h) return (Math.Max(1, w), Math.Max(1, h));
        if (targetWidth is int wOnly)
        {
            var scale = (double)wOnly / sourceWidth;
            return (Math.Max(1, wOnly), Math.Max(1, (int)Math.Round(sourceHeight * scale)));
        }
        if (targetHeight is int hOnly)
        {
            var scale = (double)hOnly / sourceHeight;
            return (Math.Max(1, (int)Math.Round(sourceWidth * scale)), Math.Max(1, hOnly));
        }
        return (sourceWidth, sourceHeight);
    }

    private static SKBitmap ApplyGrayscale(SKBitmap source)
    {
        var result = new SKBitmap(source.Width, source.Height);
        using var canvas = new SKCanvas(result);
        using var paint = new SKPaint
        {
            ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
            {
                0.21f, 0.72f, 0.07f, 0, 0,
                0.21f, 0.72f, 0.07f, 0, 0,
                0.21f, 0.72f, 0.07f, 0, 0,
                0,     0,     0,     1, 0,
            }),
        };
        canvas.DrawBitmap(source, 0, 0, SKSamplingOptions.Default, paint);
        return result;
    }

    private static SKBitmap? ApplyWatermark(SKBitmap source, WatermarkOptions wm)
    {
        using var mark = SKBitmap.Decode(wm.ImageBytes);
        if (mark is null) return null;

        var sizePercent = Math.Clamp(wm.SizePercent, 1, 100);
        var targetWidth = Math.Max(1, (int)Math.Round(source.Width * sizePercent / 100.0));
        var scale = (double)targetWidth / mark.Width;
        var targetHeight = Math.Max(1, (int)Math.Round(mark.Height * scale));

        using var scaledMark = mark.Resize(new SKImageInfo(targetWidth, targetHeight), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
        if (scaledMark is null) return null;

        var (px, py) = ResolvePosition(wm.Position, source.Width, source.Height, targetWidth, targetHeight);

        var result = new SKBitmap(source.Width, source.Height);
        using var canvas = new SKCanvas(result);
        canvas.DrawBitmap(source, 0, 0, SKSamplingOptions.Default);

        var opacity = (byte)Math.Clamp(wm.Opacity, 0, 100) / 100f * 255f;
        using var paint = new SKPaint { Color = SKColors.White.WithAlpha((byte)opacity) };
        canvas.DrawBitmap(scaledMark, px, py, SKSamplingOptions.Default, paint);
        return result;
    }

    private static (float X, float Y) ResolvePosition(WatermarkPosition position, int baseWidth, int baseHeight, int markWidth, int markHeight)
    {
        float x = position switch
        {
            WatermarkPosition.TopLeft or WatermarkPosition.MiddleLeft or WatermarkPosition.BottomLeft => MarginPx,
            WatermarkPosition.TopCenter or WatermarkPosition.Center or WatermarkPosition.BottomCenter => (baseWidth - markWidth) / 2f,
            _ => baseWidth - markWidth - MarginPx,
        };
        float y = position switch
        {
            WatermarkPosition.TopLeft or WatermarkPosition.TopCenter or WatermarkPosition.TopRight => MarginPx,
            WatermarkPosition.MiddleLeft or WatermarkPosition.Center or WatermarkPosition.MiddleRight => (baseHeight - markHeight) / 2f,
            _ => baseHeight - markHeight - MarginPx,
        };
        return (x, y);
    }
}
