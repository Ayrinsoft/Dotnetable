using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Dotnetable.Admin.Auth;
using Dotnetable.Application;
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

    public LoginModel(IMemberService memberService, IHumanVerificationService human) : base(human)
    {
        _memberService = memberService;
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

        var member = await _memberService.ValidateCredentialsAsync(Input.Username, Input.Password, ct);
        if (member is null)
        {
            ErrorMessage = "Invalid username or password.";
            PrepareCaptcha();
            return Page();
        }

        var claims = BuildClaims(member);
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true });

        return Redirect("/");
    }

    private static List<Claim> BuildClaims(Member member)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, member.MemberID.ToString()),
            new(ClaimTypes.Name, member.Username),
            new(ClaimTypes.Email, member.Email),
            new(AdminClaimTypes.MemberId, member.MemberID.ToString()),
            new(AdminClaimTypes.WebsiteId, member.WebsiteID.ToString()),
            new(AdminClaimTypes.PolicyId, member.PolicyID.ToString()),
        };

        if (member.WebsiteID == AppConstants.MasterWebsiteId)
            claims.Add(new Claim(AdminClaimTypes.Master, "true"));

        // One role claim per permission key granted through the member's policy.
        var roleKeys = member.Policy?.PolicyRoles
            .Where(pr => pr.Active && pr.Role.Active)
            .Select(pr => pr.Role.RoleKey)
            .Distinct() ?? Enumerable.Empty<string>();

        claims.AddRange(roleKeys.Select(key => new Claim(ClaimTypes.Role, key)));

        return claims;
    }
}
