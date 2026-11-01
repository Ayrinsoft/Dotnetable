namespace Dotnetable.Application.Interfaces;

/// <summary>On-the-fly watermark of a website-1 clean catalog image for the website resolved by <c>X-Website-Key</c>.</summary>
public interface IPlatformProductImageRenderer
{
    /// <summary>Returns image bytes, or null when the file must not be served through this endpoint.</summary>
    Task<PlatformProductImageBytes?> RenderAsync(int viewerWebsiteId, int fileId, bool thumbnail, CancellationToken ct = default);
}

public sealed record PlatformProductImageBytes(byte[] Content, string ContentType);
