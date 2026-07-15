namespace Dotnetable.Domain.Enums;

/// <summary>
/// Well-known purpose of an <see cref="Entities.EmailAccount"/>. Kept small and closed so email
/// templates can reference a type (rather than a free-text name) to pick their sender.
/// </summary>
public enum EmailAccountType : byte
{
    /// <summary>Transactional/system mail (OTP, activation, password reset). Never a reply-to.</summary>
    NoReply = 0,

    /// <summary>Customer support / ticket replies.</summary>
    Support = 1,

    /// <summary>Orders, invoices, purchase-related mail.</summary>
    Sales = 2,

    /// <summary>Newsletters and marketing campaigns.</summary>
    Marketing = 3,

    /// <summary>General inquiries / contact-form notifications.</summary>
    Info = 4,

    /// <summary>Anything else — <see cref="Entities.EmailAccount.Name"/> is the only label.</summary>
    Custom = 5,
}
