using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Public navigation menus for the website front-end. The caller website is resolved from the
/// <c>X-Website-Key</c> header (its <see cref="Website.AuthCode"/>); an optional <c>lang</c> query
/// parameter selects per-item translated titles.
/// </summary>
public class MenuController : BaseController
{
    /// <summary>Header carrying the caller website's per-site key (<see cref="Website.AuthCode"/>).</summary>
    public const string WebsiteKeyHeader = "X-Website-Key";

    private readonly IMenuService _menuService;
    private readonly IWebsiteService _websiteService;

    public MenuController(IMenuService menuService, IWebsiteService websiteService)
    {
        _menuService = menuService;
        _websiteService = websiteService;
    }

    /// <summary>All active menus for the caller website, one per location.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var menus = await _menuService.GetActiveMenusAsync(website.WebsiteID, lang, ct);
        return Ok(menus);
    }

    /// <summary>The active menu assigned to a location (Header, Footer, Sidebar, Mobile).</summary>
    [HttpGet("{location}")]
    public async Task<IActionResult> GetByLocation(string location, [FromQuery] string? lang = null, CancellationToken ct = default)
    {
        if (!TryParseLocation(location, out var parsed))
            return BadRequest(new { message = $"Unknown menu location '{location}'." });

        var website = await ResolveWebsiteAsync(ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var menu = await _menuService.GetByLocationAsync(website.WebsiteID, parsed, lang, ct);
        return menu is null ? NoContent() : Ok(menu);
    }

    // ── Helpers ─────────────────────────────────────────────────────

    private static bool TryParseLocation(string value, out MenuLocation location)
    {
        // Accept both the name ("Header") and the numeric value ("1").
        if (Enum.TryParse(value, ignoreCase: true, out location) && Enum.IsDefined(location))
            return true;
        location = default;
        return false;
    }

    private async Task<Website?> ResolveWebsiteAsync(CancellationToken ct)
    {
        if (!Request.Headers.TryGetValue(WebsiteKeyHeader, out var keyValue) ||
            !Guid.TryParse(keyValue.ToString(), out var authCode))
            return null;

        var website = await _websiteService.GetByAuthCodeAsync(authCode, ct);
        return website is { Active: true } ? website : null;
    }
}
