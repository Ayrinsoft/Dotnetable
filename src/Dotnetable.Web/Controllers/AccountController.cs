using Dotnetable.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Web.Controllers;

/// <summary>Customer (client) authentication for the public website — drives the popup login.</summary>
public class AccountController : Controller
{
    private readonly ApiClient _api;

    public AccountController(ApiClient api) => _api = api;

    public sealed class LoginInput
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Username) || string.IsNullOrWhiteSpace(input.Password))
            return BadRequest(new { message = "Username and password are required." });

        var result = await _api.LoginAsync(input.Username.Trim(), input.Password, ct);
        if (result is null)
            return Unauthorized(new { message = "Invalid username or password." });

        Response.Cookies.Append(ClientAuth.TokenCookie, result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = result.ExpiresAtUtc,
        });

        return Ok(new { success = true });
    }

    public sealed class RegisterInput
    {
        public string GivenName { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Drives the popup register form. Public self-registration needs a target website and
    /// member access level (PolicyID) decided at the API; until that endpoint exists this
    /// validates the input and returns a clear message rather than creating a half-formed member.
    /// </summary>
    [HttpPost]
    public IActionResult Register([FromBody] RegisterInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Username) ||
            string.IsNullOrWhiteSpace(input.Password) ||
            string.IsNullOrWhiteSpace(input.Email) ||
            string.IsNullOrWhiteSpace(input.GivenName) ||
            string.IsNullOrWhiteSpace(input.Surname))
        {
            return BadRequest(new { message = "Please fill in all fields." });
        }

        // TODO: call _api.RegisterAsync(...) once the API exposes public self-registration
        // (it must resolve the target website and a default member access level).
        return StatusCode(StatusCodes.Status501NotImplemented,
            new { message = "Online registration isn't available yet. Please contact us to create an account." });
    }

    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(ClientAuth.TokenCookie);
        return Ok(new { success = true });
    }
}
