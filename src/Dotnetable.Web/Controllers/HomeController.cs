using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApiClient _api;
    private readonly WebLocalizationService _localization;

    public HomeController(ApiClient api, WebLocalizationService localization)
    {
        _api = api;
        _localization = localization;
    }

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var lang = Request.Cookies["lang"];
        var latest = await _api.GetPostsAsync(page: 1, pageSize: 3, lang: string.IsNullOrWhiteSpace(lang) ? null : lang, ct: ct);
        return View(latest.Items);
    }

    public IActionResult About() => View();

    public IActionResult Services() => View();

    public IActionResult Contact() => View();
}
