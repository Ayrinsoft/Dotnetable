namespace Dotnetable.Application.Authorization;

/// <summary>
/// Titles of the access levels (policies) seeded for every website on first setup.
/// They are looked up by these exact titles, so keep them stable.
/// </summary>
public static class DefaultPolicies
{
    /// <summary>Full-access policy for the super administrator of the master website (WebsiteID 1).</summary>
    public const string Administrators = "Administrators";

    /// <summary>
    /// Default policy assigned to website customers — covers sign-in, general access,
    /// commenting and purchasing. New self-registered members receive this policy.
    /// </summary>
    public const string Users = "Users";
}
