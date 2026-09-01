using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Public posts for the website front-end. The caller website is resolved from the
/// <c>X-Website-Key</c> header; an optional <c>lang</c> query selects translated content.
/// </summary>
public class PostsController : BaseController
{
    private readonly IPostService _postService;
    private readonly IWebsiteService _websiteService;

    public PostsController(IPostService postService, IWebsiteService websiteService)
    {
        _postService = postService;
        _websiteService = websiteService;
    }

    /// <summary>Published posts, newest first, optionally filtered by post type / category / tag slug.</summary>
    [HttpGet]
    public async Task<IActionResult> GetPublished(
        [FromQuery] string? type = null, [FromQuery] string? category = null, [FromQuery] string? tag = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 12, [FromQuery] string? lang = null,
        CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var result = await _postService.GetPublishedAsync(website.WebsiteID, type, category, tag, page, GridQuery.ClampPageSize(pageSize), lang, ct);
        return Ok(result);
    }

    /// <summary>Featured published posts (newest first).</summary>
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured([FromQuery] int take = 4, [FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var posts = await _postService.GetFeaturedAsync(website.WebsiteID, GridQuery.ClampPageSize(take, 4), lang, ct);
        return Ok(posts);
    }

    /// <summary>A single published post by slug (base or translated). Increments the view count.</summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, [FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var post = await _postService.GetBySlugAsync(website.WebsiteID, slug, lang, ct);
        return post is null ? NotFound() : Ok(post);
    }
}
