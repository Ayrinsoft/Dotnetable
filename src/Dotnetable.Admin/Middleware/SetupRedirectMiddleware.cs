using Dotnetable.Application.Interfaces;

namespace Dotnetable.Admin.Middleware;

/// <summary>
/// Until the first-run setup is completed, redirects every request to /Setup so the
/// administrator can create the master website and account.
/// Uses only the local settings file (no DB round-trip) so a temporary DB outage does
/// not incorrectly send users back to the Setup page.
/// </summary>
public class SetupRedirectMiddleware
{
    private static volatile bool _setupCompleted;
    private readonly RequestDelegate _next;

    public SetupRedirectMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IDatabaseConfigStore configStore)
    {
        if (_setupCompleted || IsExempt(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (configStore.IsConfigured)
        {
            _setupCompleted = true;
            await _next(context);
            return;
        }

        context.Response.Redirect("/Setup");
    }

    private static bool IsExempt(PathString path) =>
        path.StartsWithSegments("/Setup", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/_framework", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/_blazor", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/css", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/lib", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/js", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/favicon", StringComparison.OrdinalIgnoreCase);
}

public static class SetupRedirectMiddlewareExtensions
{
    public static IApplicationBuilder UseSetupRedirect(this IApplicationBuilder app) =>
        app.UseMiddleware<SetupRedirectMiddleware>();
}
