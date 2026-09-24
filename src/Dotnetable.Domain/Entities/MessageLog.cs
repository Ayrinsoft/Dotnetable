using System;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// One outgoing message (email, SMS, WhatsApp or in-app notification) and its outcome. Written by the
/// senders themselves, so automatic mail (orders, notifications, one-time codes) is recorded exactly
/// like a message an admin composed by hand. Bodies that carry secrets are stored redacted.
/// </summary>
public partial class MessageLog
{
    public long MessageLogID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary><c>MessageChannel</c>.</summary>
    public byte Channel { get; set; }

    /// <summary><c>MessageLogStatus</c>.</summary>
    public byte Status { get; set; }

    /// <summary>Email address or phone number (with country code) the message went to.</summary>
    public string Recipient { get; set; } = null!;

    public string? RecipientName { get; set; }

    /// <summary><c>MessageRecipientType</c>: who <see cref="RecipientID"/> points at.</summary>
    public byte RecipientType { get; set; }

    /// <summary>MemberID or WebsiteClientID, depending on <see cref="RecipientType"/>.</summary>
    public int? RecipientID { get; set; }

    public string? Subject { get; set; }

    public string? Body { get; set; }

    /// <summary>True when the body was withheld because it carried a one-time code or reset link.</summary>
    public bool IsBodyRedacted { get; set; }

    /// <summary>Gateway / account that handled the message, e.g. <c>Kavenegar</c> or an SMTP address.</summary>
    public string? Provider { get; set; }

    /// <summary>What triggered the send: <c>Manual</c>, <c>Otp</c>, <c>Order</c>, <c>Notification</c>, <c>System</c>…</summary>
    public string Source { get; set; } = null!;

    public string? Error { get; set; }

    /// <summary>The admin who sent it by hand; null for automatic messages.</summary>
    public int? SentByMemberID { get; set; }

    public string? SentByName { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
}
