using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;

namespace Dotnetable.API.Middleware;

public class ApiKeyMiddleware
{
    private const string ApiKeyHeader = "X-Api-Key";
    private readonly RequestDelegate _next;

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService)
    {
        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var keyValue))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "API key is missing." });
            return;
        }

        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        var apiKey = await apiKeyService.ValidateAsync(keyValue.ToString(), clientIp);

        if (apiKey is null)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid or unauthorized API key." });
            return;
        }

        context.Items["ApiKey"] = apiKey;
        context.Items["WebsiteId"] = apiKey.WebsiteId;

        await _next(context);
    }
}

public static class ApiKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder app) =>
        app.UseMiddleware<ApiKeyMiddleware>();
}
