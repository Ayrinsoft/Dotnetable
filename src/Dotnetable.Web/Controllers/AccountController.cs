using System.Net;
using Dotnetable.Application.DTOs;
using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

/// <summary>
/// Customer (client) authentication for the public website — drives the popup: register with email
/// or mobile, activate with a one-time code, sign in, and reset a forgotten password.
/// </summary>
public class AccountController : Controller
{
    private readonly ApiClient _api;

    public AccountController(ApiClient api) => _api = api;

    private async Task IssueSessionAsync(LoginResult token, CancellationToken ct)
    {
        Response.Cookies.Append(ClientAuth.TokenCookie, token.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = token.ExpiresAtUtc,
        });

        // Fold any guest-cart items added before sign-in into the customer's own cart.
        await _api.MergeCartAsync(token.AccessToken, ct);
    }

    public sealed class LoginInput
    {
        public string Identifier { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Identifier) || string.IsNullOrWhiteSpace(input.Password))
            return BadRequest(new { message = "Email/mobile and password are required." });

        var result = await _api.LoginAsync(input.Identifier.Trim(), input.Password, ct);

        if (result.Ok && result.Token is not null)
        {
            await IssueSessionAsync(result.Token, ct);
            return Ok(new { success = true });
        }

        // Correct credentials but the account still needs OTP activation.
        if (result.Status == HttpStatusCode.Forbidden &&
            result.Fields.TryGetValue("status", out var status) && status == "NotActivated")
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                status = "NotActivated",
                identifier = input.Identifier.Trim(),
                message = result.Message ?? "Please verify your account.",
            });
        }

        return Unauthorized(new { message = result.Message ?? "Invalid credentials." });
    }

    public sealed class RegisterInput
    {
        public string GivenName { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? CountryCode { get; set; }
        public string? Cellphone { get; set; }
        public string Password { get; set; } = string.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterInput input, CancellationToken ct = default)
    {
        var hasEmail = !string.IsNullOrWhiteSpace(input.Email);
        var hasPhone = !string.IsNullOrWhiteSpace(input.Cellphone);
        if ((!hasEmail && !hasPhone) || string.IsNullOrWhiteSpace(input.Password))
            return BadRequest(new { message = "Provide an email or a mobile number, and a password." });

        var result = await _api.RegisterAsync(new
        {
            givenName = input.GivenName?.Trim(),
            surname = input.Surname?.Trim(),
            email = input.Email?.Trim(),
            countryCode = input.CountryCode?.Trim(),
            cellphone = input.Cellphone?.Trim(),
            password = input.Password,
        }, ct);

        if (result.Ok)
        {
            return Ok(new
            {
                success = true,
                channel = result.Fields.GetValueOrDefault("channel"),
                identifier = result.Fields.GetValueOrDefault("identifier"),
                message = result.Message ?? "We sent you a verification code.",
            });
        }

        return StatusCode((int)result.Status, new { message = result.Message ?? "Registration failed." });
    }

    public sealed class VerifyInput
    {
        public string Identifier { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Identifier) || string.IsNullOrWhiteSpace(input.Code))
            return BadRequest(new { message = "Enter the code we sent you." });

        var result = await _api.VerifyOtpAsync(input.Identifier.Trim(), input.Code.Trim(), ct);

        if (result.Ok && result.Token is not null)
        {
            await IssueSessionAsync(result.Token, ct);
            return Ok(new { success = true });
        }

        if (result.Ok) // already active, no token
            return Ok(new { success = true, message = result.Message });

        return BadRequest(new { message = result.Message ?? "The code is invalid or has expired." });
    }

    public sealed class IdentifierInput
    {
        public string Identifier { get; set; } = string.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> ResendOtp([FromBody] IdentifierInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Identifier))
            return BadRequest(new { message = "Missing account." });

        var result = await _api.ResendOtpAsync(input.Identifier.Trim(), ct);
        return result.Ok
            ? Ok(new { success = true, message = result.Message ?? "A new code has been sent." })
            : StatusCode((int)result.Status, new { message = result.Message ?? "Could not send a new code." });
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword([FromBody] IdentifierInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Identifier))
            return BadRequest(new { message = "Enter your email or mobile." });

        var result = await _api.ForgotPasswordAsync(input.Identifier.Trim(), ct);
        return result.Ok
            ? Ok(new { success = true, message = result.Message ?? "If the account exists, a reset code has been sent." })
            : StatusCode((int)result.Status, new { message = result.Message ?? "Could not send a reset code." });
    }

    public sealed class ResetInput
    {
        public string Identifier { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword([FromBody] ResetInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Identifier) ||
            string.IsNullOrWhiteSpace(input.Code) ||
            string.IsNullOrWhiteSpace(input.NewPassword))
            return BadRequest(new { message = "Fill in the code and your new password." });

        var result = await _api.ResetPasswordAsync(input.Identifier.Trim(), input.Code.Trim(), input.NewPassword, ct);
        return result.Ok
            ? Ok(new { success = true, message = result.Message ?? "Your password has been reset. You can sign in now." })
            : StatusCode((int)result.Status, new { message = result.Message ?? "The code is invalid or has expired." });
    }

    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(ClientAuth.TokenCookie);
        return Ok(new { success = true });
    }

    // ── Addresses ────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Addresses(CancellationToken ct = default)
    {
        if (!Request.Cookies.ContainsKey(ClientAuth.TokenCookie))
            return RedirectToAction(nameof(HomeController.Index), "Home");

        var addresses = await _api.GetAddressesAsync(ct);
        ViewBag.Countries = await _api.GetCountriesAsync(ct);
        return View(addresses);
    }

    [HttpGet]
    public async Task<IActionResult> Cities(int countryId, CancellationToken ct = default) =>
        Ok(await _api.GetCitiesAsync(countryId, ct));

    public sealed class AddressInput
    {
        public string? Title { get; set; }
        public string? ReceiverName { get; set; }
        public int? CountryId { get; set; }
        public int? CityId { get; set; }
        public string AddressLine { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public string? Phone { get; set; }
        public bool IsDefault { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> AddAddress([FromBody] AddressInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.AddressLine))
            return BadRequest(new { message = "Address line is required." });

        var result = await _api.CreateAddressAsync(ToRequest(input), ct);
        return result.Ok
            ? Ok(new { success = true })
            : StatusCode((int)result.Status, new { message = result.Message ?? "Could not save the address." });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAddress(int id, [FromBody] AddressInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.AddressLine))
            return BadRequest(new { message = "Address line is required." });

        var result = await _api.UpdateAddressAsync(id, ToRequest(input), ct);
        return result.Ok
            ? Ok(new { success = true })
            : StatusCode((int)result.Status, new { message = result.Message ?? "Could not save the address." });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAddress(int id, CancellationToken ct = default)
    {
        var result = await _api.DeleteAddressAsync(id, ct);
        return result.Ok
            ? Ok(new { success = true })
            : StatusCode((int)result.Status, new { message = result.Message ?? "Could not delete the address." });
    }

    [HttpPost]
    public async Task<IActionResult> SetDefaultAddress(int id, CancellationToken ct = default)
    {
        var result = await _api.SetDefaultAddressAsync(id, ct);
        return result.Ok
            ? Ok(new { success = true })
            : StatusCode((int)result.Status, new { message = result.Message ?? "Could not update the address." });
    }

    private static AddressRequest ToRequest(AddressInput input) => new()
    {
        Title = input.Title?.Trim(),
        ReceiverName = input.ReceiverName?.Trim(),
        CountryId = input.CountryId,
        CityId = input.CityId,
        AddressLine = input.AddressLine.Trim(),
        PostalCode = input.PostalCode?.Trim(),
        Phone = input.Phone?.Trim(),
        IsDefault = input.IsDefault,
    };

    // ── Orders ───────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Orders(int page = 1, CancellationToken ct = default)
    {
        if (!Request.Cookies.ContainsKey(ClientAuth.TokenCookie))
            return RedirectToAction(nameof(HomeController.Index), "Home");

        return View(await _api.GetOrdersAsync(page, 10, ct));
    }

    [HttpGet]
    public async Task<IActionResult> OrderDetail(int id, CancellationToken ct = default)
    {
        if (!Request.Cookies.ContainsKey(ClientAuth.TokenCookie))
            return RedirectToAction(nameof(HomeController.Index), "Home");

        var order = await _api.GetOrderAsync(id, ct);
        return order is null ? NotFound() : View(order);
    }

    // ── Wallet ───────────────────────────────────────────────────────

    public sealed record WalletView(WalletBalanceDto Balance, PagedResult<WalletTransactionDto> Transactions, IReadOnlyList<ClientBankAccountDto> BankAccounts);

    [HttpGet]
    public async Task<IActionResult> Wallet(int page = 1, CancellationToken ct = default)
    {
        if (!Request.Cookies.ContainsKey(ClientAuth.TokenCookie))
            return RedirectToAction(nameof(HomeController.Index), "Home");

        var balance = await _api.GetWalletBalanceAsync(ct);
        var transactions = await _api.GetWalletTransactionsAsync(page, 20, ct);
        var bankAccounts = await _api.GetClientBankAccountsAsync(ct);
        return View(new WalletView(balance, transactions, bankAccounts));
    }

    [HttpPost]
    public async Task<IActionResult> RequestWithdrawal(int clientBankAccountId, decimal amountUsd, CancellationToken ct = default)
    {
        var result = await _api.RequestWithdrawalAsync(clientBankAccountId, amountUsd, ct);
        if (!result.Ok) TempData["WalletError"] = result.Message ?? "Could not submit the withdrawal request.";
        return RedirectToAction(nameof(Wallet));
    }

    public sealed class ClientBankAccountInput
    {
        public int? BankID { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string? AccountNumber { get; set; }
        public string? IBAN { get; set; }
        public string? CardNumber { get; set; }
        public bool IsDefault { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> AddBankAccount([FromBody] ClientBankAccountInput input, CancellationToken ct = default)
    {
        var result = await _api.CreateClientBankAccountAsync(new ClientBankAccountRequest
        {
            BankID = input.BankID,
            OwnerName = input.OwnerName.Trim(),
            AccountNumber = input.AccountNumber,
            IBAN = input.IBAN,
            CardNumber = input.CardNumber,
            IsDefault = input.IsDefault,
        }, ct);
        return result.Ok
            ? Ok(new { success = true })
            : StatusCode((int)result.Status, new { message = result.Message ?? "Could not save the bank account." });
    }

    // ── Wishlist ─────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Wishlist(CancellationToken ct = default)
    {
        if (!Request.Cookies.ContainsKey(ClientAuth.TokenCookie))
            return RedirectToAction(nameof(HomeController.Index), "Home");

        return View(await _api.GetWishlistAsync(ct));
    }

    [HttpPost]
    public async Task<IActionResult> AddToWishlist(int variantId, CancellationToken ct = default)
    {
        var result = await _api.AddToWishlistAsync(variantId, ct);
        return result.Ok ? Ok(new { success = true }) : StatusCode((int)result.Status, new { message = result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFromWishlist(int variantId, CancellationToken ct = default)
    {
        await _api.RemoveFromWishlistAsync(variantId, ct);
        return RedirectToAction(nameof(Wishlist));
    }
}
