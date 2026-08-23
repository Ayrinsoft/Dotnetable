using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>Signed-in customer support tickets about orders (storefront).</summary>
[Authorize(Policy = RoleKeys.ClientPurchase)]
public class SupportController : BaseController
{
    private readonly ISupportDeskService _support;
    private readonly IWebsiteService _websites;

    public SupportController(ISupportDeskService support, IWebsiteService websites)
    {
        _support = support;
        _websites = websites;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _support.GetClientSessionsPagedAsync(
            CurrentClientId, new GridQuery { PageIndex = page, PageSize = pageSize }, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var row = await _support.GetClientSessionAsync(id, CurrentClientId, ct);
        if (row is null) return NotFound();
        var timeline = await _support.GetClientInteractionsAsync(id, CurrentClientId, ct);
        return Ok(new { ticket = row, interactions = timeline });
    }

    public sealed class CreateTicketRequest
    {
        public string Subject { get; set; } = "";
        public string Body { get; set; } = "";
        public int? RelatedOrderId { get; set; }
        public byte? Category { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketRequest body, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return BadRequest(new { message = "Website is not resolved." });

        SupportCategory? category = body.Category is byte c && Enum.IsDefined(typeof(SupportCategory), c)
            ? (SupportCategory)c
            : null;

        var (ok, err, session) = await _support.CreateCustomerTicketAsync(
            website.WebsiteID, CurrentClientId, body.Subject, body.Body, body.RelatedOrderId, category, ct);
        if (!ok || session is null) return BadRequest(new { message = err });

        var dto = await _support.GetClientSessionAsync(session.SupportSessionID, CurrentClientId, ct);
        return Ok(dto);
    }

    public sealed class ReplyRequest
    {
        public string Body { get; set; } = "";
    }

    [HttpPost("{id:int}/replies")]
    public async Task<IActionResult> Reply(int id, [FromBody] ReplyRequest body, CancellationToken ct = default)
    {
        var (ok, err) = await _support.AddCustomerReplyAsync(id, CurrentClientId, body.Body, ct);
        return ok ? Ok() : BadRequest(new { message = err });
    }

    private int CurrentClientId =>
        int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var id)
            ? id
            : throw new InvalidOperationException("Token does not carry a client id.");
}
