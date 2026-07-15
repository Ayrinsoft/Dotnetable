using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Reads/writes email templates. Every known key (<c>EmailTemplateKeys</c>) always resolves to a
/// template — the website's own row if it customized one, otherwise the master website's default.
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// One row per known template key, each resolved for the website (own override or master default),
    /// with <see cref="EmailTemplateInfo.IsOverride"/> indicating which.
    /// </summary>
    Task<List<EmailTemplateInfo>> GetForWebsiteAsync(int websiteId, CancellationToken ct = default);

    /// <summary>The resolved template for one key, or null if the key is unknown.</summary>
    Task<EmailTemplateInfo?> GetAsync(int websiteId, string templateKey, CancellationToken ct = default);

    /// <summary>Creates or updates the website's own override row for template.TemplateKey.</summary>
    Task SaveAsync(int websiteId, EmailTemplateInfo template, CancellationToken ct = default);

    /// <summary>Deletes the website's override row, reverting it to the master default.</summary>
    Task ResetToDefaultAsync(int websiteId, string templateKey, CancellationToken ct = default);
}
