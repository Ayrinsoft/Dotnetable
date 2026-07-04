using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

/// <summary>The shopping cart page — works for both guests and signed-in customers, backed by the API.</summary>
public class CartController : Controller
{
    private readonly ApiClient _api;

    public CartController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var cart = await _api.GetCartAsync(ct: ct);
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity, CancellationToken ct = default)
    {
        await _api.UpdateCartItemAsync(cartItemId, quantity, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> RemoveItem(int cartItemId, CancellationToken ct = default)
    {
        await _api.RemoveCartItemAsync(cartItemId, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ApplyCoupon(string code, CancellationToken ct = default)
    {
        var result = await _api.ApplyCouponAsync(code, ct);
        if (!result.Ok) TempData["CouponError"] = result.Message ?? "Invalid coupon.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> RemoveCoupon(CancellationToken ct = default)
    {
        await _api.RemoveCouponAsync(ct);
        return RedirectToAction(nameof(Index));
    }
}
