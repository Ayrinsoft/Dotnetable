using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Dotnetable.Infrastructure.Storage;

/// <summary>Generates downscaled JPEG/PNG thumbnails for image uploads using ImageSharp.</summary>
public static class ImageThumbnailer
{
    public const int MaxEdge = 320;

    /// <summary>
    /// Returns a thumbnail stream for <paramref name="source"/>, or null when the bytes are not a
    /// decodable image. The source stream position is reset on entry.
    /// </summary>
    public static async Task<MemoryStream?> TryCreateAsync(Stream source, CancellationToken ct = default)
    {
        if (source.CanSeek) source.Position = 0;
        try
        {
            using var image = await Image.LoadAsync(source, ct);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(MaxEdge, MaxEdge),
            }));

            var output = new MemoryStream();
            await image.SaveAsJpegAsync(output, ct);
            output.Position = 0;
            return output;
        }
        catch
        {
            return null;
        }
        finally
        {
            if (source.CanSeek) source.Position = 0;
        }
    }
}
