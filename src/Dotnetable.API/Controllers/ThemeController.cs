using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// The caller website's active visual theme. The React SPA (serverless mode) fetches this on boot
/// and applies the design tokens as CSS custom properties, so the admin can re-theme a deployed
/// site without rebuilding it.
/// </summary>
public class ThemeController : BaseController
{
    private readonly IThemeService _themeService;
    private readonly IWebsiteService _websiteService;

    public ThemeController(IThemeService themeService, IWebsiteService websiteService)
    {
        _themeService = themeService;
        _websiteService = websiteService;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var theme = await _themeService.GetActiveThemeAsync(website.WebsiteID, ct);
        if (theme is null) return NoContent();

        return Ok(new
        {
            theme.WebsiteThemeID,
            theme.Name,
            // Serialized token bag; the SPA parses it into CSS custom properties.
            Settings = theme.SettingsJson,
        });
    }
}
