using System.Net;
using Dotnetable.Application.DTOs;
using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

/// <summary>JSON endpoint behind the like/favorite and bookmark buttons
/// (<c>Views/Shared/_ReactionBar.cshtml</c> + <c>js/reactions.js</c>). Forwards to the public API with
/// the customer's token attached by the bearer handler.</summary>
public class ReactionsController : Controller
{
    private readonly ApiClient _api;

    public ReactionsController(ApiClient api) => _api = api;

    [HttpPost]
    public async Task<IActionResult> Toggle(string targetType, int targetId, string kind, bool on, CancellationToken ct = default)
    {
        if (!ReactionTargets.TryParse(targetType, out _)) return NotFound();
        if (!Request.Cookies.ContainsKey(ClientAuth.TokenCookie))
            return Unauthorized(new { message = "Please sign in first." });

        var (status, state) = await _api.SetReactionAsync(targetType, targetId, kind, on, ct);
        return status switch
        {
            _ when state is not null => Json(state),
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => Unauthorized(new { message = "Please sign in first." }),
            HttpStatusCode.NotFound => NotFound(),
            _ => StatusCode((int)status, new { message = "Could not save. Please try again." }),
        };
    }
}
