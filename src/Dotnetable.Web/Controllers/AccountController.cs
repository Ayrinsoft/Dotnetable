using System.Net;
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

    private void IssueSession(LoginResult token) =>
        Response.Cookies.Append(ClientAuth.TokenCookie, token.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = token.ExpiresAtUtc,
        });

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
            IssueSession(result.Token);
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
            IssueSession(result.Token);
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
}
