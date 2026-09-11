using System.Diagnostics;
using Dotnetable.Application.DTOs;
using Dotnetable.Web.Models;
using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApiClient _api;
    private readonly WebLocalizationService _localization;
    private readonly ContentShortcodeProcessor _shortcodes;

    public HomeController(ApiClient api, WebLocalizationService localization, ContentShortcodeProcessor shortcodes)
    {
        _api = api;
        _localization = localization;
        _shortcodes = shortcodes;
    }

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var lang = Request.Cookies["lang"];
        var langOrNull = string.IsNullOrWhiteSpace(lang) ? null : lang;

        // The homepage body is a regular CMS page (slug "home", editable in Admin → Content →
        // Pages, shortcodes supported). The generic hero/features markup in the view is only the
        // fallback until that page is created.
        var homePage = await _api.GetPageAsync("home", langOrNull, ct);
        if (homePage is not null)
            ViewData["HomeContentHtml"] = await _shortcodes.ExpandAsync(homePage.Content, ct);

        var latest = await _api.GetPostsAsync(page: 1, pageSize: 3, lang: langOrNull, ct: ct);
        ViewData["PriceLists"] = await _api.GetPriceListsAsync(ct);
        return View(latest.Items);
    }

    /// <summary>About Us is a regular CMS page (slug "about-us"), editable from Admin → Content → Pages.</summary>
    public IActionResult About() => RedirectToAction("View", "Page", new { slug = "about-us" });

    /// <summary>Services is a regular CMS page (slug "services"), editable from Admin → Content → Pages.</summary>
    public IActionResult Services() => RedirectToAction("View", "Page", new { slug = "services" });

    /// <summary>Contact Us combines the CMS page content (slug "contact-us") with a working contact form.</summary>
    [HttpGet]
    public async Task<IActionResult> Contact(CancellationToken ct = default)
    {
        var lang = Request.Cookies["lang"];
        var page = await _api.GetPageAsync("contact-us", string.IsNullOrWhiteSpace(lang) ? null : lang, ct);
        var site = await _api.GetSiteInfoAsync(lang, ct);
        var model = new ContactPageViewModel
        {
            PageTitle = page?.Title ?? "Contact",
            ContentHtml = await _shortcodes.ExpandAsync(page?.Content, ct),
            ContactInfos = site?.ContactInfos ?? new List<Application.DTOs.ContactInfoDto>(),
        };
        return View(model);
    }

    /// <summary>Fresh captcha challenge for the contact form's JS-driven submit (no page reload).</summary>
    [HttpGet]
    public async Task<IActionResult> ContactCaptcha(CancellationToken ct = default)
    {
        var challenge = await _api.GetCaptchaChallengeAsync(ct);
        if (challenge is null) return StatusCode(StatusCodes.Status503ServiceUnavailable);
        return Json(challenge);
    }

    /// <summary>AJAX submit for the contact form: takes the same request shape the public API
    /// expects (including the captcha fields) and forwards it, so the page never reloads.</summary>
    [HttpPost]
    public async Task<IActionResult> ContactSubmit([FromBody] ContactMessageRequest request, CancellationToken ct = default)
    {
        var result = await _api.SubmitContactMessageAsync(request, ct);
        return StatusCode((int)result.Status, new { message = result.Message });
    }

    /// <summary>
    /// The production exception page. <c>Program.cs</c> has always pointed <c>UseExceptionHandler</c>
    /// here, but neither this action nor its view existed — so in production the error handler itself
    /// 404d and every unhandled exception rendered a blank page.
    /// </summary>
    [HttpGet, HttpPost]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Error()
    {
        Response.StatusCode = StatusCodes.Status500InternalServerError;
        return View(new ErrorViewModel
        {
            // Correlates the page the visitor is looking at with the line in the log. Never show the
            // exception itself: the message can carry connection strings and internal paths.
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = StatusCodes.Status500InternalServerError,
        });
    }

    /// <summary>Themed page for status codes that never reached an action — 404 above all.</summary>
    [HttpGet, HttpPost]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult StatusCode(int? code)
    {
        var status = code ?? StatusCodes.Status404NotFound;
        Response.StatusCode = status;
        return View("Error", new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = status,
        });
    }
}
