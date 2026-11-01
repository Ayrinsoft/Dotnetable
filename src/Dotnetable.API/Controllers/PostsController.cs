using System.Net;
using Dotnetable.Application.Content;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Public posts for the website front-end. The caller website is resolved from the
/// <c>X-Website-Key</c> header; an optional <c>lang</c> query selects translated content.
/// </summary>
public class PostsController : BaseController
{
    /// <summary>Same person reloading a post inside this window is one view, not one per request.</summary>
    private static readonly TimeSpan ViewWindow = TimeSpan.FromHours(6);

    private readonly IPostService _postService;
    private readonly IContentVersionService _contentVersions;
    private readonly IWebsiteService _websiteService;
    private readonly IMemoryCache _cache;

    public PostsController(
        IPostService postService, IContentVersionService contentVersions, IWebsiteService websiteService, IMemoryCache cache)
    {
        _postService = postService;
        _contentVersions = contentVersions;
        _websiteService = websiteService;
        _cache = cache;
    }

    /// <summary>Published posts, newest first, optionally filtered by post type / category / tag / author slug.</summary>
    [HttpGet]
    public async Task<IActionResult> GetPublished(
        [FromQuery] string? type = null, [FromQuery] string? category = null, [FromQuery] string? tag = null, [FromQuery] string? author = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 12, [FromQuery] string? lang = null,
        [FromQuery] string? version = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        try
        {
            var result = await _postService.GetPublishedAsync(
                website.WebsiteID, type, category, tag, page, GridQuery.ClampPageSize(pageSize), lang, author, version, ct);
            return Ok(result);
        }
        catch (ContentEditionException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Published content release labels for this website (help center / policy versions).</summary>
    [HttpGet("content-versions")]
    public async Task<IActionResult> ContentVersions(CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var rows = await _contentVersions.GetPublishedAsync(website.WebsiteID, ct);
        return Ok(rows.Select(v => new ContentVersionDto
        {
            ContentVersionID = v.ContentVersionID,
            Name = v.Name,
            SortOrder = v.SortOrder,
            IsLatest = v.IsLatest,
            IsPublished = v.IsPublished,
        }));
    }

    /// <summary>
    /// Published post by stable key. <c>version</c> is a content release such as <c>1.2</c>.
    /// Omit it to use the latest published content release, or the unversioned row when the site has none.
    /// </summary>
    [HttpGet("by-key/{stableKey}")]
    public async Task<IActionResult> GetByStableKey(
        string stableKey, [FromQuery] string? version = null, [FromQuery] string? type = null,
        [FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        try
        {
            var post = await _postService.GetByStableKeyAsync(website.WebsiteID, stableKey, version, type, lang, ct);
            return post is null ? NotFound() : Ok(post);
        }
        catch (ContentEditionException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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

    /// <summary>A single published post by slug (base or translated). Reading the post does not count a view.</summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, [FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var post = await _postService.GetBySlugAsync(website.WebsiteID, slug, lang, ct);
        return post is null ? NotFound() : Ok(post);
    }

    /// <summary>
    /// Counts one human view. The storefront calls this only after its own bot check, and sends
    /// <c>X-Visitor-Id</c> (a cookie on the storefront) so a reload inside <see cref="ViewWindow"/> is ignored.
    /// </summary>
    [HttpPost("{postId:int}/view")]
    public async Task<IActionResult> RecordView(int postId, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var visitor = VisitorIdentity.From(Request);
        if (!string.IsNullOrEmpty(visitor))
        {
            var key = $"post-view:{website.WebsiteID}:{postId}:{visitor}";
            if (_cache.TryGetValue(key, out _))
                return NoContent();

            var counted = await _postService.RecordViewAsync(website.WebsiteID, postId, ct);
            if (!counted) return NotFound();
            _cache.Set(key, true, ViewWindow);
            return NoContent();
        }

        var recorded = await _postService.RecordViewAsync(website.WebsiteID, postId, ct);
        return recorded ? NoContent() : NotFound();
    }

    /// <summary>Storefront-supplied visitor id, else the forwarded client IP. Never the API connection itself.</summary>
    private static class VisitorIdentity
    {
        public static string? From(HttpRequest request)
        {
            var id = request.Headers["X-Visitor-Id"].ToString().Trim();
            if (id.Length is >= 8 and <= 64 && id.All(ch => char.IsAsciiLetterOrDigit(ch) || ch is '_' or '-'))
                return "v:" + id;

            var ip = request.Headers["X-Client-Ip"].ToString().Trim();
            if (IPAddress.TryParse(ip, out var parsed))
                return "ip:" + parsed;
            return null;
        }
    }
}
