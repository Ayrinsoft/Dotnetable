using Dotnetable.Web.Services;

namespace Dotnetable.Web.Infrastructure;

/// <summary>
/// Consults the API's redirect rules for any GET request that would otherwise 404. A matching rule
/// issues the configured redirect (301/302/307/308). Running only on unmatched paths keeps the API
/// call off the hot path for pages, static files and endpoints that already resolve.
/// </summary>
public sealed class RedirectMiddleware
{
    private readonly RequestDelegate _next;

    public RedirectMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ApiClient api)
    {
        await _next(context);

        if (context.Response.HasStarted || context.Response.StatusCode != StatusCodes.Status404NotFound)
            return;
        if (!HttpMethods.IsGet(context.Request.Method))
            return;

        var path = context.Request.Path.HasValue ? context.Request.Path.Value! : "/";
        var result = await api.ResolveRedirectAsync(path, context.RequestAborted);
        if (result is null || string.IsNullOrWhiteSpace(result.TargetPath))
            return;

        var permanent = result.StatusCode is 301 or 308;
        context.Response.Clear();
        context.Response.StatusCode = result.StatusCode is 301 or 302 or 307 or 308 ? result.StatusCode : 302;
        context.Response.Headers.Location = result.TargetPath;
        _ = permanent; // status code already carries permanence; kept for clarity
    }
}
