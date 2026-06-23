using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

/// <summary>Reads/writes the default SMTP settings (the <c>EmailSetting</c> table) and sends mail.</summary>
public interface IEmailService
{
    /// <summary>The default email settings, or null when none have been configured yet.</summary>
    Task<EmailSettingsInfo?> GetDefaultAsync(CancellationToken ct = default);

    /// <summary>Creates or updates the single default email setting row.</summary>
    Task SaveDefaultAsync(EmailSettingsInfo settings, CancellationToken ct = default);

    /// <summary>Sends an email through the default SMTP settings. Throws when email is not configured.</summary>
    Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken ct = default);

    /// <summary>True when a usable default email setting exists.</summary>
    Task<bool> IsConfiguredAsync(CancellationToken ct = default);
}
