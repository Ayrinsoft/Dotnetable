using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Public path templates for this website. The storefront uses them to send a short
/// address to the page it already renders. Nothing here is a secret.
/// </summary>
public class SocialLinksController : BaseController
{
    private readonly IWebsiteService _websites;
    private readonly ISocialLinkSettings _links;

    public SocialLinksController(IWebsiteService websites, ISocialLinkSettings links)
    {
        _websites = websites;
        _links = links;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return NotFound();
        return Ok(await _links.GetAsync(website.WebsiteID, ct));
    }
}
