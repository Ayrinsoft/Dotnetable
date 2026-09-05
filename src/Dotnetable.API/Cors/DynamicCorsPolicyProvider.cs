using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Dotnetable.API.Cors;

/// <summary>
/// Hands the CORS middleware a policy built fresh per request from <see cref="DynamicCorsOriginProvider"/>
/// instead of a fixed policy captured once at startup — this is the supported extension point for a
/// database-backed allow list (the middleware itself has no async/DI-aware origin check).
/// </summary>
public class DynamicCorsPolicyProvider : ICorsPolicyProvider
{
    private readonly DynamicCorsOriginProvider _origins;

    public DynamicCorsPolicyProvider(DynamicCorsOriginProvider origins) => _origins = origins;

    public Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName)
    {
        var policy = new CorsPolicyBuilder()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_origins.IsAllowed)
            .Build();

        return Task.FromResult<CorsPolicy?>(policy);
    }
}
