using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

public class PostsController : BaseController
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PostType? type, [FromQuery] int languageId = 0, CancellationToken ct = default)
    {
        var posts = await _postService.GetByWebsiteAsync(CurrentWebsiteId, type, PostStatus.Published, ct);
        return Ok(posts);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, [FromQuery] int languageId, CancellationToken ct = default)
    {
        var post = await _postService.GetBySlugAsync(CurrentWebsiteId, slug, languageId, ct);
        return post is null ? NotFound() : Ok(post);
    }
}
