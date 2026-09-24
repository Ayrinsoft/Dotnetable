using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.DTOs;

/// <summary>A person the composer can address: an admin member, a customer, or a typed-in address.</summary>
public sealed class MessageRecipient
{
    public MessageRecipientType Type { get; init; }

    /// <summary>MemberID or WebsiteClientID; null for a typed-in address.</summary>
    public int? Id { get; init; }

    public string DisplayName { get; init; } = "";
    public string? Email { get; init; }
    public string? CountryCode { get; init; }
    public string? Cellphone { get; init; }

    /// <summary>Stable key for de-duplication in the UI.</summary>
    public string Key => Id is int id ? $"{(byte)Type}:{id}" : $"x:{Email}|{CountryCode}|{Cellphone}";

    public override string ToString() => DisplayName;
}

/// <summary>Everyone at once, instead of (or on top of) hand-picked recipients.</summary>
public enum MessageAudience : byte
{
    None = 0,
    /// <summary>Every active admin-panel member of the website.</summary>
    AllMembers = 1,
    /// <summary>Every active customer of the website.</summary>
    AllClients = 2,
}

public sealed class ComposeMessageRequest
{
    public int WebsiteID { get; set; }
    public MessageChannel Channel { get; set; } = MessageChannel.Email;

    /// <summary>Email subject / in-app notification title. Ignored by SMS and WhatsApp.</summary>
    public string? Subject { get; set; }

    /// <summary>Plain text. <c>{name}</c> is replaced with each recipient's name.</summary>
    public string Body { get; set; } = "";

    /// <summary>Email only: send <see cref="Body"/> as HTML instead of escaping it.</summary>
    public bool BodyIsHtml { get; set; }

    /// <summary>Email only: which of the site's SMTP accounts sends.</summary>
    public EmailAccountType EmailAccountType { get; set; } = EmailAccountType.Info;

    /// <summary>In-app only: optional admin-panel link the notification opens.</summary>
    public string? ActionUrl { get; set; }

    public List<MessageRecipient> Recipients { get; set; } = new();
    public MessageAudience Audience { get; set; }

    public int SentByMemberID { get; set; }
    public string? SentByName { get; set; }
}

public sealed class ComposeMessageResult
{
    public int Sent { get; set; }
    public int Failed { get; set; }

    /// <summary>Recipients with no address for the chosen channel (no email / no mobile / not staff).</summary>
    public int Skipped { get; set; }

    /// <summary>Distinct error messages, capped, for the result summary.</summary>
    public List<string> Errors { get; } = new();
}

/// <summary>Progress callback payload while a batch is being sent.</summary>
public readonly record struct ComposeProgress(int Done, int Total);
