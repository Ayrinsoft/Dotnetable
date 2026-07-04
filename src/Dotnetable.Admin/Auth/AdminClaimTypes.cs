using System.Security.Claims;
using Dotnetable.Application.Authorization;

namespace Dotnetable.Admin.Auth;

/// <summary>Thin alias over the shared <see cref="MemberClaims"/> constants so the admin UI keeps its familiar names.</summary>
public static class AdminClaimTypes
{
    public const string MemberId = MemberClaims.MemberId;
    public const string WebsiteId = MemberClaims.WebsiteId;
    public const string PolicyId = MemberClaims.PolicyId;

    /// <summary>"true" for members of the master website (WebsiteID 1), who have full cross-site access.</summary>
    public const string Master = MemberClaims.Master;

    public const string UiModeClaim = MemberClaims.AdminUiMode;

    /// <summary>Reads the signed-in member's <see cref="AdminUiMode"/>, defaulting to General when absent or unparsable.</summary>
    public static AdminUiMode GetUiMode(ClaimsPrincipal user)
    {
        var raw = user.FindFirst(UiModeClaim)?.Value;
        return byte.TryParse(raw, out var value) && Enum.IsDefined(typeof(AdminUiMode), value)
            ? (AdminUiMode)value
            : AdminUiMode.General;
    }
}
