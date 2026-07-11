using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>Public captcha challenge for the website front-end (resolved from the <c>X-Website-Key</c> header).</summary>
public class CaptchaController : BaseController
{
    private readonly IWebsiteService _websiteService;
    private readonly IHumanVerificationService _verification;

    public CaptchaController(IWebsiteService websiteService, IHumanVerificationService verification)
    {
        _websiteService = websiteService;
        _verification = verification;
    }

    [HttpGet("challenge")]
    public async Task<IActionResult> Challenge(CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var setting = await _websiteService.GetCaptchaSettingAsync(website.WebsiteID, ct);
        var resolution = _verification.ResolveForWebsite(setting);

        if (resolution.UseTurnstile)
            return Ok(new CaptchaChallengeDto { Provider = "turnstile", SiteKey = resolution.TurnstileSiteKey });

        var challenge = _verification.CreateMathChallenge();
        return Ok(new CaptchaChallengeDto { Provider = "math", Token = challenge.Token, Svg = challenge.Svg });
    }
}
