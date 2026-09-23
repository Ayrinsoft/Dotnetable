using SkiaSharp;

namespace Dotnetable.Infrastructure.Storage;

/// <summary>
/// Decodes an uploaded image with its EXIF orientation baked into the pixels. <c>SKBitmap.Decode</c>
/// ignores the tag, so a portrait phone photo came out sideways once re-encoded (WebP carries no EXIF
/// forward to let the browser fix it).
/// </summary>
internal static class ImageDecoder
{
    /// <summary>
    /// Returns the upright bitmap, or null when the bytes are not a decodable image. Reads from the
    /// stream's current position and never takes ownership of it.
    /// </summary>
    public static SKBitmap? DecodeOriented(Stream source)
    {
        using var data = ReadAll(source);
        using var codec = SKCodec.Create(data);
        if (codec is null) return null;

        var decoded = SKBitmap.Decode(codec);
        if (decoded is null) return null;

        var oriented = ApplyOrigin(decoded, codec.EncodedOrigin);
        if (!ReferenceEquals(oriented, decoded)) decoded.Dispose();
        return oriented;
    }

    private static SKData ReadAll(Stream source)
    {
        if (source is MemoryStream ms && ms.TryGetBuffer(out var segment))
        {
            var start = (int)ms.Position;
            return SKData.CreateCopy(segment.AsSpan(start, (int)ms.Length - start));
        }

        using var copy = new MemoryStream();
        source.CopyTo(copy);
        return SKData.CreateCopy(copy.GetBuffer().AsSpan(0, (int)copy.Length));
    }

    /// <summary>Maps the stored pixels onto the upright canvas for each of the eight EXIF orientations.</summary>
    private static SKBitmap ApplyOrigin(SKBitmap src, SKEncodedOrigin origin)
    {
        int w = src.Width, h = src.Height;
        var (width, height, matrix) = origin switch
        {
            SKEncodedOrigin.TopRight => (w, h, new SKMatrix(-1, 0, w, 0, 1, 0, 0, 0, 1)),
            SKEncodedOrigin.BottomRight => (w, h, new SKMatrix(-1, 0, w, 0, -1, h, 0, 0, 1)),
            SKEncodedOrigin.BottomLeft => (w, h, new SKMatrix(1, 0, 0, 0, -1, h, 0, 0, 1)),
            SKEncodedOrigin.LeftTop => (h, w, new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1)),
            SKEncodedOrigin.RightTop => (h, w, new SKMatrix(0, -1, h, 1, 0, 0, 0, 0, 1)),
            SKEncodedOrigin.RightBottom => (h, w, new SKMatrix(0, -1, h, -1, 0, w, 0, 0, 1)),
            SKEncodedOrigin.LeftBottom => (h, w, new SKMatrix(0, 1, 0, -1, 0, w, 0, 0, 1)),
            _ => (w, h, SKMatrix.Identity),
        };
        if (matrix.IsIdentity) return src;

        var result = new SKBitmap(new SKImageInfo(width, height, src.ColorType, src.AlphaType, src.ColorSpace));
        using var canvas = new SKCanvas(result);
        canvas.SetMatrix(matrix);
        canvas.DrawBitmap(src, 0, 0, SKSamplingOptions.Default);
        return result;
    }
}
