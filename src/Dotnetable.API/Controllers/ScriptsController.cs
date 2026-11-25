using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Active custom snippets for the caller website (Google tags, webmaster verification, and the
/// other rows under Website Settings → Scripts). Resolved from <c>X-Website-Key</c>.
/// </summary>
public class ScriptsController : BaseController
{
    private readonly IWebsiteSettingService _settings;
    private readonly IWebsiteService _websites;

    public ScriptsController(IWebsiteSettingService settings, IWebsiteService websites)
    {
        _settings = settings;
        _websites = websites;
    }

    [HttpGet]
    public async Task<IActionResult> GetActive(CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var scripts = await _settings.GetPublicScriptsAsync(website.WebsiteID, ct);
        return Ok(scripts);
    }
}
