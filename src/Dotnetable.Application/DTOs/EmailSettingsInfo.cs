namespace Dotnetable.Application.DTOs;

/// <summary>SMTP credentials used to send transactional email (e.g. forgot-password links).</summary>
public class EmailSettingsInfo
{
    public string MailServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool EnableSSL { get; set; } = true;

    /// <summary>From address / SMTP username.</summary>
    public string EmailAddress { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    /// <summary>Friendly "from" display name.</summary>
    public string MailName { get; set; } = string.Empty;

    /// <summary>True when enough fields are present to attempt sending mail.</summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(MailServer) && !string.IsNullOrWhiteSpace(EmailAddress);
}
