using Microsoft.AspNetCore.Authorization;

namespace Dotnetable.Admin.SharedServices.Authorization;

internal class APIAuthorizationRequirement : IAuthorizationRequirement
{
    public APIAuthorizationRequirement(string roleName) => RoleName = roleName;

    public string RoleName { get; private set; }
}
