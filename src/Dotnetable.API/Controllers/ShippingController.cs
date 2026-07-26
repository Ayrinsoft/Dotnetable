using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>Public storefront shipping quotes — available methods and their resolved prices for a destination + weight.</summary>
public class ShippingController : BaseController
{
    private readonly IShippingService _shipping;
    private readonly IWebsiteService _websites;

    public ShippingController(IShippingService shipping, IWebsiteService websites)
    {
        _shipping = shipping;
        _websites = websites;
    }

    [HttpGet("methods")]
    public async Task<IActionResult> GetMethods(
        [FromQuery] int? countryId, [FromQuery] int? stateId, [FromQuery] int? cityId,
        [FromQuery] decimal weightKg = 0, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return Unauthorized(new { message = "Invalid or missing website key." });

        var results = await _shipping.GetAvailableWithPricesAsync(website.WebsiteID, countryId, stateId, cityId, weightKg, ct);

        return Ok(results.Select(r => new
        {
            shippingMethodId = r.Method.ShippingMethodID,
            title = r.Method.Title,
            carrierName = r.Method.CarrierName,
            // USD bridge for multi-currency storefront conversion; site amount derived client-side via rates when needed.
            priceUsd = r.PriceUsd,
        }));
    }
}
