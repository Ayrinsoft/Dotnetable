using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Public active theme for the caller website. The MVC Web host uses this to resolve the view
/// root (WordPress-style package under Themes/{viewRoot}).
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
        return Ok(theme);
    }
}
