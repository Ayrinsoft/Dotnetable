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
        var latest = await _api.GetPostsAsync(page: 1, pageSize: 3, lang: string.IsNullOrWhiteSpace(lang) ? null : lang, ct: ct);
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
        var model = new ContactPageViewModel
        {
            PageTitle = page?.Title ?? "Contact",
            ContentHtml = await _shortcodes.ExpandAsync(page?.Content, ct),
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactPageViewModel model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _api.SubmitContactMessageAsync(new ContactMessageRequest
        {
            SenderName = model.Name,
            EmailAddress = model.Email,
            CellphoneNumber = model.Phone ?? string.Empty,
            MessageSubject = model.Subject ?? string.Empty,
            MessageBody = model.Message,
        }, ct);

        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Could not send your message. Please try again.");
            return View(model);
        }

        model.Submitted = true;
        model.Name = model.Email = model.Phone = model.Subject = model.Message = string.Empty;
        return View(model);
    }
}
