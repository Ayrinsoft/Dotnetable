using System.Security.Claims;
using Dotnetable.Application.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Dotnetable.API.Auth;

/// <summary>
/// Registers one authorization policy per permission key (admin + client), each named exactly the
/// RoleKey, so endpoints guard themselves with <c>[Authorize(Policy = RoleKeys.ClientPurchase)]</c>.
/// Mirrors the admin panel's role-based model: master-website members pass every check.
/// </summary>
public static class ApiAuthorization
{
    /// <summary>Master-website members only.</summary>
    public const string SuperAdminOnly = "SuperAdminOnly";

    public static void Register(AuthorizationOptions options)
    {
        options.AddPolicy(SuperAdminOnly, p =>
            p.RequireAssertion(ctx => IsMaster(ctx.User)));

        foreach (var def in RoleCatalog.All)
        {
            var roleKey = def.Key; // capture for the closure
            options.AddPolicy(roleKey, p =>
                p.RequireAssertion(ctx => Allows(ctx.User, roleKey)));
        }
    }

    /// <summary>True if the user is a master-website member or holds the given role key.</summary>
    public static bool Allows(ClaimsPrincipal user, string roleKey) =>
        IsMaster(user) || user.HasClaim(ClaimTypes.Role, roleKey);

    private static bool IsMaster(ClaimsPrincipal user) =>
        user.HasClaim(MemberClaims.Master, "true");
}
