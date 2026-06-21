using System.Security.Claims;
using Dotnetable.Application.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Dotnetable.Admin.Auth;

public static class AdminPolicies
{
    /// <summary>Members of the master website (WebsiteID 1) only — full access to every website.</summary>
    public const string SuperAdminOnly = "SuperAdminOnly";

    /// <summary>Master members, or any member holding a management role.</summary>
    public const string WebsiteAdminOrAbove = "WebsiteAdminOrAbove";

    /// <summary>Any authenticated member.</summary>
    public const string AnyEditor = "AnyEditor";

    public static void Register(AuthorizationOptions options)
    {
        options.AddPolicy(SuperAdminOnly, p =>
            p.RequireAssertion(ctx => IsMaster(ctx.User)));

        options.AddPolicy(WebsiteAdminOrAbove, p =>
            p.RequireAssertion(ctx => IsMaster(ctx.User) || HasAnyManagementRole(ctx.User)));

        options.AddPolicy(AnyEditor, p =>
            p.RequireAuthenticatedUser());
    }

    /// <summary>True if the user is a master-website member or holds the given role key.</summary>
    public static bool Allows(ClaimsPrincipal user, string roleKey) =>
        IsMaster(user) || user.HasClaim(ClaimTypes.Role, roleKey);

    private static bool IsMaster(ClaimsPrincipal user) =>
        user.HasClaim(AdminClaimTypes.Master, "true");

    private static bool HasAnyManagementRole(ClaimsPrincipal user) =>
        RoleKeys.All.Any(key => user.HasClaim(ClaimTypes.Role, key));
}
