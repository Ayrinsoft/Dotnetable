using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Dotnetable.Hosting;
using Microsoft.Extensions.Caching.Memory;

namespace Dotnetable.API.Controllers;

/// <summary>Public contact-form submission for the website front-end (resolved from the <c>X-Website-Key</c> header).</summary>
[EnableRateLimiting(RateLimiting.PublicWritePolicy)]
public class ContactController : BaseController
{
    // Blocks rapid-fire scripted submissions from the same IP without punishing a human who
    // double-clicks; the captcha is the real bot gate.
    private static readonly TimeSpan ThrottleWindow = TimeSpan.FromSeconds(20);

    private readonly IContactMessageService _contactMessageService;
    private readonly IWebsiteService _websiteService;
    private readonly IHumanVerificationService _verification;
    private readonly IMemoryCache _cache;

    public ContactController(
        IContactMessageService contactMessageService,
        IWebsiteService websiteService,
        IHumanVerificationService verification,
        IMemoryCache cache)
    {
        _contactMessageService = contactMessageService;
        _websiteService = websiteService;
        _verification = verification;
        _cache = cache;
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] ContactMessageRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        // Honeypot: real visitors never see or fill this field. Bots that autofill every input
        // trip it — pretend success so the bot doesn't learn to skip the field next time.
        if (!string.IsNullOrEmpty(request.Website))
            return Ok(new { message = "Your message has been sent." });

        if (string.IsNullOrWhiteSpace(request.SenderName) ||
            string.IsNullOrWhiteSpace(request.EmailAddress) ||
            string.IsNullOrWhiteSpace(request.MessageBody))
            return BadRequest(new { message = "Name, email and message are required." });

        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var throttleKey = $"contact-throttle:{website.WebsiteID}:{remoteIp}";
        if (!string.IsNullOrEmpty(remoteIp) && _cache.TryGetValue(throttleKey, out _))
            return StatusCode(StatusCodes.Status429TooManyRequests, new { message = "Please wait a moment before sending another message." });

        var setting = await _websiteService.GetCaptchaSettingAsync(website.WebsiteID, ct);
        var resolution = _verification.ResolveForWebsite(setting);

        bool captchaOk;
        if (resolution.UseTurnstile)
        {
            var result = await _verification.VerifyTurnstileAsync(request.CaptchaToken, remoteIp, resolution.TurnstileSecretKey!, ct);
            captchaOk = result == true;
        }
        else
        {
            captchaOk = _verification.ValidateMath(request.CaptchaToken, request.CaptchaAnswer);
        }

        if (!captchaOk)
            return BadRequest(new { message = "Captcha verification failed. Please try again." });

        if (!string.IsNullOrEmpty(remoteIp))
            _cache.Set(throttleKey, true, ThrottleWindow);

        await _contactMessageService.CreateAsync(new ContactUsMessage
        {
            WebsiteID = website.WebsiteID,
            SenderName = request.SenderName.Trim(),
            EmailAddress = request.EmailAddress.Trim(),
            CellphoneNumber = request.CellphoneNumber?.Trim() ?? string.Empty,
            MessageSubject = request.MessageSubject?.Trim() ?? string.Empty,
            MessageBody = request.MessageBody.Trim(),
            SenderIPAddress = remoteIp,
        }, ct);

        return Ok(new { message = "Your message has been sent." });
    }
}
