namespace Dotnetable.Application.DTOs;

/// <summary>
/// Initial data collected by the first-run setup form: the master website plus its administrator member.
/// </summary>
public class SetupRequest
{
    // Database connection (tested, optionally created, then persisted)
    public DatabaseConnectionInfo Database { get; set; } = new();

    // Website
    public string TradeName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string WebsiteAddress { get; set; } = string.Empty;
    public string Manager { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string WebsiteEmail { get; set; } = string.Empty;
    public string DefaultLanguageCode { get; set; } = "en";

    // Administrator member
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Givenname { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;

    // Optional bot protection. When left empty the admin forms fall back to the math captcha.
    public string TurnstileSiteKey { get; set; } = string.Empty;
    public string TurnstileSecretKey { get; set; } = string.Empty;

    // Optional SMTP settings, enabling forgot-password email from first run. All optional.
    public string MailServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool MailEnableSSL { get; set; } = true;
    public string MailAddress { get; set; } = string.Empty;
    public string MailPassword { get; set; } = string.Empty;
    public string MailName { get; set; } = string.Empty;
}
