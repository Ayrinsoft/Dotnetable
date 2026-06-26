using Dotnetable.Application.Authorization;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

public class LocalizationController : BaseController
{
    private readonly ILocalizationService _localizationService;

    public LocalizationController(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    [HttpGet("{languageCode}")]
    public async Task<IActionResult> GetAll(string languageCode, CancellationToken ct = default)
    {
        var translations = await _localizationService.GetAllAsync(CurrentWebsiteId, languageCode, ct);
        return Ok(translations);
    }

    [HttpPut("{languageCode}/{key}")]
    [Authorize(Policy = RoleKeys.LocalizationEdit)]
    public async Task<IActionResult> Set(string languageCode, string key, [FromBody] string value, CancellationToken ct = default)
    {
        await _localizationService.SetAsync(CurrentWebsiteId, languageCode, key, value, ct);
        return NoContent();
    }
}
