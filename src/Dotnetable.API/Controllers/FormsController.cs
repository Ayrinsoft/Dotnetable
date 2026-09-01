using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Dotnetable.Hosting;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Public endpoints of the dynamic form / survey engine. The caller website is resolved from the
/// <c>X-Website-Key</c> header. A form can be fetched by slug (standalone page) or by id (the
/// <c>[form:ID]</c> shortcode embedded in Post/Page content); submissions are validated and stored
/// server-side. Building forms and reading reports happens in the Admin app, not through this API.
/// </summary>
[EnableRateLimiting(RateLimiting.PublicWritePolicy)]
public class FormsController : BaseController
{
    private readonly IFormService _formService;
    private readonly IWebsiteService _websiteService;

    public FormsController(IFormService formService, IWebsiteService websiteService)
    {
        _formService = formService;
        _websiteService = websiteService;
    }

    /// <summary>An active form/survey by slug, with its renderable field definitions.</summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var form = await _formService.GetPublicFormAsync(website.WebsiteID, slug, ct);
        return form is null ? NoContent() : Ok(form);
    }

    /// <summary>An active form/survey by id — used to resolve the <c>[form:ID]</c> shortcode.</summary>
    [HttpGet("id/{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var form = await _formService.GetPublicFormByIdAsync(website.WebsiteID, id, ct);
        return form is null ? NoContent() : Ok(form);
    }

    /// <summary>Stores a visitor submission. Works for anonymous visitors; when the request carries a
    /// client bearer token the response is linked to that client (required for RequireLogin forms).</summary>
    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> Submit(int id, [FromBody] FormSubmissionRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        int? clientId = int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var cid) ? cid : null;
        var senderIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var result = await _formService.SubmitAsync(website.WebsiteID, id, clientId, senderIp, request, ct);
        return result.Ok ? Ok(result) : BadRequest(result);
    }

    /// <summary>Aggregate survey results, only for forms whose admin enabled public results.</summary>
    [HttpGet("{id:int}/results")]
    public async Task<IActionResult> GetResults(int id, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var results = await _formService.GetPublicResultsAsync(website.WebsiteID, id, ct);
        return results is null ? NoContent() : Ok(results);
    }
}
