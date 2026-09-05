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

    /// <summary>The member's <see cref="Member.AdminUIMode"/> (0 = Basic, 1 = General, 2 = Advanced), as a string digit.</summary>
    public const string AdminUiMode = "auimode";

    /// <summary>When the member is bound to a marketplace vendor (VendorType = Member), the VendorID.</summary>
    public const string VendorId = "vid";

    /// <summary>
    /// Builds the identity + role claims for <paramref name="member"/>. The member must have its
    /// Policy → PolicyRoles → Role graph loaded for the role claims to be populated.
    /// Include <see cref="Member.Vendor"/> when present so <see cref="VendorId"/> is emitted.
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
            new(AdminUiMode, member.AdminUIMode.ToString()),
        };

        if (member.WebsiteID == AppConstants.MasterWebsiteId)
            claims.Add(new Claim(Master, "true"));

        if (member.Vendor is { IsActive: true, VendorType: (byte)Domain.Enums.VendorType.Member } v)
            claims.Add(new Claim(VendorId, v.VendorID.ToString()));

        // One role claim per active permission key granted through the member's policy.
        var roleKeys = member.Policy?.PolicyRoles
            .Where(pr => pr.Active && pr.Role.Active)
            .Select(pr => pr.Role.RoleKey)
            .Distinct() ?? Enumerable.Empty<string>();

        claims.AddRange(roleKeys.Select(key => new Claim(ClaimTypes.Role, key)));

        return claims;
    }

    /// <summary>
    /// Same identity claims as <see cref="Build"/> but without the per-permission role claims — for
    /// the admin sign-in cookie only. A member with many granted permissions carried one role claim
    /// per key inside the cookie itself, which could grow the <c>Cookie</c> request header past IIS/
    /// Kestrel's header-size limit ("Request Too Long" / 400). The API's JWT still uses
    /// <see cref="Build"/> as-is — a bearer token has to be self-contained since nothing re-expands it
    /// per request — while the admin cookie stays slim and a per-request claims transformation
    /// expands its <see cref="PolicyId"/> claim back into role claims from cache/DB instead.
    /// </summary>
    public static IReadOnlyList<Claim> BuildForCookie(Member member) =>
        Build(member).Where(c => c.Type != ClaimTypes.Role).ToList();
}
