using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>Public post categories for the website front-end (resolved from the <c>X-Website-Key</c> header).</summary>
public class CategoriesController : BaseController
{
    private readonly ICategoryService _categoryService;
    private readonly IWebsiteService _websiteService;

    public CategoriesController(ICategoryService categoryService, IWebsiteService websiteService)
    {
        _categoryService = categoryService;
        _websiteService = websiteService;
    }

    /// <summary>Active categories as a tree, optionally filtered to a post type.</summary>
    [HttpGet]
    public async Task<IActionResult> GetTree([FromQuery] int? postTypeId = null, [FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var tree = await _categoryService.GetTreeAsync(website.WebsiteID, postTypeId, lang, ct);
        return Ok(tree);
    }

    /// <summary>A single active category by slug (base or translated).</summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, [FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var category = await _categoryService.GetBySlugAsync(website.WebsiteID, slug, lang, ct);
        return category is null ? NotFound() : Ok(category);
    }
}
