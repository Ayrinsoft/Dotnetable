using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Public keyword advertisements for the website front-end. The caller website is resolved from the
/// <c>X-Website-Key</c> header. An optional <c>lang</c> query parameter selects per-ad translated
/// keyword/URL; missing translations fall back to the default language.
/// </summary>
public class AdvertisementController : BaseController
{
    private readonly IAdvertisementService _advertisementService;
    private readonly IWebsiteService _websiteService;

    public AdvertisementController(IAdvertisementService advertisementService, IWebsiteService websiteService)
    {
        _advertisementService = advertisementService;
        _websiteService = websiteService;
    }

    /// <summary>Active advertisements assigned to a location (Header, Footer, Sidebar, Home, Product, Blog, Page).</summary>
    [HttpGet("{location}")]
    public async Task<IActionResult> GetByLocation(string location, [FromQuery] string? lang = null, CancellationToken ct = default)
    {
        if (!TryParseLocation(location, out var parsed))
            return BadRequest(new { message = $"Unknown advertisement location '{location}'." });

        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var ads = await _advertisementService.GetByLocationAsync(website.WebsiteID, parsed, lang, ct);
        return Ok(ads);
    }

    private static bool TryParseLocation(string value, out AdvertisementLocation location)
    {
        if (Enum.TryParse(value, ignoreCase: true, out location) && Enum.IsDefined(location))
            return true;
        location = default;
        return false;
    }
}
