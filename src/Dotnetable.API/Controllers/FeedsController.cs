using Asp.Versioning;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Serves the product feeds that search engines and price-comparison sites crawl — Torob, Emalls,
/// Google Merchant Center. The URL carries the channel's own unguessable token rather than a website
/// id, so a catalog is only readable by whoever was given the link in the seller panel.
/// </summary>
[ApiController]
[ApiVersionNeutral]
[Route("feeds")]
public sealed class FeedsController : ControllerBase
{
    private readonly IMarketplaceChannelService _channels;

    public FeedsController(IMarketplaceChannelService channels) => _channels = channels;

    /// <summary>Returns the built feed for an active channel, or 404 when the token matches nothing.</summary>
    [HttpGet("{feedToken}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(string feedToken, CancellationToken ct)
    {
        var feed = await _channels.BuildFeedByTokenAsync(feedToken, ct);
        if (feed is null) return NotFound();

        return Content(feed.Content, feed.ContentType);
    }
}
