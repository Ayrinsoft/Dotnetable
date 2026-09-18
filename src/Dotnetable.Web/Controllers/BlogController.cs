using Dotnetable.Application.DTOs;
using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

/// <summary>Public blog / content pages, backed by the API's post endpoints.</summary>
public class BlogController : Controller
{
    private const int PageSize = 9;

    private readonly ApiClient _api;

    public BlogController(ApiClient api) => _api = api;

    /// <summary>View model for a blog listing: the current page of posts plus paging state.</summary>
    public sealed record BlogListView(
        IReadOnlyList<PostSummaryDto> Posts,
        int Page,
        int TotalCount,
        int PageSize,
        string? Category,
        string? Tag,
        string? Heading,
        string? Summary = null)
    {
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public async Task<IActionResult> Index(int page = 1, string? category = null, string? tag = null, CancellationToken ct = default)
    {
        var lang = CurrentLang();
        var result = await _api.GetPostsAsync(category: category, tag: tag, page: page, pageSize: PageSize, lang: lang, ct: ct);

        // The category itself carries the heading and the summary shown above the list — reading them
        // from the posts would leave an empty (or filtered-out) page with no heading at all.
        var categoryDto = string.IsNullOrWhiteSpace(category) ? null : await _api.GetCategoryAsync(category, lang, ct);
        var heading = categoryDto?.Name
            ?? (!string.IsNullOrWhiteSpace(tag)
                ? result.Items.SelectMany(p => p.Tags).FirstOrDefault(t => t.Slug == tag)?.Name
                : null);

        return View("Index", new BlogListView(result.Items, page, result.TotalCount, PageSize, category, tag, heading, categoryDto?.Summary));
    }

    public Task<IActionResult> Category(string slug, int page = 1, CancellationToken ct = default) =>
        Index(page, category: slug, tag: null, ct);

    public Task<IActionResult> Tag(string slug, int page = 1, CancellationToken ct = default) =>
        Index(page, category: null, tag: slug, ct);

    public async Task<IActionResult> Post(string slug, CancellationToken ct = default)
    {
        var post = await _api.GetPostAsync(slug, CurrentLang(), ct);
        return post is null ? NotFound() : View(post);
    }

    /// <summary>Current UI language from the <c>lang</c> cookie, or null for the site default.</summary>
    private string? CurrentLang()
    {
        var lang = Request.Cookies["lang"];
        return string.IsNullOrWhiteSpace(lang) ? null : lang;
    }
}
