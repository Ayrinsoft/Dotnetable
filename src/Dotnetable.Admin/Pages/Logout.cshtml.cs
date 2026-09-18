using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dotnetable.Admin.Pages;

/// <summary>
/// Signs the member out and sends them to the login form. Everything that carries sign-in state goes
/// in one pass — the auth cookie, the half-finished two-factor session and its cookie — because a
/// leftover piece leaves the member in a state where they are "signed in" enough to be bounced away
/// from /Login but not enough to use the panel, which they can only escape by clearing cookies.
/// </summary>
[AllowAnonymous]
public class LogoutModel : PageModel
{
    public Task<IActionResult> OnGetAsync() => SignOutAsync();

    public Task<IActionResult> OnPostAsync() => SignOutAsync();

    private async Task<IActionResult> SignOutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        HttpContext.Session.Clear();
        Response.Cookies.Delete("dn-admin-session");

        // Without this the browser can serve a cached, still-"signed in" page from its back/forward
        // cache after the sign-out.
        Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        Response.Headers.Pragma = "no-cache";

        // signedOut=1 tells the login page to show the form instead of bouncing to the dashboard,
        // in case a stale cookie somehow survived.
        return Redirect("/Login?signedOut=1");
    }
}
