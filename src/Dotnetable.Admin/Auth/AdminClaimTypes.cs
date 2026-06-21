namespace Dotnetable.Admin.Auth;

public static class AdminClaimTypes
{
    public const string MemberId = "mid";
    public const string WebsiteId = "wid";
    public const string PolicyId = "pid";

    /// <summary>"true" for members of the master website (WebsiteID 1), who have full cross-site access.</summary>
    public const string Master = "master";
}
