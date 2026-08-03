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
        [FromQuery] decimal weightKg = 0,
        [FromQuery] decimal cartSubtotal = 0,
        [FromQuery] string? currency = null,
        CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return Unauthorized(new { message = "Invalid or missing website key." });

        var results = await _shipping.GetAvailableWithPricesAsync(
            website.WebsiteID, countryId, stateId, cityId, weightKg, cartSubtotal, ct);
        var storeUsd = await _currency.GetStorePricesInUsdAsync(website.WebsiteID, ct);

        var payload = new List<object>();
        foreach (var r in results)
        {
            async Task<MoneyDto> ToMoney(decimal usd)
            {
                try
                {
                    return await _currency.ToDisplayAsync(website.WebsiteID, usd, storeUsd ? currency : null, ct);
                }
                catch
                {
                    var (code, _) = await _currency.GetActiveRateAsync(website.WebsiteID, null, ct);
                    return new MoneyDto { Amount = usd, AmountUsd = usd, CurrencyCode = code };
                }
            }

            MoneyDto? prepaid = r.PrepaidPriceUsd is decimal p ? await ToMoney(p) : null;
            MoneyDto? cod = r.CodPriceUsd is decimal c ? await ToMoney(c) : null;
            var defaultPrice = prepaid ?? cod!;

            payload.Add(new
            {
                shippingMethodId = r.Method.ShippingMethodID,
                title = r.Method.Title,
                carrierName = r.Method.CarrierName,
                logoUrl = r.Method.LogoFile?.ThumbnailCDN ?? r.Method.LogoFile?.CNDUrl,
                supportsPrepaid = r.Method.SupportsPrepaid,
                supportsCod = r.Method.SupportsCod && (website.AllowCashOnDelivery),
                prepaidPrice = prepaid,
                codPrice = cod,
                isFreeShipping = r.IsFreeShipping,
                // Default charge (prepaid preferred) — backward-compatible fields.
                price = defaultPrice,
                priceUsd = defaultPrice.AmountUsd,
            });
        }

        return Ok(payload);
    }
}
