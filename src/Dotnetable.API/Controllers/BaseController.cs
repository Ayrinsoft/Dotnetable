using Dotnetable.API.Versioning;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Public storefront controllers. Implicit API version is <see cref="ApiVersions.V1"/> via
/// <c>X-Api-Version</c> (defaults to 1.0). To ship a breaking change without dropping old
/// clients: keep this controller as-is and add a sibling with
/// <c>[ApiVersion("2.0")]</c> and the same <c>[Route("api/…")]</c>.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    /// <summary>Header carrying the caller website's per-site key (<see cref="Website.AuthCode"/>).</summary>
    public const string WebsiteKeyHeader = "X-Website-Key";

    /// <summary>Header that selects the API contract. See <see cref="ApiVersions.HeaderName"/>.</summary>
    public const string ApiVersionHeader = ApiVersions.HeaderName;

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
