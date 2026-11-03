using System;

namespace Dotnetable.Domain.Entities;

/// <summary>The Telegram bot a website publishes through. One row per website.</summary>
public partial class WebsiteTelegramAccount
{
    public int WebsiteTelegramAccountID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>Bot token from BotFather. Stored encrypted when a protection key is configured.</summary>
    public string BotToken { get; set; } = null!;

    /// <summary>Destination chat: @channel or a numeric id.</summary>
    public string ChatId { get; set; } = null!;

    public string? Username { get; set; }

    public bool IsActive { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
}
