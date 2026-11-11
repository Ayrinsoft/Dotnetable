using System;

namespace Dotnetable.Domain.Entities;

/// <summary>An X user a website posts as. A website may have several, for different languages or purposes. All four keys are stored encrypted.</summary>
public partial class WebsiteXAccount
{
    public int WebsiteXAccountID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>Name the admin gives this channel, such as the language or the purpose.</summary>
    public string Title { get; set; } = "";

    /// <summary>Content language this channel is for. Empty means any language.</summary>
    public string? LanguageCode { get; set; }

    public string ApiKey { get; set; } = null!;

    public string ApiSecret { get; set; } = null!;

    public string AccessToken { get; set; } = null!;

    public string AccessTokenSecret { get; set; } = null!;

    public string? Username { get; set; }

    public bool IsActive { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
}
