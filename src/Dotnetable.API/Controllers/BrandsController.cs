using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>Public active brands for the website front-end (resolved from the <c>X-Website-Key</c> header).</summary>
public class BrandsController : BaseController
{
    private readonly IBrandService _brandService;
    private readonly IWebsiteService _websiteService;

    public BrandsController(IBrandService brandService, IWebsiteService websiteService)
    {
        _brandService = brandService;
        _websiteService = websiteService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var brands = await _brandService.GetActiveAsync(website.WebsiteID, lang, ct);
        return Ok(brands);
    }
}
