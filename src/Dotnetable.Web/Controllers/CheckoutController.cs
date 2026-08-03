using Dotnetable.Application.DTOs;
using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

/// <summary>Address → shipping → payment checkout flow for the signed-in customer's cart.</summary>
public class CheckoutController : Controller
{
    private readonly ApiClient _api;

    public CheckoutController(ApiClient api) => _api = api;

    public sealed record CheckoutView(
        CartViewDto Cart, IReadOnlyList<AddressDto> Addresses, IReadOnlyList<ShippingOptionDto> ShippingOptions);

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        if (!Request.Cookies.ContainsKey(ClientAuth.TokenCookie))
            return RedirectToAction("Index", "Cart");

        var cart = await _api.GetCartAsync(ct: ct);
        if (cart is null || cart.Items.Count == 0)
            return RedirectToAction("Index", "Cart");

        var addresses = await _api.GetAddressesAsync(ct);
        var defaultAddress = addresses.FirstOrDefault(a => a.IsDefault) ?? addresses.FirstOrDefault();
        var cartSubtotal = cart.SubTotal?.Amount ?? 0;
        var shippingOptions = await _api.GetShippingOptionsAsync(
            defaultAddress?.CountryId, defaultAddress?.StateId, defaultAddress?.CityId, cart.TotalWeightKg, cartSubtotal, ct);

        return View(new CheckoutView(cart, addresses, shippingOptions));
    }

    [HttpPost]
    public async Task<IActionResult> Place(int addressId, int shippingMethodId, CancellationToken ct = default)
    {
        var cart = await _api.GetCartAsync(ct: ct);
        if (cart is null) return RedirectToAction(nameof(Index));

        var (success, error, orderId, orderNumber) = await _api.CheckoutAsync(cart.CartID, addressId, shippingMethodId, null, ct);
        if (!success)
        {
            TempData["CheckoutError"] = error ?? "Checkout failed.";
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Payment), new { orderId });
    }

    public async Task<IActionResult> Payment(int orderId, CancellationToken ct = default)
    {
        var order = await _api.GetOrderAsync(orderId, ct);
        if (order is null) return NotFound();

        var wallet = await _api.GetWalletBalanceAsync(ct);
        ViewBag.WalletBalance = wallet.BalanceUsd;
        ViewBag.BankAccounts = await _api.GetOfflineBankAccountsAsync(ct);
        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> PayWithWallet(int orderId, CancellationToken ct = default)
    {
        var result = await _api.PayWithWalletAsync(orderId, ct);
        if (!result.Ok)
        {
            TempData["PaymentError"] = result.Message ?? "Wallet payment failed.";
            return RedirectToAction(nameof(Payment), new { orderId });
        }
        return RedirectToAction(nameof(Confirmation), new { orderId });
    }

    [HttpPost]
    public async Task<IActionResult> SubmitReceipt(int orderId, int bankAccountId, IFormFile receipt, CancellationToken ct = default)
    {
        if (receipt is null || receipt.Length == 0)
        {
            TempData["PaymentError"] = "Please attach the transfer receipt image.";
            return RedirectToAction(nameof(Payment), new { orderId });
        }

        var (uploadOk, fileId, uploadError) = await _api.UploadReceiptAsync(receipt, ct);
        if (!uploadOk)
        {
            TempData["PaymentError"] = uploadError ?? "Could not upload the receipt.";
            return RedirectToAction(nameof(Payment), new { orderId });
        }

        var result = await _api.SubmitBankReceiptAsync(orderId, bankAccountId, fileId!.Value, ct);
        if (!result.Ok)
        {
            TempData["PaymentError"] = result.Message ?? "Could not submit the receipt.";
            return RedirectToAction(nameof(Payment), new { orderId });
        }
        return RedirectToAction(nameof(Confirmation), new { orderId });
    }

    public async Task<IActionResult> Confirmation(int orderId, CancellationToken ct = default)
    {
        var order = await _api.GetOrderAsync(orderId, ct);
        return order is null ? NotFound() : View(order);
    }
}
