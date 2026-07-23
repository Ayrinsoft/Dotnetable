using Dotnetable.Application.Authorization;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Shopping cart for the website front-end. Works for both guests (identified by the
/// <c>X-Cart-Session</c> header, a random key the site stores in a cookie) and signed-in customers
/// (identified by the bearer token's <see cref="ClientClaims.ClientId"/> claim, when present) — no
/// <c>[Authorize]</c> is applied here since guests must be able to shop without signing in.
/// </summary>
public class CartController : BaseController
{
    private const string CartSessionHeader = "X-Cart-Session";

    private readonly ICartService _cartService;
    private readonly IWebsiteService _websiteService;

    public CartController(ICartService cartService, IWebsiteService websiteService)
    {
        _cartService = cartService;
        _websiteService = websiteService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? currency = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var cart = await _cartService.GetOrCreateAsync(website.WebsiteID, CurrentClientId, SessionKey, ct);
        var view = await _cartService.GetCartViewAsync(website.WebsiteID, cart.CartID, currency, ct);
        return Ok(new { view.CartID, cart.SessionKey, view.Items, view.SubTotal, view.CouponCode, view.DiscountAmount, view.CouponError, view.TotalWeightKg });
    }

    public sealed record AddItemRequest(int VariantId, int Quantity, int? VendorProductId, int? VendorId);

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddItemRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var cart = await _cartService.GetOrCreateAsync(website.WebsiteID, CurrentClientId, SessionKey, ct);
        await _cartService.AddItemAsync(website.WebsiteID, cart.CartID, request.VariantId, request.Quantity, request.VendorProductId, request.VendorId, ct);
        var view = await _cartService.GetCartViewAsync(website.WebsiteID, cart.CartID, null, ct);
        return Ok(new { view.CartID, cart.SessionKey, view.Items, view.SubTotal });
    }

    public sealed record UpdateQuantityRequest(int Quantity);

    [HttpPut("items/{cartItemId:int}")]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, [FromBody] UpdateQuantityRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var cart = await _cartService.GetOrCreateAsync(website.WebsiteID, CurrentClientId, SessionKey, ct);
        await _cartService.UpdateQuantityAsync(cart.CartID, cartItemId, request.Quantity, ct);
        return Ok();
    }

    [HttpDelete("items/{cartItemId:int}")]
    public async Task<IActionResult> RemoveItem(int cartItemId, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var cart = await _cartService.GetOrCreateAsync(website.WebsiteID, CurrentClientId, SessionKey, ct);
        await _cartService.RemoveItemAsync(cart.CartID, cartItemId, ct);
        return Ok();
    }

    public sealed record ApplyCouponRequest(string Code);

    [HttpPost("coupon")]
    public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var cart = await _cartService.GetOrCreateAsync(website.WebsiteID, CurrentClientId, SessionKey, ct);
        var (success, error) = await _cartService.ApplyCouponAsync(website.WebsiteID, cart.CartID, request.Code, CurrentClientId, ct);
        return success ? Ok() : BadRequest(new { message = error });
    }

    [HttpDelete("coupon")]
    public async Task<IActionResult> RemoveCoupon(CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var cart = await _cartService.GetOrCreateAsync(website.WebsiteID, CurrentClientId, SessionKey, ct);
        await _cartService.RemoveCouponAsync(cart.CartID, ct);
        return Ok();
    }

    /// <summary>Called right after a client logs in, to fold their guest cart into their account cart.</summary>
    [HttpPost("merge")]
    public async Task<IActionResult> Merge(CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });
        if (CurrentClientId is not int clientId) return Unauthorized();
        if (string.IsNullOrEmpty(SessionKey)) return Ok();

        await _cartService.MergeGuestCartAsync(website.WebsiteID, SessionKey, clientId, ct);
        return Ok();
    }

    // ── Helpers ─────────────────────────────────────────────────────

    private int? CurrentClientId =>
        int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var id) ? id : null;

    private string? SessionKey =>
        Request.Headers.TryGetValue(CartSessionHeader, out var v) && !string.IsNullOrWhiteSpace(v) ? v.ToString() : null;
}
