using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.DTOs;

/// <summary>One send to record. Scope values (source, recipient, sender) are filled in by the log service.</summary>
public sealed class MessageLogEntry
{
    public int WebsiteID { get; init; }
    public MessageChannel Channel { get; init; }
    public bool Success { get; init; }
    public string Recipient { get; init; } = "";
    public string? Subject { get; init; }
    public string? Body { get; init; }
    public string? Provider { get; init; }
    public string? Error { get; init; }
}

/// <summary>Filters for the message log page. Null means "any".</summary>
public sealed class MessageLogFilter
{
    /// <summary>Null = every website (master only).</summary>
    public int? WebsiteID { get; set; }
    public MessageChannel? Channel { get; set; }
    public MessageLogStatus? Status { get; set; }
    public string? Source { get; set; }

    /// <summary>Matches recipient address/number or name.</summary>
    public string? Recipient { get; set; }

    /// <summary>Matches subject or body.</summary>
    public string? Text { get; set; }

    /// <summary>Matches the admin who sent it.</summary>
    public string? SentBy { get; set; }

    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
}

/// <summary>A row on the message log page (body cut to a preview — the full one is fetched on demand).</summary>
public sealed class MessageLogListItem
{
    public long MessageLogID { get; init; }
    public int WebsiteID { get; init; }
    public string? WebsiteName { get; init; }
    public MessageChannel Channel { get; init; }
    public MessageLogStatus Status { get; init; }
    public string Recipient { get; init; } = "";
    public string? RecipientName { get; init; }
    public MessageRecipientType RecipientType { get; init; }
    public int? RecipientID { get; init; }
    public string? Subject { get; init; }
    public bool IsBodyRedacted { get; init; }
    public string? Provider { get; init; }
    public string Source { get; init; } = "";
    public string? Error { get; init; }
    public string? SentByName { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>Headline numbers shown above the log grid for the current filter.</summary>
public sealed class MessageLogSummary
{
    public int Total { get; init; }
    public int Sent { get; init; }
    public int Failed { get; init; }
    public IReadOnlyDictionary<MessageChannel, int> ByChannel { get; init; } = new Dictionary<MessageChannel, int>();
}
