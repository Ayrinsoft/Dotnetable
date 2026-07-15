using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.DTOs;

/// <summary>One SMTP-configured sending identity (an <c>EmailAccount</c> row).</summary>
public class EmailAccountInfo
{
    public int EmailAccountID { get; set; }
    public int WebsiteID { get; set; }
    public EmailAccountType AccountType { get; set; } = EmailAccountType.NoReply;

    /// <summary>Admin-facing label; required (and freely editable) when AccountType is Custom.</summary>
    public string Name { get; set; } = string.Empty;

    public string MailServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool EnableSSL { get; set; } = true;

    /// <summary>From address / SMTP username.</summary>
    public string EmailAddress { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    /// <summary>Friendly "from" display name.</summary>
    public string MailName { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
    public bool Active { get; set; } = true;

    /// <summary>True when enough fields are present to attempt sending mail.</summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(MailServer) && !string.IsNullOrWhiteSpace(EmailAddress);
}
