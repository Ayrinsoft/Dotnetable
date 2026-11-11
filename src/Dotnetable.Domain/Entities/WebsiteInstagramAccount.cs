using System;

namespace Dotnetable.Domain.Entities;

/// <summary>An Instagram professional account a website publishes to. A website may have several, for different languages or purposes.</summary>
public partial class WebsiteInstagramAccount
{
    public int WebsiteInstagramAccountID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>Name the admin gives this channel, such as the language or the purpose.</summary>
    public string Title { get; set; } = "";

    /// <summary>Content language this channel is for. Empty means any language.</summary>
    public string? LanguageCode { get; set; }

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
