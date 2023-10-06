using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Dotnetable.Admin.SharedServices.Authorization;

internal class APIAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    public APIAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) => FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        var policy = new AuthorizationPolicyBuilder();
        policy.AddRequirements(new APIAuthorizationRequirement(policyName));
        return Task.FromResult(policy.Build());
    }
}
