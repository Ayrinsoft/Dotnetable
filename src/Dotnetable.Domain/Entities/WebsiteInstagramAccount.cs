using System;

namespace Dotnetable.Domain.Entities;

/// <summary>The Instagram professional account a website publishes to. One row per website.</summary>
public partial class WebsiteInstagramAccount
{
    public int WebsiteInstagramAccountID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>Instagram professional account id (the Graph API user id, not the page id).</summary>
    public string IgUserId { get; set; } = null!;

    /// <summary>Long-lived Facebook Page access token. Stored encrypted when a protection key is configured.</summary>
    public string AccessToken { get; set; } = null!;

    public string? PageId { get; set; }

    public string? Username { get; set; }

    public bool IsActive { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
}
