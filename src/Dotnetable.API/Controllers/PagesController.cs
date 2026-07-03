using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>Public CMS pages for the website front-end (resolved from the <c>X-Website-Key</c> header).</summary>
public class PagesController : BaseController
{
    private readonly IPageService _pageService;
    private readonly IWebsiteService _websiteService;

    public PagesController(IPageService pageService, IWebsiteService websiteService)
    {
        _pageService = pageService;
        _websiteService = websiteService;
    }

    /// <summary>All active pages as a tree (for footers / sitemaps).</summary>
    [HttpGet]
    public async Task<IActionResult> GetTree([FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var pages = await _pageService.GetTreeAsync(website.WebsiteID, lang, ct);
        return Ok(pages);
    }

    /// <summary>The active homepage page, or 204 when none is set.</summary>
    [HttpGet("home")]
    public async Task<IActionResult> GetHomepage([FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var page = await _pageService.GetHomepageAsync(website.WebsiteID, lang, ct);
        return page is null ? NoContent() : Ok(page);
    }

    /// <summary>A single active page by slug (base or translated).</summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, [FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var page = await _pageService.GetBySlugAsync(website.WebsiteID, slug, lang, ct);
        return page is null ? NotFound() : Ok(page);
    }
}
