using Dotnetable.Application.DTOs;
using Dotnetable.Hosting;
using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dotnetable.Web.Controllers;

/// <summary>Public digital visiting card at <c>/c/{slug}</c>.</summary>
public class VisitCardController : Controller
{
    private readonly ApiClient _api;

    public VisitCardController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return NotFound();
        var card = await _api.GetVisitCardAsync(slug, ct);
        return card is null ? NotFound() : View("Index", card);
    }

    [HttpPost]
    [EnableRateLimiting(RateLimiting.PublicWritePolicy)]
    public async Task<IActionResult> Contact(string slug, [FromBody] VisitCardContactRequest request, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(request.Company))
            return Json(new { ok = true });

        var result = await _api.SubmitVisitCardContactAsync(slug, request, ct);
        if (result.Ok) return Json(new { ok = true });
        result.Fields.TryGetValue("code", out var code);
        return StatusCode((int)result.Status, new { ok = false, code, message = result.Message });
    }
}
