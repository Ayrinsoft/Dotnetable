using Asp.Versioning;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    /// <summary>Header carrying the caller website's per-site key (<see cref="Website.AuthCode"/>).</summary>
    public const string WebsiteKeyHeader = "X-Website-Key";

    /// <summary>Website scope for the request, taken from the <c>X-Website-Id</c> header.</summary>
    protected int CurrentWebsiteId =>
        Request.Headers.TryGetValue("X-Website-Id", out var value) && int.TryParse(value, out var websiteId)
            ? websiteId
            : throw new InvalidOperationException("X-Website-Id header is missing or invalid.");

    /// <summary>Resolves the caller's active website from the <c>X-Website-Key</c> header (its AuthCode),
    /// or null when the header is missing/invalid or the website is inactive.</summary>
    protected async Task<Website?> ResolveWebsiteAsync(IWebsiteService websiteService, CancellationToken ct)
    {
        if (!Request.Headers.TryGetValue(WebsiteKeyHeader, out var keyValue) ||
            !Guid.TryParse(keyValue.ToString(), out var authCode))
            return null;

        var website = await websiteService.GetByAuthCodeAsync(authCode, ct);
        return website is { Active: true } ? website : null;
    }
}
