using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Customer digital library — permanent access to purchased download links, codes, and service URLs,
/// with access logging. Downloadables are external URLs only (no file hosting).
/// </summary>
[Authorize(Policy = RoleKeys.ClientPurchase)]
public class DigitalController : BaseController
{
    private readonly IDigitalDeliveryService _digital;
    private readonly IWebsiteService _websites;

    public DigitalController(IDigitalDeliveryService digital, IWebsiteService websites)
    {
        _digital = digital;
        _websites = websites;
    }

    [HttpGet]
    public async Task<IActionResult> GetLibrary(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return Unauthorized(new { message = "Invalid or missing website key." });

        var result = await _digital.GetClientLibraryAsync(
            website.WebsiteID, CurrentClientId,
            new GridQuery { PageIndex = page, PageSize = pageSize }, ct);
        return Ok(result);
    }

    [HttpGet("order/{orderId:int}")]
    public async Task<IActionResult> GetByOrder(int orderId, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return Unauthorized(new { message = "Invalid or missing website key." });

        var items = await _digital.GetByOrderAsync(website.WebsiteID, CurrentClientId, orderId, ct);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDetail(int id, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return Unauthorized(new { message = "Invalid or missing website key." });

        var (ok, _) = await _digital.LogAccessAsync(
            website.WebsiteID, CurrentClientId, id, DigitalAccessType.View,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(), ct);
        if (!ok) return NotFound();

        var detail = await _digital.GetDetailAsync(website.WebsiteID, CurrentClientId, id, ct);
        return detail is null ? NotFound() : Ok(detail);
    }

    [HttpPost("{id:int}/access")]
    public async Task<IActionResult> LogAccess(int id, [FromBody] AccessRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return Unauthorized(new { message = "Invalid or missing website key." });

        var type = request.AccessType switch
        {
            "download" => DigitalAccessType.Download,
            "code" => DigitalAccessType.ViewCode,
            "service" => DigitalAccessType.ViewService,
            _ => DigitalAccessType.View,
        };

        var (ok, err) = await _digital.LogAccessAsync(
            website.WebsiteID, CurrentClientId, id, type,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(), ct);
        return ok ? Ok(new { success = true }) : NotFound(new { message = err });
    }

    /// <summary>
    /// Logs download access and returns the external download URL.
    /// The binary is never hosted here — clients open the link themselves.
    /// </summary>
    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return Unauthorized(new { message = "Invalid or missing website key." });

        var result = await _digital.DownloadAsync(
            website.WebsiteID, CurrentClientId, id,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(), ct);

        if (!result.Success || string.IsNullOrWhiteSpace(result.DownloadUrl))
            return NotFound(new { message = result.Error ?? "Link not found." });

        return Ok(new { downloadUrl = result.DownloadUrl });
    }

    public sealed record AccessRequest(string? AccessType);

    private int CurrentClientId =>
        int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var id)
            ? id
            : throw new InvalidOperationException("Token does not carry a client id.");
}
