using Dotnetable.Application.Authorization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>Signed-in customer return (RMA) pre-requests and shipment updates.</summary>
[Authorize(Policy = RoleKeys.ClientPurchase)]
public class ReturnsController : BaseController
{
    private readonly ICustomerReturnService _returns;
    private readonly IWebsiteService _websites;
    private readonly IFileService _files;
    private readonly IStorageSettingService _storage;

    public ReturnsController(
        ICustomerReturnService returns,
        IWebsiteService websites,
        IFileService files,
        IStorageSettingService storage)
    {
        _returns = returns;
        _websites = websites;
        _files = files;
        _storage = storage;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _returns.GetPagedAsync(CurrentWebsiteId, CurrentClientId, null,
            new GridQuery { PageIndex = page, PageSize = pageSize }, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var row = await _returns.GetByIdAsync(id, CurrentClientId, ct);
        return row is null ? NotFound() : Ok(row);
    }

    [HttpGet("eligible/{orderId:int}")]
    public async Task<IActionResult> Eligible(int orderId, CancellationToken ct = default)
        => Ok(await _returns.GetEligibilityAsync(orderId, CurrentClientId, ct));

    public sealed class CreateReturnRequest
    {
        public int OrderId { get; set; }
        public byte Reason { get; set; }
        public string? ReasonNote { get; set; }
        public string? Description { get; set; }
        public string? ShipMethod { get; set; }
        public List<CustomerReturnLineInput> Lines { get; set; } = new();
        public List<int>? PhotoFileIds { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReturnRequest body, CancellationToken ct = default)
    {
        if (body.OrderId <= 0) return BadRequest(new { message = "Order is required." });
        var reason = Enum.IsDefined(typeof(CustomerReturnReason), body.Reason)
            ? (CustomerReturnReason)body.Reason
            : CustomerReturnReason.Other;
        var (ok, err, row) = await _returns.CreateAsync(
            CurrentWebsiteId, CurrentClientId, body.OrderId, reason, body.ReasonNote, body.Description,
            body.ShipMethod, body.Lines, body.PhotoFileIds, ct);
        return ok ? Ok(row) : BadRequest(new { message = err });
    }

    [HttpPost("{id:int}/photos")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile file, CancellationToken ct = default)
    {
        var website = await ResolveWebsiteAsync(_websites, ct);
        if (website is null) return NotFound(new { message = "Website could not be resolved." });
        if (file is null || file.Length == 0) return BadRequest(new { message = "No file uploaded." });

        var storages = await _storage.GetActiveForWebsiteAsync(website.WebsiteID, ct);
        var storage = storages.FirstOrDefault();
        if (storage is null) return StatusCode(503, new { message = "No storage backend is configured for this website." });

        await using var stream = file.OpenReadStream();
        var record = await _files.UploadAsync(new FileUploadRequest
        {
            WebsiteID = website.WebsiteID,
            StorageSettingID = storage.WebsiteStorageSettingsID,
            Content = stream,
            OriginalFileName = file.FileName,
            MimeType = file.ContentType,
            Title = "Return photo",
        }, ct);

        var (ok, err) = await _returns.AttachPhotoAsync(id, CurrentClientId, record.FileRecordID, ct);
        return ok ? Ok(new { fileId = record.FileRecordID }) : BadRequest(new { message = err });
    }

    public sealed class ShipReturnRequest
    {
        public string TrackingCode { get; set; } = "";
        public string? ShipMethod { get; set; }
    }

    [HttpPost("{id:int}/ship")]
    public async Task<IActionResult> Ship(int id, [FromBody] ShipReturnRequest body, CancellationToken ct = default)
    {
        var (ok, err) = await _returns.SubmitShipmentAsync(id, CurrentClientId, body.ShipMethod, body.TrackingCode, ct);
        return ok ? Ok() : BadRequest(new { message = err });
    }

    [HttpPut("{id:int}/tracking")]
    public async Task<IActionResult> UpdateTracking(int id, [FromBody] ShipReturnRequest body, CancellationToken ct = default)
    {
        var (ok, err) = await _returns.UpdateTrackingAsync(id, CurrentClientId, body.TrackingCode, body.ShipMethod, ct);
        return ok ? Ok() : BadRequest(new { message = err });
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct = default)
    {
        var (ok, err) = await _returns.CancelAsync(id, CurrentClientId, null, "Cancelled by customer", ct);
        return ok ? Ok() : BadRequest(new { message = err });
    }

    private int CurrentClientId =>
        int.TryParse(User.FindFirst(ClientClaims.ClientId)?.Value, out var id)
            ? id
            : throw new InvalidOperationException("Token does not carry a client id.");
}
