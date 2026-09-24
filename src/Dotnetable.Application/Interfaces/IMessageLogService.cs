using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

/// <summary>Records and queries every outgoing message (<c>MessageLogs</c>).</summary>
public interface IMessageLogService
{
    /// <summary>
    /// Records one send, merged with the ambient <see cref="Messaging.MessageLogScope"/>. Never throws:
    /// a logging failure must not turn a delivered message into a reported error.
    /// </summary>
    Task WriteAsync(MessageLogEntry entry, CancellationToken ct = default);

    Task<PagedResult<MessageLogListItem>> GetPagedAsync(MessageLogFilter filter, GridQuery query, CancellationToken ct = default);

    Task<MessageLogSummary> GetSummaryAsync(MessageLogFilter filter, CancellationToken ct = default);

    /// <summary>The full row including the body. Null when missing or outside <paramref name="websiteId"/> (null = any).</summary>
    Task<MessageLog?> GetByIdAsync(long id, int? websiteId, CancellationToken ct = default);

    /// <summary>Deletes rows older than <paramref name="beforeUtc"/> (for one website, or all when null). Returns the number removed.</summary>
    Task<int> PurgeAsync(int? websiteId, DateTime beforeUtc, CancellationToken ct = default);
}

/// <summary>Admin-side "send a message to anyone" over every channel.</summary>
public interface IMessageComposerService
{
    /// <summary>Members and customers of the website matching <paramref name="term"/> (name, email, mobile).</summary>
    Task<IReadOnlyList<MessageRecipient>> SearchRecipientsAsync(int websiteId, string? term, int take = 20, CancellationToken ct = default);

    /// <summary>How many recipients an audience expands to, for the confirmation prompt.</summary>
    Task<int> CountAudienceAsync(int websiteId, MessageAudience audience, CancellationToken ct = default);

    /// <summary>Which channels can actually send for the website right now.</summary>
    Task<IReadOnlyDictionary<MessageChannel, bool>> GetChannelAvailabilityAsync(int websiteId, CancellationToken ct = default);

    Task<ComposeMessageResult> SendAsync(ComposeMessageRequest request, IProgress<ComposeProgress>? progress = null, CancellationToken ct = default);
}
