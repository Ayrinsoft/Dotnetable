using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Public branding/identity of the caller website (resolved from the <c>X-Website-Key</c> header):
/// brand name, logo, contact details, social links and SEO defaults. The Web/React layouts render
/// from this instead of hardcoded placeholder text.
/// </summary>
public class SiteInfoController : BaseController
{
    private readonly IWebsiteService _websiteService;

    public SiteInfoController(IWebsiteService websiteService) => _websiteService = websiteService;

    /// <summary>An optional <c>lang</c> query parameter selects translated contact info fields;
    /// missing translations fall back to the default language.</summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var info = await _websiteService.GetSiteInfoAsync(website.WebsiteID, lang, ct);
        return info is null ? NoContent() : Ok(info);
    }
}
