using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

public class LocalizationController : BaseController
{
    private readonly ILocalizationService _localizationService;

    public LocalizationController(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    [HttpGet("{languageId}")]
    public async Task<IActionResult> GetAll(int languageId, CancellationToken ct = default)
    {
        var translations = await _localizationService.GetAllAsync(CurrentWebsiteId, languageId, ct);
        return Ok(translations);
    }

    [HttpPut("{languageId}/{key}")]
    public async Task<IActionResult> Set(int languageId, string key, [FromBody] string value, CancellationToken ct = default)
    {
        await _localizationService.SetAsync(CurrentWebsiteId, languageId, key, value, ct);
        return NoContent();
    }
}
