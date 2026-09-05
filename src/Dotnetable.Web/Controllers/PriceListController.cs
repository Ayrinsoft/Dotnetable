using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

/// <summary>Public FX-linked catalog price lists (steel, profiles, sheets, …).</summary>
public class PriceListController : Controller
{
    private readonly ApiClient _api;

    public PriceListController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var lists = await _api.GetPriceListsAsync(ct);
        return View(lists);
    }

    public async Task<IActionResult> Detail(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return NotFound();
        var list = await _api.GetPriceListAsync(slug, ct);
        return list is null ? NotFound() : View(list);
    }
}
