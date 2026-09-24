using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Messaging;

/// <summary>
/// Ambient context for the message log, flowing through <c>async</c> calls the way
/// <c>AmbientDbContext</c> does. The senders (<c>IEmailService</c>, <c>ISmsSender</c>,
/// <c>IWhatsAppSender</c>) write one <c>MessageLog</c> row per send and read this scope to learn
/// what triggered it, who it was for, and whether the body may be stored.
///
/// <para>Callers that send secrets — one-time codes, password-reset links — <b>must</b> open a scope
/// with <c>redactBody: true</c>, or the code lands in a table admins can read.</para>
///
/// <code>
/// using (MessageLogScope.Begin(MessageLogSources.Otp, redactBody: true))
///     await _sms.SendAsync(...);
/// </code>
///
/// <para><see cref="Begin"/> is deliberately synchronous: an <see cref="AsyncLocal{T}"/> set inside
/// an <c>async</c> method would not be visible to the caller once it returns.</para>
/// </summary>
public sealed class MessageLogScope : IDisposable
{
    private static readonly AsyncLocal<MessageLogScope?> _current = new();

    private readonly MessageLogScope? _previous;
    private bool _disposed;

    private MessageLogScope(MessageLogScope? previous) => _previous = previous;

    /// <summary>The innermost open scope, or null when the send is not wrapped in one.</summary>
    public static MessageLogScope? Current => _current.Value;

    /// <summary>One of <see cref="MessageLogSources"/>.</summary>
    public string Source { get; private init; } = MessageLogSources.System;

    /// <summary>True when the body carries a secret and must not be stored.</summary>
    public bool RedactBody { get; private init; }

    public MessageRecipientType RecipientType { get; private init; }
    public int? RecipientID { get; private init; }
    public string? RecipientName { get; private init; }

    public int? SentByMemberID { get; private init; }
    public string? SentByName { get; private init; }

    /// <summary>
    /// Opens a scope. Values not given are inherited from the enclosing scope, so an inner
    /// <c>Begin(recipientType: …)</c> keeps an outer <c>redactBody: true</c>.
    /// </summary>
    public static MessageLogScope Begin(
        string? source = null,
        bool? redactBody = null,
        MessageRecipientType? recipientType = null,
        int? recipientId = null,
        string? recipientName = null,
        int? sentByMemberId = null,
        string? sentByName = null)
    {
        var outer = _current.Value;
        var scope = new MessageLogScope(outer)
        {
            Source = source ?? outer?.Source ?? MessageLogSources.System,
            // Redaction only ever tightens: an inner scope cannot un-redact an outer one.
            RedactBody = (redactBody ?? false) || (outer?.RedactBody ?? false),
            RecipientType = recipientType ?? outer?.RecipientType ?? MessageRecipientType.Other,
            RecipientID = recipientType is null ? outer?.RecipientID : recipientId,
            RecipientName = recipientType is null ? outer?.RecipientName : recipientName,
            SentByMemberID = sentByMemberId ?? outer?.SentByMemberID,
            SentByName = sentByName ?? outer?.SentByName,
        };
        _current.Value = scope;
        return scope;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _current.Value = _previous;
    }
}

/// <summary>Values for <c>MessageLog.Source</c>.</summary>
public static class MessageLogSources
{
    /// <summary>Composed by an admin on Messages → Send message.</summary>
    public const string Manual = "Manual";

    /// <summary>One-time codes and password-reset links. Always redacted.</summary>
    public const string Otp = "Otp";

    /// <summary>Order / shipment notices to the customer.</summary>
    public const string Order = "Order";

    /// <summary>Admin notifications fanned out to email / WhatsApp.</summary>
    public const string Notification = "Notification";

    /// <summary>A gateway test from the gateway settings pages.</summary>
    public const string Test = "Test";

    /// <summary>Anything sent outside a scope.</summary>
    public const string System = "System";

    public static readonly IReadOnlyList<string> All = [Manual, Otp, Order, Notification, Test, System];
}
