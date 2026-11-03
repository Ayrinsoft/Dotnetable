using System;

namespace Dotnetable.Domain.Entities;

/// <summary>The X user a website posts as. One row per website. All four keys are stored encrypted.</summary>
public partial class WebsiteXAccount
{
    public int WebsiteXAccountID { get; set; }

    public int WebsiteID { get; set; }

    public string ApiKey { get; set; } = null!;

    public string ApiSecret { get; set; } = null!;

    public string AccessToken { get; set; } = null!;

    public string AccessTokenSecret { get; set; } = null!;

    public string? Username { get; set; }

    public bool IsActive { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
}
