using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dotnetable.API.Controllers;

/// <summary>
/// Public digital visiting cards. The caller website comes from <c>X-Website-Key</c>.
/// The page itself is <c>/c/{slug}</c> on the storefront.
/// </summary>
[Route("api/visit-cards")]
public class VisitCardsController : BaseController
{
    private readonly IAuthorVisitCardService _cards;
    private readonly IWebsiteService _websiteService;

    public VisitCardsController(IAuthorVisitCardService cards, IWebsiteService websiteService)
    {
        _cards = cards;
        _websiteService = websiteService;
    }

    /// <summary>The published card for a short link. 404 when unknown, disabled or the member is inactive.</summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var card = await _cards.GetPublicAsync(website.WebsiteID, slug, ct);
        return card is null ? NotFound() : Ok(card);
    }

    /// <summary>vCard 3.0 file of the card owner.</summary>
    [HttpGet("{slug}/vcard")]
    public async Task<IActionResult> Vcard(string slug, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var vcf = await _cards.BuildPublicVcfAsync(website.WebsiteID, slug, ct);
        if (vcf is null) return NotFound();
        return File(System.Text.Encoding.UTF8.GetBytes(vcf), "text/vcard; charset=utf-8", $"{slug}.vcf");
    }

    /// <summary>A visitor leaves a name, a phone and/or email, and why they should be called.</summary>
    [HttpPost("{slug}/contacts")]
    [EnableRateLimiting(RateLimiting.PublicWritePolicy)]
    public async Task<IActionResult> AddContact(string slug, [FromBody] VisitCardContactRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        var code = await _cards.InsertContactAsync(website.WebsiteID, slug, request, ct);
        if (code is null) return Ok(new { ok = true });
        return BadRequest(new { code, message = ContactMessage(code) });
    }

    private static string ContactMessage(string code) => code switch
    {
        "name" => "Enter a name between 2 and 70 characters.",
        "contact" => "Enter a phone number or an email.",
        "mobile" => "The phone number is not valid.",
        "email" => "The email address is not valid.",
        "reason" => "Say why you should be contacted (up to 300 characters).",
        "disabled" => "This card is not taking new numbers.",
        "full" => "This phone book is full.",
        _ => "This card was not found.",
    };
}
