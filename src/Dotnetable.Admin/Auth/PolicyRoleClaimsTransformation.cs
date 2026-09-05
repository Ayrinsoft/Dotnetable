using System.Security.Claims;
using Dotnetable.Application.Authorization;
using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Caching;
using Dotnetable.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Admin.Auth;

/// <summary>
/// Expands the signed-in member's <see cref="MemberClaims.PolicyId"/> claim into one
/// <see cref="ClaimTypes.Role"/> claim per granted permission key, computed fresh on each request
/// instead of being baked into the sign-in cookie — see <see cref="MemberClaims.BuildForCookie"/> for
/// why. Cached per policy so this does not mean a DB round trip on every request.
/// </summary>
public class PolicyRoleClaimsTransformation : IClaimsTransformation
{
    private const string Tag = "policy-roles";

    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ICacheService _cache;
    private readonly TimeSpan _ttl;

    public PolicyRoleClaimsTransformation(IDbContextFactory<AppDbContext> contextFactory, ICacheService cache, CacheOptions options)
    {
        _contextFactory = contextFactory;
        _cache = cache;
        _ttl = options.DefaultTtl;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identity as ClaimsIdentity;
        if (identity is null || !identity.IsAuthenticated) return principal;

        // Master members bypass every policy check (see AdminPolicies.Allows) and never need role claims.
        if (identity.HasClaim(AdminClaimTypes.Master, "true")) return principal;

        // TransformAsync can run more than once per request; avoid stacking duplicate claims.
        if (identity.HasClaim(c => c.Type == ClaimTypes.Role)) return principal;

        var policyIdValue = identity.FindFirst(MemberClaims.PolicyId)?.Value;
        if (!int.TryParse(policyIdValue, out var policyId)) return principal;

        var roleKeys = await _cache.GetOrCreateAsync(
            $"{Tag}:{policyId}",
            [Tag],
            _ttl,
            () => LoadRoleKeysAsync(policyId));

        foreach (var key in roleKeys)
            identity.AddClaim(new Claim(ClaimTypes.Role, key));

        return principal;
    }

    private async Task<List<string>> LoadRoleKeysAsync(int policyId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.PolicyRoles
            .Where(pr => pr.PolicyID == policyId && pr.Active && pr.Role.Active)
            .Select(pr => pr.Role.RoleKey)
            .Distinct()
            .ToListAsync();
    }
}
