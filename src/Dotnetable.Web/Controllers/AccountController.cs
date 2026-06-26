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

    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(ClientAuth.TokenCookie);
        return Ok(new { success = true });
    }
}
