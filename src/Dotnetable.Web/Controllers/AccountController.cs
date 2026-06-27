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
    /// Drives the popup register form. The API resolves the target website from the configured
    /// website key and assigns the website's default "Users" access level to the new member.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.Username) ||
            string.IsNullOrWhiteSpace(input.Password) ||
            string.IsNullOrWhiteSpace(input.Email) ||
            string.IsNullOrWhiteSpace(input.GivenName) ||
            string.IsNullOrWhiteSpace(input.Surname))
        {
            return BadRequest(new { message = "Please fill in all fields." });
        }

        var result = await _api.RegisterAsync(new
        {
            givenName = input.GivenName.Trim(),
            surname = input.Surname.Trim(),
            email = input.Email.Trim(),
            username = input.Username.Trim(),
            password = input.Password,
        }, ct);

        if (result.Success)
            return Ok(new { success = true, message = result.Message ?? "Account created. You can sign in now." });

        return BadRequest(new { message = result.Message ?? "Registration failed. Please try again." });
    }

    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(ClientAuth.TokenCookie);
        return Ok(new { success = true });
    }
}
