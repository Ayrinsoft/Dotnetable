using System;

namespace Dotnetable.Domain.Entities;

/// <summary>A Telegram bot a website publishes through. A website may have several, for different languages or purposes.</summary>
public partial class WebsiteTelegramAccount
{
    public int WebsiteTelegramAccountID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>Name the admin gives this channel, such as the language or the purpose.</summary>
    public string Title { get; set; } = "";

    /// <summary>Content language this channel is for. Empty means any language.</summary>
    public string? LanguageCode { get; set; }

    /// <summary>Bot token from BotFather. Stored encrypted when a protection key is configured.</summary>
    public string BotToken { get; set; } = null!;

    /// <summary>Destination chat: @channel or a numeric id.</summary>
    public string ChatId { get; set; } = null!;

    public string? Username { get; set; }

    public bool IsActive { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
}
