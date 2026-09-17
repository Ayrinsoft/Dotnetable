using Dotnetable.API.Auth;
using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Enums;
using Dotnetable.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Likes (favorites for products) and bookmarks of signed-in customers on products, posts and pages.
/// <c>{targetType}</c> is <c>product</c>, <c>post</c> or <c>page</c>. Reading the state is public (the
/// caller's own flags are filled in when a customer token is sent); changing it needs a customer token.
/// A like is counted once per customer. A product favorite is the wishlist: favoriting adds the
/// product to <c>/api/wishlist</c>, un-favoriting removes it.
/// </summary>
public class ReactionsController : BaseController
{
    private readonly IClientReactionService _reactions;
    private readonly IWebsiteService _websiteService;

    public ReactionsController(IClientReactionService reactions, IWebsiteService websiteService)
    {
        _reactions = reactions;
        _websiteService = websiteService;
    }

    /// <summary>Like counter plus the caller's own like/bookmark flags.</summary>
    [HttpGet("{targetType}/{targetId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetState(string targetType, int targetId, CancellationToken ct = default)
    {
        if (!ReactionTargets.TryParse(targetType, out var type)) return NotFound();
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var state = await _reactions.GetStateAsync(website.WebsiteID, type, targetId, OptionalClientId, ct);
        return state is null ? NotFound() : Ok(state);
    }

    [HttpPut("{targetType}/{targetId:int}/like")]
    [Authorize(Policy = RoleKeys.ClientProfile)]
    [EnableRateLimiting(RateLimiting.PublicWritePolicy)]
    public Task<IActionResult> Like(string targetType, int targetId, CancellationToken ct = default) =>
        SetAsync(targetType, targetId, ReactionType.Like, true, ct);

    [HttpDelete("{targetType}/{targetId:int}/like")]
    [Authorize(Policy = RoleKeys.ClientProfile)]
    [EnableRateLimiting(RateLimiting.PublicWritePolicy)]
    public Task<IActionResult> Unlike(string targetType, int targetId, CancellationToken ct = default) =>
        SetAsync(targetType, targetId, ReactionType.Like, false, ct);

    [HttpPut("{targetType}/{targetId:int}/bookmark")]
    [Authorize(Policy = RoleKeys.ClientProfile)]
    [EnableRateLimiting(RateLimiting.PublicWritePolicy)]
    public Task<IActionResult> Bookmark(string targetType, int targetId, CancellationToken ct = default) =>
        SetAsync(targetType, targetId, ReactionType.Bookmark, true, ct);

    [HttpDelete("{targetType}/{targetId:int}/bookmark")]
    [Authorize(Policy = RoleKeys.ClientProfile)]
    [EnableRateLimiting(RateLimiting.PublicWritePolicy)]
    public Task<IActionResult> Unbookmark(string targetType, int targetId, CancellationToken ct = default) =>
        SetAsync(targetType, targetId, ReactionType.Bookmark, false, ct);

    /// <summary>The customer's bookmarks (<c>kind=bookmark</c>, default) or likes (<c>kind=like</c>),
    /// newest first, optionally of one <c>type</c>.</summary>
    [HttpGet("mine")]
    [Authorize(Policy = RoleKeys.ClientProfile)]
    public async Task<IActionResult> Mine(
        [FromQuery] string? kind = null, [FromQuery] string? type = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? lang = null,
        CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });
        if (OptionalClientId is not int clientId) return Unauthorized();

        var reaction = string.Equals(kind, "like", StringComparison.OrdinalIgnoreCase) ? ReactionType.Like : ReactionType.Bookmark;
        ReactionTargetType? targetType = null;
        if (!string.IsNullOrWhiteSpace(type))
        {
            if (!ReactionTargets.TryParse(type, out var parsed)) return BadRequest(new { message = "Unknown type." });
            targetType = parsed;
        }

        return Ok(await _reactions.GetListAsync(website.WebsiteID, clientId, reaction, targetType,
            page, GridQuery.ClampPageSize(pageSize), lang, ct));
    }

    private async Task<IActionResult> SetAsync(string targetType, int targetId, ReactionType reaction, bool on, CancellationToken ct)
    {
        if (!ReactionTargets.TryParse(targetType, out var type)) return NotFound();
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });
        if (OptionalClientId is not int clientId) return Unauthorized();

        var state = await _reactions.SetAsync(website.WebsiteID, clientId, type, targetId, reaction, on, ct);
        return state is null ? NotFound() : Ok(state);
    }

    /// <summary>The customer id from a client token, or null for an anonymous caller (or an admin token).</summary>
    private int? OptionalClientId =>
        User.Identity?.IsAuthenticated == true && int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var id)
            ? id
            : null;
}
