namespace Dotnetable.Domain.Entities;

/// <summary>
/// A stable public code for one address. The public site asks the admin for <see cref="TargetUrl"/>
/// and redirects. Editing the target does not change <see cref="Code"/>.
/// </summary>
public class ShortLink
{
    public int ShortLinkID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>Public token, for example <c>LK2873H32</c>. Unique per website.</summary>
    public string Code { get; set; } = "";

    /// <summary>Absolute http(s) address the visitor should reach.</summary>
    public string TargetUrl { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
}
