using Dotnetable.Domain.Entities;

namespace Dotnetable.Admin.Auth;

public static class AdminPolicies
{
    public const string SuperAdminOnly = "SuperAdminOnly";
    public const string WebsiteAdminOrAbove = "WebsiteAdminOrAbove";
    public const string AnyEditor = "AnyEditor";

    public static void Register(Microsoft.AspNetCore.Authorization.AuthorizationOptions options)
    {
        options.AddPolicy(SuperAdminOnly, p =>
            p.RequireRole(nameof(UserRole.SuperAdmin)));

        options.AddPolicy(WebsiteAdminOrAbove, p =>
            p.RequireRole(nameof(UserRole.SuperAdmin), nameof(UserRole.WebsiteAdmin)));

        options.AddPolicy(AnyEditor, p =>
            p.RequireAuthenticatedUser());
    }
}
