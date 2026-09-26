using Dotnetable.Application.Authorization;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>Public booking calendar and authenticated reservation. Payment continues on the existing order.</summary>
public class BookingsController : BaseController
{
    private readonly IBookingService _bookings;
    private readonly IWebsiteService _websites;

    public BookingsController(IBookingService bookings, IWebsiteService websites)
    {
        _bookings = bookings;
        _websites = websites;
    }

    [HttpGet("catalog")]
    public async Task<IActionResult> Catalog(CancellationToken ct)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });
        var catalog = await _bookings.GetCatalogAsync(website.WebsiteID, ct);
        return catalog is null ? NotFound() : Ok(catalog);
    }

    [HttpGet("days")]
    public async Task<IActionResult> Days(int offeringId, int year, int month, CancellationToken ct)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });
        if (month is < 1 or > 12 || year < 2000) return BadRequest(new { message = "Invalid month." });
        try
        {
            return Ok(await _bookings.GetMonthAsync(offeringId, year, month, ct));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("slots")]
    public async Task<IActionResult> Slots(int offeringId, string date, CancellationToken ct)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });
        if (!DateOnly.TryParse(date, out var day)) return BadRequest(new { message = "Invalid date." });
        try
        {
            return Ok(await _bookings.GetSlotsAsync(offeringId, day, ct));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    public sealed record BookRequest(int OfferingId, string Date, string Start, string? Note);

    [Authorize(Policy = RoleKeys.ClientPurchase)]
    [HttpPost]
    public async Task<IActionResult> Book([FromBody] BookRequest request, CancellationToken ct)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });
        if (!DateOnly.TryParse(request.Date, out var date) || !TimeOnly.TryParse(request.Start, out var start))
            return BadRequest(new { message = "Invalid date or time." });

        var clientId = int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var id) ? id : 0;
        if (clientId <= 0) return Unauthorized();

        var result = await _bookings.BookAsync(website.WebsiteID, clientId, request.OfferingId, date, start, request.Note, ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
