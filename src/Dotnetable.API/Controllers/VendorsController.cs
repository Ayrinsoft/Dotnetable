using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>Public active vendors for the website front-end (resolved from the <c>X-Website-Key</c> header).</summary>
public class VendorsController : BaseController
{
    private readonly IVendorService _vendorService;
    private readonly IWebsiteService _websiteService;

    public VendorsController(IVendorService vendorService, IWebsiteService websiteService)
    {
        _vendorService = vendorService;
        _websiteService = websiteService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var vendors = await _vendorService.GetActiveAsync(website.WebsiteID, lang, ct);
        return Ok(vendors);
    }
}
