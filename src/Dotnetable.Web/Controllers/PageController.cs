using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

/// <summary>Public CMS pages, backed by the API's page endpoints and rendered by slug.</summary>
public class PageController : Controller
{
    private readonly ApiClient _api;

    public PageController(ApiClient api) => _api = api;

    /// <summary>Renders a CMS page by its slug (e.g. /page/about-us).</summary>
    public async Task<IActionResult> View(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return NotFound();

        var lang = Request.Cookies["lang"];
        var page = await _api.GetPageAsync(slug, string.IsNullOrWhiteSpace(lang) ? null : lang, ct);
        return page is null ? NotFound() : base.View("View", page);
    }
}
