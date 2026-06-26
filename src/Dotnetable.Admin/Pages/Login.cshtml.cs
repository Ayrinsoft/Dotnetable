using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Dotnetable.Admin.Auth;
using Dotnetable.Application;
using Dotnetable.Application.Authorization;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.Admin.Pages;

[AllowAnonymous]
public class LoginModel : CaptchaPageModel
{
    private readonly IMemberService _memberService;
    private readonly ILoginLogService _loginLog;

    public LoginModel(IMemberService memberService, ILoginLogService loginLog, IHumanVerificationService human) : base(human)
    {
        _memberService = memberService;
        _loginLog = loginLog;
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
            return Redirect("/");

        PrepareCaptcha();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            PrepareCaptcha();
            return Page();
        }

        if (!await ValidateCaptchaAsync(ct))
            return Page();

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

        var member = await _memberService.ValidateCredentialsAsync(Input.Username, Input.Password, ct);
        if (member is null)
        {
            // Attribute the failed attempt to the matching member's website when one exists (0 = unknown, master-only).
            var websiteId = await _memberService.GetWebsiteIdByUsernameAsync(Input.Username, ct) ?? 0;
            await _loginLog.RecordAsync(Input.Username, websiteId, false, ip, ct);

            ErrorMessage = "Invalid username or password.";
            PrepareCaptcha();
            return Page();
        }

        await _loginLog.RecordAsync(member.Username, member.WebsiteID, true, ip, ct);

        var claims = MemberClaims.Build(member);
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true });

        return Redirect("/");
    }
}
