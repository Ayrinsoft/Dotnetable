using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>Public contact-form submission for the website front-end (resolved from the <c>X-Website-Key</c> header).</summary>
public class ContactController : BaseController
{
    private readonly IContactMessageService _contactMessageService;
    private readonly IWebsiteService _websiteService;

    public ContactController(IContactMessageService contactMessageService, IWebsiteService websiteService)
    {
        _contactMessageService = contactMessageService;
        _websiteService = websiteService;
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] ContactMessageRequest request, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websiteService, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });

        if (string.IsNullOrWhiteSpace(request.SenderName) ||
            string.IsNullOrWhiteSpace(request.EmailAddress) ||
            string.IsNullOrWhiteSpace(request.MessageBody))
            return BadRequest(new { message = "Name, email and message are required." });

        await _contactMessageService.CreateAsync(new ContactUsMessage
        {
            WebsiteID = website.WebsiteID,
            SenderName = request.SenderName.Trim(),
            EmailAddress = request.EmailAddress.Trim(),
            CellphoneNumber = request.CellphoneNumber?.Trim() ?? string.Empty,
            MessageSubject = request.MessageSubject?.Trim() ?? string.Empty,
            MessageBody = request.MessageBody.Trim(),
            SenderIPAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
        }, ct);

        return Ok(new { message = "Your message has been sent." });
    }
}
