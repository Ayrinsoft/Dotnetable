using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>Public, derived stock availability for the website front-end (cart/PDP badges).
/// Never exposes raw QuantityOnHand/QuantityReserved — those are internal admin figures.</summary>
public class InventoryController : BaseController
{
    private readonly IInventoryService _inventoryService;
    private readonly IWebsiteService _websiteService;

    public InventoryController(IInventoryService inventoryService, IWebsiteService websiteService)
    {
        _inventoryService = inventoryService;
        _websiteService = websiteService;
    }

    /// <summary>Availability for a comma-separated list of variant ids, e.g. <c>?variantIds=1,2,3</c>.</summary>
    [HttpGet("availability")]
    public async Task<IActionResult> GetAvailability([FromQuery] string variantIds, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var ids = (variantIds ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var id) ? id : (int?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (ids.Count == 0) return Ok(new Dictionary<int, object>());

        var availability = await _inventoryService.GetAvailabilityBulkAsync(website.WebsiteID, ids, ct);

        var result = availability.ToDictionary(
            kv => kv.Key,
            kv => new
            {
                IsInStock = kv.Value.Available > 0,
                MaxPurchasable = Math.Max(0, kv.Value.Available),
            });

        return Ok(result);
    }
}
