using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>The signed-in customer's own order history/detail.</summary>
[Authorize(Policy = RoleKeys.ClientPurchase)]
public class OrdersController : BaseController
{
    private readonly IOrderService _orderService;
    private readonly IWebsiteService _website;

    public OrdersController(IOrderService orderService, IWebsiteService websiteService)
    {
        _orderService = orderService;
        _website = websiteService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _orderService.GetClientHistoryAsync(CurrentClientId, new GridQuery { PageIndex = page, PageSize = pageSize }, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var order = await _orderService.GetByIdAsync(id, CurrentClientId, ct);
        return order is null ? NotFound() : Ok(order);
    }

    /// <summary>Records the signed-in customer's flight or hotel hold, with the price and tax frozen on the line.</summary>
    [HttpPost("travel")]
    public async Task<IActionResult> RecordTravel([FromBody] TravelOrderWrite? write, CancellationToken ct = default)
    {
        if (write is null)
            return BadRequest(new { message = "Order is required." });
        if (await ResolveWebsiteAsync(_website, ct) is not { } website)
            return BadRequest(new { message = "Unknown or missing website." });

        var result = await _orderService.RecordTravelAsync(website.WebsiteID, CurrentClientId, write, ct);
        return result.Success
            ? Ok(new { orderId = result.OrderId, orderNumber = result.OrderNumber })
            : BadRequest(new { message = result.Error });
    }

    private int CurrentClientId =>
        int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var id)
            ? id
            : throw new InvalidOperationException("Token does not carry a client id.");
}
