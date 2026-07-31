using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>Public product catalog reads for the website front-end (resolved from the <c>X-Website-Key</c> header).</summary>
public class ProductsController : BaseController
{
    private readonly IProductService _productService;
    private readonly IWebsiteService _websiteService;

    public ProductsController(IProductService productService, IWebsiteService websiteService)
    {
        _productService = productService;
        _websiteService = websiteService;
    }

    /// <summary>Paged/filterable published product listing, priced in the requested display currency.</summary>
    /// <param name="inStock">When true/false, only in-stock or out-of-stock products. Omit for all.</param>
    /// <param name="attributeOptionIds">Optional facet filter: product must have every listed attribute option (AND).</param>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? categorySlug = null, [FromQuery] string? brandSlug = null, [FromQuery] string? search = null,
        [FromQuery] decimal? minPrice = null, [FromQuery] decimal? maxPrice = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? lang = null, [FromQuery] string? currency = null,
        [FromQuery] bool? inStock = null, [FromQuery] int[]? attributeOptionIds = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var result = await _productService.GetPublishedAsync(
            website.WebsiteID, categorySlug, brandSlug, search, minPrice, maxPrice, page, pageSize, lang, currency, inStock, attributeOptionIds, ct);
        return Ok(result);
    }

    /// <summary>A single published product by slug (base or translated), fully detailed and priced.</summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, [FromQuery] string? lang = null, [FromQuery] string? currency = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var product = await _productService.GetBySlugAsync(website.WebsiteID, slug, lang, currency, ct);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>Products related to (or category-derived from) the given product.</summary>
    [HttpGet("{slug}/related")]
    public async Task<IActionResult> GetRelated(string slug, [FromQuery] int take = 8, [FromQuery] string? lang = null, [FromQuery] string? currency = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var related = await _productService.GetRelatedAsync(website.WebsiteID, slug, take, lang, currency, ct);
        return Ok(related);
    }
}
