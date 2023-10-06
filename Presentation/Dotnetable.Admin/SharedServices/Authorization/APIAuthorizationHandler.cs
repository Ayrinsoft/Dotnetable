using Dotnetable.Service;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Dotnetable.Admin.SharedServices.Authorization;

internal class APIAuthorizationHandler : AuthorizationHandler<APIAuthorizationRequirement>
{
    AuthenticationService _authentication;
    IConfiguration _config;
    public APIAuthorizationHandler(AuthenticationService authentication, IConfiguration config)
    {
        _authentication = authentication;
        _config = config;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, APIAuthorizationRequirement requirement)
    {
        if (context is not null && context.User is not null && context.User.Claims is not null)
        {
            ClaimsPrincipal user = context.User;
            string fetchHash = user.Claims.FirstOrDefault(c => c.Type.EndsWith("/hash", StringComparison.OrdinalIgnoreCase))?.Value ?? "";
            string fetchLogHash = user.Claims.FirstOrDefault(c => c.Type.EndsWith("/authorizationdecision", StringComparison.OrdinalIgnoreCase))?.Value ?? "";
            string fetchMemberID = user.Claims.FirstOrDefault(c => c.Type.EndsWith("/spn", StringComparison.OrdinalIgnoreCase))?.Value ?? "0";
            string fetchName = context.User.Identity?.Name ?? "";

            if (!string.IsNullOrEmpty(fetchName) && Guid.TryParse(fetchName, out Guid _userHashKey) && !string.IsNullOrEmpty(fetchHash ?? "") && int.TryParse(fetchMemberID, out int _currentMemberID) && AuthenticationService.CheckJWTHash(new() { JWTHashKey = fetchHash, LogHashKey = fetchLogHash, UserHashKey = fetchName, MemberID = _currentMemberID }, _config) && _authentication.UserValidatePolicy(new() { RoleNames = requirement.RoleName.Split(',').ToList(), UserHashKey = _userHashKey }).Result)
                context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}