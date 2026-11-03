using System.Text;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Seo;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Public IndexNow ownership key for the storefront. Bing fetches <c>https://{host}/{key}.txt</c>
/// on the public site; the Web app reads this endpoint and serves that file. The key is not a
/// credential — the protocol requires it to be publicly readable.
/// </summary>
public class IndexNowController : BaseController
{
    private readonly IWebsiteService _websites;
    private readonly IWebsiteSettingService _settings;

    public IndexNowController(IWebsiteService websites, IWebsiteSettingService settings)
    {
        _websites = websites;
        _settings = settings;
    }

    [HttpGet("key")]
    public async Task<IActionResult> Key(CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return NotFound();

        var seo = await _settings.GetSeoSettingAsync(website.WebsiteID, ct);
        var key = seo?.IndexNowKey?.Trim();
        if (seo is not { IndexNowEnabled: true } || key is null || !IndexNowKeyRules.IsValid(key))
            return NotFound();

        return Content(key, "text/plain", Encoding.UTF8);
    }
}
