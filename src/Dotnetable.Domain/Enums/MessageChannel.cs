namespace Dotnetable.Domain.Enums;

/// <summary>How a message is delivered. Stored in <c>MessageLog.Channel</c>.</summary>
public enum MessageChannel : byte
{
    Email = 1,
    Sms = 2,
    WhatsApp = 3,
    /// <summary>In-app notification in the admin panel's inbox (staff members only).</summary>
    InApp = 4,
}

/// <summary>Outcome of one send, stored in <c>MessageLog.Status</c>.</summary>
public enum MessageLogStatus : byte
{
    Sent = 1,
    Failed = 2,
}

/// <summary>Who a logged message was addressed to, stored in <c>MessageLog.RecipientType</c>.</summary>
public enum MessageRecipientType : byte
{
    /// <summary>A bare address/number with no matching record.</summary>
    Other = 0,
    Member = 1,
    Client = 2,
}
