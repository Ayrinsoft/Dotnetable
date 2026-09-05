using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>Published catalog price lists for the storefront (resolved from <c>X-Website-Key</c>).</summary>
public class PriceListsController : BaseController
{
    private readonly IPriceListService _priceLists;
    private readonly IWebsiteService _websiteService;

    public PriceListsController(IPriceListService priceLists, IWebsiteService websiteService)
    {
        _priceLists = priceLists;
        _websiteService = websiteService;
    }

    /// <summary>Active price lists for this website, with last price-update timestamps.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var lists = await _priceLists.GetPublishedAsync(website.WebsiteID, ct);
        return Ok(lists);
    }

    /// <summary>One active price list by slug, with live display prices (USD base × current FX).</summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var list = await _priceLists.GetPublishedBySlugAsync(website.WebsiteID, slug, ct);
        return list is null ? NotFound() : Ok(list);
    }
}
