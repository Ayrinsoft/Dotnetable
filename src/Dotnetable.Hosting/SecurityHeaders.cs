using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Dotnetable.Hosting;

/// <summary>
/// The response headers every host sends. These are cheap, apply to all three apps, and each one
/// closes a class of attack that no amount of application code can close on its own.
/// </summary>
public static class SecurityHeaders
{
    /// <summary>
    /// Adds the baseline headers.
    ///
    /// <para><c>X-Content-Type-Options: nosniff</c> is the important one for this codebase: uploaded
    /// media is served back with a MIME type the uploader chose, so without it a browser is free to
    /// sniff an "image" that is really HTML and run it on the serving origin.</para>
    ///
    /// <para><paramref name="contentSecurityPolicy"/> is opt-in per host because the admin panel
    /// (Blazor + MudBlazor + CKEditor) and the storefront themes have very different script needs; a
    /// policy strict enough for one breaks the other. Pass null to skip CSP and set it deliberately
    /// once each host's asset origins are known.</para>
    /// </summary>
    public static IApplicationBuilder UseDotnetableSecurityHeaders(
        this IApplicationBuilder app,
        string? contentSecurityPolicy = null,
        bool allowFraming = false)
    {
        return app.Use(async (context, next) =>
        {
            var headers = context.Response.Headers;

            headers["X-Content-Type-Options"] = "nosniff";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            // Clickjacking. The storefront may legitimately want to be embedded (a payment page in an
            // iframe, a widget); the admin never should.
            if (!allowFraming)
                headers["X-Frame-Options"] = "DENY";

            // Nothing in this product uses the camera, microphone or geolocation, so deny them
            // outright rather than leaving the decision to whatever gets embedded later.
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=(), usb=()";

            if (!string.IsNullOrWhiteSpace(contentSecurityPolicy))
                headers["Content-Security-Policy"] = contentSecurityPolicy;

            await next();
        });
    }
}
