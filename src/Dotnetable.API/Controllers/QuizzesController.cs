using Dotnetable.Application.Authorization;
using Dotnetable.Domain.Entities;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Dotnetable.Hosting;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Public timed tests. The caller website comes from <c>X-Website-Key</c>.
/// Building tests and sending private answers is done in Admin.
/// </summary>
[EnableRateLimiting(RateLimiting.PublicWritePolicy)]
public class QuizzesController : BaseController
{
    private readonly IQuizService _quizzes;
    private readonly IWebsiteService _websites;

    public QuizzesController(IQuizService quizzes, IWebsiteService websites)
    {
        _quizzes = quizzes;
        _websites = websites;
    }

    [HttpGet("categories")]
    public async Task<IActionResult> Categories([FromQuery] bool postsOnly = false, [FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });
        return Ok(await _quizzes.GetPublicCategoriesAsync(website.WebsiteID, lang, postsOnly, ct));
    }

    [HttpGet("categories/{id:int}")]
    public async Task<IActionResult> Category(int id, [FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });
        var category = await _quizzes.GetPublicCategoryAsync(website.WebsiteID, id, lang, ct);
        return category is null ? NoContent() : Ok(category);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, [FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });
        var quiz = await _quizzes.GetPublicQuizAsync(website.WebsiteID, id, lang, ct);
        return quiz is null ? NoContent() : Ok(quiz);
    }

    [HttpPost("{id:int}/start")]
    public async Task<IActionResult> Start(int id, [FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });
        var result = await _quizzes.StartAttemptAsync(website.WebsiteID, id, ClientId(), lang, ct);
        return result.Ok ? Ok(result.Attempt) : BadRequest(new { message = result.Message });
    }

    [HttpGet("attempts/{token}")]
    public async Task<IActionResult> Attempt(string token, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });
        if (!Guid.TryParse(token, out var parsed)) return NoContent();
        var attempt = await _quizzes.GetAttemptAsync(website.WebsiteID, parsed, ct);
        return attempt is null ? NoContent() : Ok(attempt);
    }

    [HttpPost("attempts/{token}/submit")]
    public async Task<IActionResult> Submit(string token, [FromBody] QuizSubmitRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });
        if (!Guid.TryParse(token, out var parsed)) return BadRequest(new { message = "This sitting was not found." });
        var result = await _quizzes.SubmitAttemptAsync(website.WebsiteID, parsed, request, ct);
        return result.Ok ? Ok(result.Attempt) : BadRequest(new { message = result.Message });
    }

    [HttpPost("attempts/{token}/review")]
    public async Task<IActionResult> Review(string token, [FromBody] QuizPrivateReviewRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });
        if (!Guid.TryParse(token, out var parsed)) return BadRequest(new { message = "This sitting was not found." });
        if (request.PayOnline && !IsOwnSiteUrl(website, request.ReturnUrl))
            return BadRequest(new { message = "The return URL must belong to this website." });

        var result = await _quizzes.RequestPrivateReviewAsync(website.WebsiteID, parsed, ClientId(), request, ct);
        return result.Ok ? Ok(result) : BadRequest(result);
    }

    [HttpGet("attempts/{token}/payment")]
    [HttpPost("attempts/{token}/payment")]
    public async Task<IActionResult> Payment(string token, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });
        if (!Guid.TryParse(token, out var parsed)) return BadRequest(new { message = "This sitting was not found." });

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in Request.Query)
            values[key] = value.ToString();
        if (Request.HasFormContentType)
        {
            foreach (var (key, value) in Request.Form)
                values[key] = value.ToString();
        }

        var callback = new GatewayCallback(
            Authority: Find(values, "Authority", "authority", "trackId", "token", "trans_id", "id", "session_id", "refId", "refid"),
            Status: Find(values, "Status", "status", "result"),
            Values: values);
        var result = await _quizzes.CompletePrivatePaymentAsync(website.WebsiteID, parsed, callback, ct);
        return result.Ok ? Ok(result) : BadRequest(result);
    }

    private int? ClientId() =>
        int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var id) ? id : null;

    private static string? Find(IReadOnlyDictionary<string, string> values, params string[] names)
    {
        foreach (var name in names)
        {
            if (values.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value))
                return value;
        }
        return null;
    }

    private static bool IsOwnSiteUrl(Website website, string? url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return false;
        var configured = website.WebsiteAddress?.Trim();
        if (string.IsNullOrWhiteSpace(configured)) return false;
        var host = Uri.TryCreate(configured, UriKind.Absolute, out var configuredUri)
            ? configuredUri.Host
            : configured;
        return string.Equals(uri.Host, host, StringComparison.OrdinalIgnoreCase);
    }
}
