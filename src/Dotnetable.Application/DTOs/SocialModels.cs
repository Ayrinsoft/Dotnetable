namespace Dotnetable.Application.DTOs;

/// <summary>Which networks have received this post at least once. Count and time are not shown.</summary>
public sealed class SocialSentFlags
{
    public bool Instagram { get; init; }
    public bool Telegram { get; init; }
    public bool X { get; init; }
}

/// <summary>What succeeded in one send. A network stays false when it was not selected or it failed.</summary>
public sealed class SocialPublishResult
{
    public bool Instagram { get; init; }
    public bool Telegram { get; init; }
    public bool X { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
}

public sealed class TelegramAccountStatus
{
    public bool Connected { get; init; }
    public string? ChatId { get; init; }
    public string? Username { get; init; }
    public bool HasToken { get; init; }
}

public sealed class TelegramAccountInput
{
    public string ChatId { get; set; } = "";
    public string? BotToken { get; set; }
}

public sealed class XAccountStatus
{
    public bool Connected { get; init; }
    public string? Username { get; init; }
    public bool HasKeys { get; init; }
}

public sealed class XAccountInput
{
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? AccessToken { get; set; }
    public string? AccessTokenSecret { get; set; }
}
