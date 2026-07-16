using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// The caller website's own active languages (its subset of the master language catalog, picked in
/// the admin under Content Languages), for the front-end language switcher. Resolved from the
/// <c>X-Website-Key</c> header.
/// </summary>
public class LanguagesController : BaseController
{
    private readonly IWebsiteService _websiteService;
    private readonly ILanguageService _languageService;

    public LanguagesController(IWebsiteService websiteService, ILanguageService languageService)
    {
        _websiteService = websiteService;
        _languageService = languageService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var languages = await _languageService.GetActiveForWebsiteAsync(website.WebsiteID, ct);
        var result = languages
            .OrderBy(l => l.Priority)
            .Select(l => new LanguageDto
            {
                Code = l.LanguageCode,
                CodeISO = l.LanguageCodeISO,
                Name = l.Name,
                IsDefault = l.IsDefault,
                RTLDesign = l.RTLDesign,
            })
            .ToList();

        return Ok(result);
    }
}
