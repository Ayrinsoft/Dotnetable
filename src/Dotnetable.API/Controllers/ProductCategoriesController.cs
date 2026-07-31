using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>Public product category tree for the website front-end (resolved from the <c>X-Website-Key</c> header).</summary>
public class ProductCategoriesController : BaseController
{
    private readonly IProductCategoryService _categoryService;
    private readonly IWebsiteService _websiteService;

    public ProductCategoriesController(IProductCategoryService categoryService, IWebsiteService websiteService)
    {
        _categoryService = categoryService;
        _websiteService = websiteService;
    }

    [HttpGet("tree")]
    public async Task<IActionResult> GetTree([FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var tree = await _categoryService.GetTreeAsync(website.WebsiteID, lang, ct);
        return Ok(tree);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, [FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var category = await _categoryService.GetBySlugAsync(website.WebsiteID, slug, lang, ct);
        return category is null ? NotFound() : Ok(category);
    }

    /// <summary>
    /// Filterable attribute facets for a category (including ancestor category attributes).
    /// Use with <c>GET /products?attributeOptionIds=…</c>.
    /// </summary>
    [HttpGet("{slug}/filters")]
    public async Task<IActionResult> GetFilters(string slug, [FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var filters = await _categoryService.GetFilterableAttributesBySlugAsync(website.WebsiteID, slug, lang, ct);
        return Ok(filters);
    }
}
