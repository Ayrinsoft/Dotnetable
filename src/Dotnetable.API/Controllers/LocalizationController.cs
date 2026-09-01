using Dotnetable.Application.Authorization;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// UI string translations for a website.
///
/// <para>Scope used to come from an <c>X-Website-Id</c> request header. Nothing sent that header, so
/// both actions threw and returned 500 on every call; and had a caller sent one, it was an
/// unauthenticated integer that selected any tenant's data. Scope now comes from the same
/// <c>X-Website-Key</c> every other storefront endpoint uses, which is a secret the site owns.</para>
/// </summary>
public class LocalizationController : BaseController
{
    private readonly ILocalizationService _localizationService;
    private readonly IWebsiteService _websiteService;

    public LocalizationController(ILocalizationService localizationService, IWebsiteService websiteService)
    {
        _localizationService = localizationService;
        _websiteService = websiteService;
    }

    [HttpGet("{languageCode}")]
    public async Task<IActionResult> GetAll(string languageCode, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var translations = await _localizationService.GetAllAsync(website.WebsiteID, languageCode, ct);
        return Ok(translations);
    }

    [HttpPut("{languageCode}/{key}")]
    [Authorize(Policy = RoleKeys.LocalizationEdit)]
    public async Task<IActionResult> Set(string languageCode, string key, [FromBody] string value, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        await _localizationService.SetAsync(website.WebsiteID, languageCode, key, value, ct);
        return NoContent();
    }
}
