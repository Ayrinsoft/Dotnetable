using Dotnetable.Application.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Dotnetable.Admin.Auth;

/// <summary>
/// Wired as <see cref="CookieAuthenticationEvents.OnValidatePrincipal"/>. A sign-in cookie carries a
/// <see cref="MemberClaims.ClaimsVersion"/> claim stamped at login; when the shape of the claims this
/// app issues changes (see the incident note on <see cref="MemberClaims.CurrentClaimsVersion"/>), a
/// cookie issued before the change no longer matches and would otherwise keep sliding its expiration
/// forward forever with its stale/incompatible claims never re-checked — surfacing as authorization
/// failures a signed-in member can't explain or fix short of manually clearing cookies. Rejecting the
/// principal here signs them out and sends them back to a normal login instead.
/// </summary>
public static class StaleCookieValidator
{
    public static Task ValidateAsync(CookieValidatePrincipalContext context)
    {
        var version = context.Principal?.FindFirst(MemberClaims.ClaimsVersion)?.Value;
        if (version != MemberClaims.CurrentClaimsVersion)
        {
            context.RejectPrincipal();
            return context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        return Task.CompletedTask;
    }
}
