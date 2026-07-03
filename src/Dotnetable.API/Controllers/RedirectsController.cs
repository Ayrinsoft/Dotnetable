using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Runtime redirect resolution for the website front-end. The public site queries this with the
/// requested path; a match returns the target and status code so the site can issue the redirect.
/// </summary>
public class RedirectsController : BaseController
{
    private readonly IRedirectService _redirectService;
    private readonly IWebsiteService _websiteService;

    public RedirectsController(IRedirectService redirectService, IWebsiteService websiteService)
    {
        _redirectService = redirectService;
        _websiteService = websiteService;
    }

    /// <summary>Resolves a source path to a redirect target, or 204 when no rule matches.</summary>
    [HttpGet("resolve")]
    public async Task<IActionResult> Resolve([FromQuery] string path, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(path)) return NoContent();

        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var result = await _redirectService.ResolveAsync(website.WebsiteID, path, ct);
        return result is null ? NoContent() : Ok(result);
    }
}
