using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>Public storefront shipping quotes — available methods and their resolved prices for a destination + weight.</summary>
public class ShippingController : BaseController
{
    private readonly IShippingService _shipping;
    private readonly IWebsiteService _websites;
    private readonly ICurrencyConversionService _currency;

    public ShippingController(IShippingService shipping, IWebsiteService websites, ICurrencyConversionService currency)
    {
        _shipping = shipping;
        _websites = websites;
        _currency = currency;
    }

    [HttpGet("methods")]
    public async Task<IActionResult> GetMethods(
        [FromQuery] int? countryId, [FromQuery] int? stateId, [FromQuery] int? cityId,
        [FromQuery] decimal weightKg = 0, [FromQuery] string? currency = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return Unauthorized(new { message = "Invalid or missing website key." });

        var results = await _shipping.GetAvailableWithPricesAsync(website.WebsiteID, countryId, stateId, cityId, weightKg, ct);
        var storeUsd = await _currency.GetStorePricesInUsdAsync(website.WebsiteID, ct);

        var payload = new List<object>();
        foreach (var r in results)
        {
            // PriceUsd column is dual/bridge; for single-currency sites it mirrors site amount.
            MoneyDto price;
            try
            {
                // Prefer converting from local when shipping rate has site Price authority.
                // GetAvailableWithPrices currently returns USD dual; use ToDisplayAsync then optionally
                // map through site mode.
                price = await _currency.ToDisplayAsync(website.WebsiteID, r.PriceUsd, storeUsd ? currency : null, ct);
            }
            catch
            {
                var (code, _) = await _currency.GetActiveRateAsync(website.WebsiteID, null, ct);
                price = new MoneyDto { Amount = r.PriceUsd, AmountUsd = r.PriceUsd, CurrencyCode = code };
            }

            payload.Add(new
            {
                shippingMethodId = r.Method.ShippingMethodID,
                title = r.Method.Title,
                carrierName = r.Method.CarrierName,
                price,
                // Backward-compatible field (same as price.AmountUsd / bridge).
                priceUsd = price.AmountUsd,
            });
        }

        return Ok(payload);
    }
}
