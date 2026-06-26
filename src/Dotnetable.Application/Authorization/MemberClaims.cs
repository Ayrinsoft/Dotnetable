using System.Security.Claims;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Authorization;

/// <summary>
/// Single source of truth for the claims that identify a signed-in member, shared by the admin
/// cookie sign-in and the API JWT issuance so a token means the same thing everywhere.
/// </summary>
public static class MemberClaims
{
    public const string MemberId = "mid";
    public const string WebsiteId = "wid";
    public const string PolicyId = "pid";

    /// <summary>"true" for members of the master website (full cross-site access).</summary>
    public const string Master = "master";

    /// <summary>
    /// Builds the identity + role claims for <paramref name="member"/>. The member must have its
    /// Policy → PolicyRoles → Role graph loaded for the role claims to be populated.
    /// </summary>
    public static IReadOnlyList<Claim> Build(Member member)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, member.MemberID.ToString()),
            new(ClaimTypes.Name, member.Username),
            new(ClaimTypes.Email, member.Email),
            new(MemberId, member.MemberID.ToString()),
            new(WebsiteId, member.WebsiteID.ToString()),
            new(PolicyId, member.PolicyID.ToString()),
        };

        if (member.WebsiteID == AppConstants.MasterWebsiteId)
            claims.Add(new Claim(Master, "true"));

        // One role claim per active permission key granted through the member's policy.
        var roleKeys = member.Policy?.PolicyRoles
            .Where(pr => pr.Active && pr.Role.Active)
            .Select(pr => pr.Role.RoleKey)
            .Distinct() ?? Enumerable.Empty<string>();

        claims.AddRange(roleKeys.Select(key => new Claim(ClaimTypes.Role, key)));

        return claims;
    }
}
