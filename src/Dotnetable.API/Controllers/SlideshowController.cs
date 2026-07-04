using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Public image slideshows for the website front-end. The caller website is resolved from the
/// <c>X-Website-Key</c> header (its <see cref="Website.AuthCode"/>). A slideshow can be fetched
/// either by its placement zone (a fixed spot the theme renders, e.g. the header) or by id
/// (used to resolve a <c>[slideshow:ID]</c> shortcode embedded inside a Post/Page body).
/// </summary>
public class SlideshowController : BaseController
{
    private readonly ISlideshowService _slideshowService;
    private readonly IWebsiteService _websiteService;

    public SlideshowController(ISlideshowService slideshowService, IWebsiteService websiteService)
    {
        _slideshowService = slideshowService;
        _websiteService = websiteService;
    }

    /// <summary>The active slideshow assigned to a placement key (e.g. "home_top").</summary>
    [HttpGet("placement/{placementKey}")]
    public async Task<IActionResult> GetByPlacement(string placementKey, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var slideshow = await _slideshowService.GetByPlacementAsync(website.WebsiteID, placementKey, ct);
        return slideshow is null ? NoContent() : Ok(slideshow);
    }

    /// <summary>A single active slideshow by id — used to resolve the <c>[slideshow:ID]</c> shortcode.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var slideshow = await _slideshowService.GetByIdAsync(website.WebsiteID, id, ct);
        return slideshow is null ? NoContent() : Ok(slideshow);
    }
}
