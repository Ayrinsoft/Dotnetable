using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>Public (guest-or-client) coupon validation for the storefront cart — no redemption happens here.</summary>
public class CouponsController : BaseController
{
    private readonly ICouponService _coupons;
    private readonly IWebsiteService _websites;

    public CouponsController(ICouponService coupons, IWebsiteService websites)
    {
        _coupons = coupons;
        _websites = websites;
    }

    /// <summary>
    /// Validates a coupon code against a cart subtotal and returns the discount it would yield.
    /// clientId, when supplied, is resolved and trusted by the caller (e.g. the Cart domain) — this
    /// endpoint does not read a bearer token itself, so guest carts can validate coupons too.
    /// </summary>
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateCouponRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return Unauthorized(new { message = "Invalid or missing website key." });

        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest(new { message = "Coupon code is required." });

        var (valid, error, discountUsd) = await _coupons.ValidateAndComputeAsync(
            website.WebsiteID, request.Code.Trim(), request.ClientId, request.CartSubtotalUsd, ct);

        return Ok(new { valid, error, discountUsd });
    }
}

public sealed class ValidateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public decimal CartSubtotalUsd { get; set; }
    public int? ClientId { get; set; }
}
