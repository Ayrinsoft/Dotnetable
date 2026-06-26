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
}
