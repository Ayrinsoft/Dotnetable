using Dotnetable.Application.Authorization;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Dotnetable.Hosting;

namespace Dotnetable.API.Controllers;

/// <summary>Converts the signed-in customer's cart into an order.</summary>
[Authorize(Policy = RoleKeys.ClientPurchase)]
[EnableRateLimiting(RateLimiting.CheckoutPolicy)]
public class CheckoutController : BaseController
{
    private readonly IOrderService _orderService;
    private readonly IWebsiteService _websiteService;

    public CheckoutController(IOrderService orderService, IWebsiteService websiteService)
    {
        _orderService = orderService;
        _websiteService = websiteService;
    }

    public sealed record CheckoutRequest(int CartId, int AddressId, int ShippingMethodId, string? Currency, string? Note);

    [HttpPost]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var result = await _orderService.CheckoutAsync(
            website.WebsiteID, CurrentClientId, request.CartId, request.AddressId, request.ShippingMethodId,
            request.Currency, request.Note, ct);

        return result.Success
            ? Ok(new { orderId = result.OrderId, orderNumber = result.OrderNumber })
            : BadRequest(new { message = result.Error });
    }

    private int CurrentClientId =>
        int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var id)
            ? id
            : throw new InvalidOperationException("Token does not carry a client id.");
}
