using Dotnetable.Application.DTOs;
using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

/// <summary>Public "about the author" page (<c>/author/{slug}</c>): bio, online résumé, timeline and the author's posts.</summary>
public class AuthorController : Controller
{
    private const int PageSize = 6;

    private readonly ApiClient _api;

    public AuthorController(ApiClient api) => _api = api;

    /// <summary>View model for the author page: the author plus one page of their posts.</summary>
    public sealed record AuthorPageView(AuthorPageDto Author, IReadOnlyList<PostSummaryDto> Posts, int Page, int TotalCount)
    {
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public async Task<IActionResult> Profile(string slug, int page = 1, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return NotFound();

        var lang = CurrentLang();
        var author = await _api.GetAuthorAsync(slug, lang, ct);
        if (author is null) return NotFound();

        var posts = await _api.GetPostsAsync(author: author.Slug, page: page < 1 ? 1 : page, pageSize: PageSize, lang: lang, ct: ct);
        return View("Profile", new AuthorPageView(author, posts.Items, page < 1 ? 1 : page, posts.TotalCount));
    }

    /// <summary>Current UI language from the <c>lang</c> cookie, or null for the site default.</summary>
    private string? CurrentLang()
    {
        var lang = Request.Cookies["lang"];
        return string.IsNullOrWhiteSpace(lang) ? null : lang;
    }
}
