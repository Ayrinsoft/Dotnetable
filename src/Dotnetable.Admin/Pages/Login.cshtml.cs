using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Dotnetable.Admin.Auth;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetable.Admin.Pages;

public class LoginModel : PageModel
{
    private readonly IUserService _userService;

    public LoginModel(IUserService userService)
    {
        _userService = userService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required] public string Username { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToPage("/Dashboard");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await _userService.ValidateCredentialsAsync(Input.Username, Input.Password);
        if (user is null)
        {
            ErrorMessage = "Invalid username or password.";
            return Page();
        }

        var claims = BuildClaims(user);
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true });

        user.LastLoginAt = DateTime.UtcNow;

        return RedirectToPage("/Dashboard");
    }

    private static List<Claim> BuildClaims(User user) =>
    [
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Name, user.Username),
        new(ClaimTypes.Email, user.Email),
        new(ClaimTypes.Role, user.Role.ToString()),
        new(AdminClaimTypes.UserId, user.Id.ToString()),
        new(AdminClaimTypes.Role, user.Role.ToString()),
        .. user.WebsiteId.HasValue
            ? new[] { new Claim(AdminClaimTypes.WebsiteId, user.WebsiteId.Value.ToString()) }
            : Array.Empty<Claim>(),
    ];
}
