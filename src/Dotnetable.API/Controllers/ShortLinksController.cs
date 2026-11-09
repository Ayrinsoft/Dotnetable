using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// The public site calls this with its website key and the code from <c>/l/{code}</c>.
/// The answer is the stored address. Nothing here is a secret.
/// </summary>
public class ShortLinksController : BaseController
{
    private readonly IWebsiteService _websites;
    private readonly IShortLinkService _links;

    public ShortLinksController(IWebsiteService websites, IShortLinkService links)
    {
        _websites = websites;
        _links = links;
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> Get(string code, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return NotFound();
        var url = await _links.ResolveAsync(website.WebsiteID, code, ct);
        return string.IsNullOrWhiteSpace(url) ? NotFound() : Ok(new { url });
    }
}
