using System.Net.Mail;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Hosting;
using Dotnetable.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.API.Controllers;

/// <summary>Public newsletter signup for the website front-end (resolved from the <c>X-Website-Key</c> header).</summary>
[EnableRateLimiting(RateLimiting.PublicWritePolicy)]
public class SubscribeController : BaseController
{
    private readonly IWebsiteService _websiteService;
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public SubscribeController(IWebsiteService websiteService, IDbContextFactory<AppDbContext> contextFactory)
    {
        _websiteService = websiteService;
        _contextFactory = contextFactory;
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubscribeRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        if (!string.IsNullOrEmpty(request.Website))
            return Ok(new { message = "Thanks for subscribing." });

        if (string.IsNullOrWhiteSpace(request.Email) || !MailAddress.TryCreate(request.Email.Trim(), out var parsed))
            return BadRequest(new { message = "A valid email address is required." });

        var email = parsed.Address;
        if (email.Length > 64) email = email[..64];

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var exists = await context.EmailSubscribes.AsNoTracking()
            .AnyAsync(s => s.WebsiteID == website.WebsiteID && s.Email == email, ct);
        if (!exists)
        {
            context.EmailSubscribes.Add(new EmailSubscribe
            {
                WebsiteID = website.WebsiteID,
                Email = email,
                LogTime = DateTime.UtcNow,
                Active = true,
                Approved = false,
            });
            await context.SaveChangesAsync(ct);
        }

        return Ok(new { message = "Thanks for subscribing." });
    }
}
