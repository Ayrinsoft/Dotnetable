using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Dotnetable.Admin.Auth;
using Dotnetable.Admin.Localization;
using Dotnetable.Application;
using Dotnetable.Application.Authorization;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dotnetable.Admin.Pages;

[AllowAnonymous]
[EnableRateLimiting(Dotnetable.Hosting.RateLimiting.AuthPolicy)]
public class LoginModel : CaptchaPageModel
{
    /// <summary>
    /// Session key holding the member id between the password step and the code step. The id alone is
    /// not an authentication: nothing is signed in until <see cref="OnPostVerifyAsync"/> succeeds, so
    /// a leaked value grants nothing without the second factor.
    /// </summary>
    private const string PendingMemberSessionKey = "dn-2fa-pending";

    /// <summary>How long the half-finished sign-in stays valid before the password must be re-entered.</summary>
    private static readonly TimeSpan TwoFactorWindow = TimeSpan.FromMinutes(5);

    private const string PendingSinceSessionKey = "dn-2fa-since";

    private readonly IMemberService _memberService;
    private readonly ILoginLogService _loginLog;

    public LoginModel(IMemberService memberService, ILoginLogService loginLog, IHumanVerificationService human,
        IAuthLanguageResolver langResolver, ILanguageService languageService) : base(human, langResolver, languageService)
    {
        _memberService = memberService;
        _loginLog = loginLog;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty]
    public string? TwoFactorCode { get; set; }

    /// <summary>Where to land after a successful sign-in. Only ever followed when it is a local path.</summary>
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    /// <summary>Set by /Logout: show the form even if a cookie somehow survived the sign-out.</summary>
    [BindProperty(SupportsGet = true)]
    public bool SignedOut { get; set; }

    public string? ErrorMessage { get; set; }

    /// <summary>True once the password was accepted and the page is asking for the authenticator code.</summary>
    public bool AwaitingTwoFactor { get; set; }

    public class InputModel
    {
        [Required] public string Username { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        // A cached login document posts a dead antiforgery token. Browsers report that as 400/403,
        // and clearing cookies does not help while the HTML itself is still the cached copy.
        Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        Response.Headers.Pragma = "no-cache";

        // signedOut/signout/logout=1 must not stay on the address. The form used to post back to
        // that exact URL, and a returnUrl pointing at it signed the member out again after every
        // successful login — an infinite loop that also trips IIS into a 403.
        if (RequestHasSignOutFlag())
        {
            if (User.Identity?.IsAuthenticated == true)
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return Redirect(CleanLoginUrl());
        }

        if (User.Identity?.IsAuthenticated == true)
            return Redirect(SafeReturnUrl() ?? "/");

        // A stale half-finished sign-in must not survive a page reload.
        ClearPendingTwoFactor();

        await ResolveLanguageAsync(ct);
        PrepareCaptcha();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        await ResolveLanguageAsync(ct);

        if (!ModelState.IsValid)
        {
            PrepareCaptcha();
            return Page();
        }

        if (!await ValidateCaptchaAsync(ct))
            return Page();

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var result = await _memberService.ValidateSignInAsync(Input.Username, Input.Password, ct);

        switch (result.Status)
        {
            case MemberSignInStatus.Success when result.Member is not null:
                await _loginLog.RecordAsync(result.Member.Username, result.Member.WebsiteID, true, ip, ct);
                await SignInAsync(result.Member);
                return RedirectAfterSignIn();

            case MemberSignInStatus.TwoFactorRequired when result.Member is not null:
                // The password was right, so this counts as neither a success nor a failure yet; the
                // log entry is written when the second factor resolves it one way or the other.
                BeginPendingTwoFactor(result.Member.MemberID);
                AwaitingTwoFactor = true;
                PrepareCaptcha();
                return Page();

            case MemberSignInStatus.LockedOut:
                await RecordFailureAsync(Input.Username, ip, ct);
                ErrorMessage = LockoutMessage(result.LockoutEndUtc);
                PrepareCaptcha();
                return Page();

            default:
                await RecordFailureAsync(Input.Username, ip, ct);
                ErrorMessage = S.InvalidCredentials;
                PrepareCaptcha();
                return Page();
        }
    }

    /// <summary>Back to the page the member was refused, when it is a local app path; the dashboard otherwise.</summary>
    private IActionResult RedirectAfterSignIn() => Redirect(SafeReturnUrl() ?? "/");

    private string? SafeReturnUrl() =>
        !string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl) && AuthReturnUrl.IsSafe(ReturnUrl)
            ? ReturnUrl
            : null;

    private bool RequestHasSignOutFlag()
    {
        if (SignedOut) return true;
        foreach (var key in Request.Query.Keys)
        {
            if (AuthReturnUrl.IsSignOutKey(key)) return true;
        }
        return false;
    }

    /// <summary>Login without a sign-out flag. A safe returnUrl is kept so a refused page is not lost.</summary>
    private string CleanLoginUrl()
    {
        var safe = SafeReturnUrl();
        return safe is null ? "/Login" : "/Login?returnUrl=" + Uri.EscapeDataString(safe);
    }

    /// <summary>Second step: the authenticator (or recovery) code for a member that passed the password step.</summary>
    public async Task<IActionResult> OnPostVerifyAsync(CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        await ResolveLanguageAsync(ct);

        var pending = ReadPendingTwoFactor();
        if (pending is null)
        {
            // Window elapsed or the session was dropped — start over rather than leaving the member
            // on a code prompt that can never succeed.
            ErrorMessage = S.InvalidCredentials;
            PrepareCaptcha();
            return Page();
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var result = await _memberService.VerifyTwoFactorAsync(pending.Value, TwoFactorCode ?? string.Empty, ct);

        switch (result.Status)
        {
            case MemberSignInStatus.Success when result.Member is not null:
                ClearPendingTwoFactor();
                await _loginLog.RecordAsync(result.Member.Username, result.Member.WebsiteID, true, ip, ct);
                await SignInAsync(result.Member);
                return RedirectAfterSignIn();

            case MemberSignInStatus.LockedOut:
                ClearPendingTwoFactor();
                ErrorMessage = LockoutMessage(result.LockoutEndUtc);
                PrepareCaptcha();
                return Page();

            default:
                AwaitingTwoFactor = true;
                ErrorMessage = "The authentication code is incorrect or has expired.";
                PrepareCaptcha();
                return Page();
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────

    private async Task SignInAsync(Member member)
    {
        var claims = MemberClaims.BuildForCookie(member);
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true });
    }

    private async Task RecordFailureAsync(string username, string ip, CancellationToken ct)
    {
        // Attribute the failed attempt to the matching member's website when one exists.
        // Unknown usernames stay unattributed (null) — do not send 0; there is no WebsiteID 0.
        var websiteId = await _memberService.GetWebsiteIdByUsernameAsync(username, ct);
        await _loginLog.RecordAsync(username, websiteId, false, ip, ct);
    }

    private static string LockoutMessage(DateTime? until)
    {
        var minutes = until is DateTime when
            ? Math.Max(1, (int)Math.Ceiling((when - DateTime.UtcNow).TotalMinutes))
            : 15;
        return $"Too many failed attempts. Please try again in {minutes} minute(s).";
    }

    private void BeginPendingTwoFactor(int memberId)
    {
        HttpContext.Session.SetInt32(PendingMemberSessionKey, memberId);
        HttpContext.Session.SetString(PendingSinceSessionKey, DateTime.UtcNow.ToString("O"));
    }

    private int? ReadPendingTwoFactor()
    {
        var memberId = HttpContext.Session.GetInt32(PendingMemberSessionKey);
        if (memberId is null) return null;

        var since = HttpContext.Session.GetString(PendingSinceSessionKey);
        if (!DateTime.TryParse(since, null, System.Globalization.DateTimeStyles.RoundtripKind, out var started) ||
            DateTime.UtcNow - started > TwoFactorWindow)
        {
            ClearPendingTwoFactor();
            return null;
        }

        return memberId;
    }

    private void ClearPendingTwoFactor()
    {
        HttpContext.Session.Remove(PendingMemberSessionKey);
        HttpContext.Session.Remove(PendingSinceSessionKey);
    }
}
