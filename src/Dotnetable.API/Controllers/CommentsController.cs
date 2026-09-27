using System.Net;
using Dotnetable.API.Auth;
using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Visitor comments on blog posts and CMS pages. Reads return approved comments only; writes are
/// open to guests (name + captcha) and to signed-in customers (identity from the token), and every
/// new comment waits for moderation in the admin panel before it appears.
/// </summary>
[Route("api")]
[AllowAnonymous]
public class CommentsController : BaseController
{
    // Stops a script from posting a burst; the captcha is the real bot gate.
    private static readonly TimeSpan ThrottleWindow = TimeSpan.FromSeconds(15);

    // One accepted comment per visitor per post or page. The storefront shows that comment back
    // until the window ends; clearing the site cookie does not reset it when X-Client-Ip is sent.
    private static readonly TimeSpan CommentLockWindow = TimeSpan.FromHours(24);

    private readonly IContentCommentService _comments;
    private readonly IWebsiteService _websiteService;
    private readonly CaptchaGuard _captcha;
    private readonly IMemoryCache _cache;

    public CommentsController(IContentCommentService comments, IWebsiteService websiteService, CaptchaGuard captcha, IMemoryCache cache)
    {
        _comments = comments;
        _websiteService = websiteService;
        _captcha = captcha;
        _cache = cache;
    }

    /// <summary>Approved comments of a post: top-level comments newest first, replies nested.</summary>
    [HttpGet("posts/{postId:int}/comments")]
    public Task<IActionResult> GetForPost(int postId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        GetAsync(CommentTarget.Post, postId, page, pageSize, ct);

    /// <summary>Approved comments of a CMS page.</summary>
    [HttpGet("pages/{pageId:int}/comments")]
    public Task<IActionResult> GetForPage(int pageId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default) =>
        GetAsync(CommentTarget.Page, pageId, page, pageSize, ct);

    [HttpPost("posts/{postId:int}/comments")]
    [EnableRateLimiting(RateLimiting.PublicWritePolicy)]
    public Task<IActionResult> SubmitForPost(int postId, [FromBody] CommentSubmitRequest request, CancellationToken ct = default) =>
        SubmitAsync(CommentTarget.Post, postId, request, ct);

    [HttpPost("pages/{pageId:int}/comments")]
    [EnableRateLimiting(RateLimiting.PublicWritePolicy)]
    public Task<IActionResult> SubmitForPage(int pageId, [FromBody] CommentSubmitRequest request, CancellationToken ct = default) =>
        SubmitAsync(CommentTarget.Page, pageId, request, ct);

    private async Task<IActionResult> GetAsync(CommentTarget target, int targetId, int page, int pageSize, CancellationToken ct)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var query = new GridQuery { PageIndex = page, PageSize = GridQuery.ClampPageSize(pageSize) };
        return Ok(await _comments.GetApprovedAsync(website.WebsiteID, target, targetId, query, ct));
    }

    private async Task<IActionResult> SubmitAsync(CommentTarget target, int targetId, CommentSubmitRequest request, CancellationToken ct)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        const string pendingMessage = "Thanks! Your comment will appear after it has been reviewed.";
        const string lockedMessage = "You can comment here again after 24 hours. The comment you sent is waiting for review.";

        // Honeypot tripped: pretend success so the bot learns nothing. Do not start the 24-hour lock.
        if (!string.IsNullOrEmpty(request.Website))
            return Ok(new { message = pendingMessage });

        int? clientId = null;
        if (User.HasClaim(ClientClaims.IsClient, "true") || User.FindFirst(ClientClaims.ClientId) is not null)
        {
            if (!int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var id))
                return Unauthorized();
            if (!ApiAuthorization.Allows(User, RoleKeys.ClientReview))
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Your account is not allowed to comment." });
            clientId = id;
        }

        // The storefront's server is the TCP peer, so the 24-hour lock uses only the headers that
        // identify the person. Falling back to the connection address would lock every visitor of that site.
        var clientIp = ClientIp(Request);
        var visitorId = VisitorId(Request);
        var identity = clientId?.ToString() ?? visitorId ?? clientIp ?? HttpContext.Connection.RemoteIpAddress?.ToString();
        var dayKeys = LockKeys(website.WebsiteID, target, targetId, visitorId, clientIp, clientId).ToList();
        if (dayKeys.Any(key => _cache.TryGetValue(key, out _)))
            return StatusCode(StatusCodes.Status429TooManyRequests, new { message = lockedMessage });

        var throttleKey = $"comment-throttle:{website.WebsiteID}:{target}:{targetId}:{identity}";
        if (!string.IsNullOrEmpty(identity) && _cache.TryGetValue(throttleKey, out _))
            return StatusCode(StatusCodes.Status429TooManyRequests, new { message = "Please wait a moment before posting another comment." });

        // A signed-in customer already proved themselves at sign-in; guests must pass the captcha.
        if (clientId is null && !await _captcha.VerifyAsync(website.WebsiteID, request.CaptchaToken, request.CaptchaAnswer, clientIp, ct))
            return BadRequest(new { message = "Captcha verification failed. Please try again." });

        var userAgent = Request.Headers["X-Visitor-User-Agent"].ToString();
        if (string.IsNullOrWhiteSpace(userAgent))
            userAgent = Request.Headers.UserAgent.ToString();

        var result = await _comments.SubmitAsync(new CommentSubmission(
            website.WebsiteID, target, targetId, request.ParentCommentId, clientId,
            request.AuthorName, request.AuthorEmail, request.Body,
            clientIp, userAgent), ct);

        if (!result.Success)
            return result.NotFound
                ? NotFound(new { message = result.Error })
                : BadRequest(new { message = result.Error });

        if (!string.IsNullOrEmpty(identity))
            _cache.Set(throttleKey, true, ThrottleWindow);
        foreach (var key in dayKeys)
            _cache.Set(key, true, CommentLockWindow);

        return Ok(new { commentId = result.CommentId, status = "pending", message = pendingMessage });
    }

    private static IEnumerable<string> LockKeys(int websiteId, CommentTarget target, int targetId, string? visitorId, string? clientIp, int? clientId)
    {
        var prefix = $"comment-day:{websiteId}:{(byte)target}:{targetId}:";
        if (clientId is int id)
            yield return prefix + "c:" + id;
        if (!string.IsNullOrEmpty(visitorId))
            yield return prefix + "v:" + visitorId;
        if (!string.IsNullOrEmpty(clientIp))
            yield return prefix + "ip:" + clientIp;
    }

    /// <summary>Cookie id minted by the storefront. Ignored unless it looks like that id.</summary>
    private static string? VisitorId(HttpRequest request)
    {
        var id = request.Headers["X-Visitor-Id"].ToString().Trim();
        if (id.Length is >= 8 and <= 64 && id.All(ch => char.IsAsciiLetterOrDigit(ch) || ch is '_' or '-'))
            return id;
        return null;
    }

    /// <summary>Visitor address as seen by the storefront. The API's own connection address is not used here.</summary>
    private static string? ClientIp(HttpRequest request)
    {
        var ip = request.Headers["X-Client-Ip"].ToString().Trim();
        return IPAddress.TryParse(ip, out var parsed) ? parsed.ToString() : null;
    }
}
