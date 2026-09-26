using System.Security.Cryptography;
using System.Text;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Write API for catalog agents. The storefront key (<c>X-Website-Key</c>) only identifies the website.
/// Writes also require <c>X-Catalog-Agent-Key</c>, generated on the admin products page, so a public
/// storefront key cannot change products.
/// </summary>
[ApiController]
[Route("api/product-agent")]
public sealed class ProductAgentController : ControllerBase
{
    public const string AgentKeyHeader = "X-Catalog-Agent-Key";

    private readonly IProductAgentService _agent;
    private readonly IWebsiteService _websites;

    public ProductAgentController(IProductAgentService agent, IWebsiteService websites)
    {
        _agent = agent;
        _websites = websites;
    }

    /// <summary>Current product in the agent JSON shape. <paramref name="key"/> is a slug or a product code such as DN-42. Drafts are included.</summary>
    [HttpGet("{key}")]
    public async Task<IActionResult> Get(string key, CancellationToken ct)
    {
        var website = await AuthorizeAsync(ct);
        if (website is null) return Unauthorized(new { message = "Website key or catalog agent key was rejected." });

        try
        {
            var productId = await _agent.FindProductIdAsync(website.WebsiteID, key, ct);
            if (productId is null) return NotFound(new { message = "Product was not found." });
            var json = await _agent.ExportJsonAsync(productId.Value, ct);
            return Content(json, "application/json", Encoding.UTF8);
        }
        catch (ProductAgentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Creates or updates a product from the agent JSON body.
    /// Match order is product_code, then slug. Omitted keys are left unchanged.
    /// Prices are in the website currency. Images, seller stock, and related products are not changed.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Upsert(CancellationToken ct)
    {
        var website = await AuthorizeAsync(ct);
        if (website is null) return Unauthorized(new { message = "Website key or catalog agent key was rejected." });

        string json;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            json = await reader.ReadToEndAsync(ct);
        if (Encoding.UTF8.GetByteCount(json) > ProductAgentParser.MaxBytes)
            return BadRequest(new { message = "JSON body is larger than 2 MB." });

        try
        {
            var result = await _agent.ApplyAsync(website.WebsiteID, json, actingMemberId: null, restrictToCreatedByMemberId: null, ct);
            return Ok(new
            {
                product_id = result.ProductId,
                product_code = result.ProductCode,
                slug = result.Slug,
                created = result.Created,
                notes = result.Notes,
            });
        }
        catch (ProductAgentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private async Task<Website?> AuthorizeAsync(CancellationToken ct)
    {
        if (!Request.Headers.TryGetValue(BaseController.WebsiteKeyHeader, out var siteKey)
            || !Guid.TryParse(siteKey.ToString(), out var authCode))
            return null;
        if (!Request.Headers.TryGetValue(AgentKeyHeader, out var agentHeader))
            return null;

        var website = await _websites.GetByAuthCodeAsync(authCode, ct);
        if (website is not { Active: true } || website.CatalogAgentKey is not Guid expected)
            return null;
        if (!Guid.TryParse(agentHeader.ToString(), out var provided))
            return null;
        if (!CryptographicOperations.FixedTimeEquals(expected.ToByteArray(), provided.ToByteArray()))
            return null;
        return website;
    }
}
