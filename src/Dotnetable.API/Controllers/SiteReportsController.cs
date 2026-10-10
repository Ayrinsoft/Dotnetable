using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Visitor reports about bugs and content on the public site. The caller website comes from
/// <c>X-Website-Key</c>. A signed-in customer token is optional and only stamps <c>WebsiteClientID</c>.
/// </summary>
[EnableRateLimiting(RateLimiting.PublicWritePolicy)]
public class SiteReportsController : BaseController
{
    private static readonly TimeSpan ThrottleWindow = TimeSpan.FromSeconds(20);

    private readonly ISiteReportService _reports;
    private readonly IWebsiteService _websiteService;
    private readonly IHumanVerificationService _verification;
    private readonly IMemoryCache _cache;

    public SiteReportsController(
        ISiteReportService reports,
        IWebsiteService websiteService,
        IHumanVerificationService verification,
        IMemoryCache cache)
    {
        _reports = reports;
        _websiteService = websiteService;
        _verification = verification;
        _cache = cache;
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SiteReportRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        if (!string.IsNullOrEmpty(request.Website))
            return Ok(new { message = "Your report has been sent." });

        if (string.IsNullOrWhiteSpace(request.ReporterName) ||
            string.IsNullOrWhiteSpace(request.EmailAddress) ||
            string.IsNullOrWhiteSpace(request.Subject) ||
            string.IsNullOrWhiteSpace(request.MessageBody))
            return BadRequest(new { message = "Name, email, subject and message are required." });

        if (!Enum.IsDefined(typeof(SiteReportKind), request.Kind))
            return BadRequest(new { message = "Report type is not recognized." });

        var pageUrl = string.IsNullOrWhiteSpace(request.PageUrl) ? null : request.PageUrl.Trim();
        if (!IsSafePageUrl(pageUrl))
            return BadRequest(new { message = "Page URL must be a site path or an http(s) address." });

        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var throttleKey = $"site-report-throttle:{website.WebsiteID}:{remoteIp}";
        if (!string.IsNullOrEmpty(remoteIp) && _cache.TryGetValue(throttleKey, out _))
            return StatusCode(StatusCodes.Status429TooManyRequests, new { message = "Please wait a moment before sending another report." });

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

        int? clientId = null;
        if (User.HasClaim(ClientClaims.IsClient, "true") &&
            int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var parsed))
            clientId = parsed;

        await _reports.CreateAsync(new SiteReport
        {
            WebsiteID = website.WebsiteID,
            WebsiteClientID = clientId,
            ReporterName = request.ReporterName.Trim(),
            EmailAddress = request.EmailAddress.Trim(),
            Kind = request.Kind,
            Subject = request.Subject.Trim(),
            MessageBody = request.MessageBody.Trim(),
            PageUrl = pageUrl,
            SenderIPAddress = remoteIp.Length > 45 ? remoteIp[..45] : remoteIp,
        }, ct);

        return Ok(new { message = "Your report has been sent." });
    }

    private static bool IsSafePageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        if (url.Length > 500) return false;
        if (url.StartsWith('/')) return !url.StartsWith("//") && !url.Contains('\\');
        return Uri.TryCreate(url, UriKind.Absolute, out var absolute)
            && (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps);
    }
}
