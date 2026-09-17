using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Public author pages (bio, online résumé and timeline) for the website front-end. The caller
/// website is resolved from the <c>X-Website-Key</c> header; an optional <c>lang</c> query selects
/// translated text. An author's posts are listed through <c>GET /api/posts?author={slug}</c>.
/// </summary>
public class AuthorsController : BaseController
{
    private readonly IAuthorProfileService _authors;
    private readonly IWebsiteService _websiteService;

    public AuthorsController(IAuthorProfileService authors, IWebsiteService websiteService)
    {
        _authors = authors;
        _websiteService = websiteService;
    }

    /// <summary>An author's public page by slug. 404 when unknown, inactive or the résumé page is turned off.</summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, [FromQuery] string? lang = null, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var author = await _authors.GetPublicAsync(website.WebsiteID, slug, lang, ct);
        return author is null ? NotFound() : Ok(author);
    }
}
