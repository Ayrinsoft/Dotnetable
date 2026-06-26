using System.Security.Claims;
using Dotnetable.Application.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Dotnetable.Admin.Auth;

public static class AdminPolicies
{
    /// <summary>Members of the master website (WebsiteID 1) only — full access to every website.</summary>
    public const string SuperAdminOnly = "SuperAdminOnly";

    /// <summary>Any authenticated member (e.g. the dashboard landing page).</summary>
    public const string AnyEditor = "AnyEditor";

    public static void Register(AuthorizationOptions options)
    {
        options.AddPolicy(SuperAdminOnly, p =>
            p.RequireAssertion(ctx => IsMaster(ctx.User)));

        options.AddPolicy(AnyEditor, p =>
            p.RequireAuthenticatedUser());

        // One policy per admin permission key. The policy name IS the role key, so pages can
        // guard themselves with [Authorize(Policy = RoleKeys.MembersEdit)]. Master members pass
        // every check; everyone else must hold the exact key in their granted roles.
        foreach (var key in RoleCatalog.AdminKeys)
        {
            var roleKey = key; // capture for the closure
            options.AddPolicy(roleKey, p =>
                p.RequireAssertion(ctx => Allows(ctx.User, roleKey)));
        }
    }

    /// <summary>True if the user is a master-website member or holds the given role key.</summary>
    public static bool Allows(ClaimsPrincipal user, string roleKey) =>
        IsMaster(user) || user.HasClaim(ClaimTypes.Role, roleKey);

    private static bool IsMaster(ClaimsPrincipal user) =>
        user.HasClaim(AdminClaimTypes.Master, "true");
}
