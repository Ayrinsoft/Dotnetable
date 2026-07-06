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

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var info = await _websiteService.GetSiteInfoAsync(website.WebsiteID, ct);
        return info is null ? NoContent() : Ok(info);
    }
}
