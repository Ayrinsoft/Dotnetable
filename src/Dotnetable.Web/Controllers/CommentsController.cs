using Dotnetable.Application.DTOs;
using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

/// <summary>JSON endpoints behind the comment thread on blog posts and CMS pages
/// (<c>Views/Shared/_Comments.cshtml</c> + <c>js/comments.js</c>). Both forward to the public API.</summary>
public class CommentsController : Controller
{
    private readonly ApiClient _api;

    public CommentsController(ApiClient api) => _api = api;

    /// <summary>A further page of approved comments ("load more").</summary>
    [HttpGet]
    public async Task<IActionResult> List(CommentTarget target, int targetId, int page = 1, CancellationToken ct = default) =>
        Json(await _api.GetCommentsAsync(target, targetId, page, ct: ct));

    [HttpPost]
    public async Task<IActionResult> Submit(CommentTarget target, int targetId, [FromBody] CommentSubmitRequest request, CancellationToken ct = default)
    {
        var result = await _api.SubmitCommentAsync(target, targetId, request, ct);
        return StatusCode((int)result.Status, new { message = result.Message });
    }
}
