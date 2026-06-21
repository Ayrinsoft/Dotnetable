using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

public class ProductsController : BaseController
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var products = await _productService.GetByWebsiteAsync(CurrentWebsiteId, activeOnly: true, ct);
        return Ok(products);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, [FromQuery] int languageId, CancellationToken ct = default)
    {
        var product = await _productService.GetBySlugAsync(CurrentWebsiteId, slug, languageId, ct);
        return product is null ? NotFound() : Ok(product);
    }
}
