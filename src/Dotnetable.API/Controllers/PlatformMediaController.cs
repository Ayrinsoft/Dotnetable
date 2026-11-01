using Asp.Versioning;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// On-the-fly watermark for a platform catalog image. The caller is the destination website
/// (<c>X-Website-Key</c>). The response is the image bytes, never a redirect to the clean file.
/// </summary>
[ApiController]
[ApiVersionNeutral]
[Route("api/media")]
public sealed class PlatformMediaController : BaseController
{
    private readonly IPlatformProductImageRenderer _renderer;
    private readonly IWebsiteService _websites;

    public PlatformMediaController(IPlatformProductImageRenderer renderer, IWebsiteService websites)
    {
        _renderer = renderer;
        _websites = websites;
    }

    [HttpGet("platform-image/{fileId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(int fileId, [FromQuery] string? s, CancellationToken ct)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return NotFound();

        var thumbnail = !string.Equals(s, "f", StringComparison.OrdinalIgnoreCase);
        var image = await _renderer.RenderAsync(website.WebsiteID, fileId, thumbnail, ct);
        if (image is null) return NotFound();

        Response.Headers.CacheControl = "private, max-age=120";
        return File(image.Content, image.ContentType);
    }
}
