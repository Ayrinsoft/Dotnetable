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

    /// <summary>
    /// The resolved template for one key, or null if the key is unknown.
    /// When <paramref name="languageCode"/> is a non-default language with a saved translation,
    /// <see cref="EmailTemplateInfo.Subject"/> and <see cref="EmailTemplateInfo.HtmlBody"/> are
    /// replaced with that translation (falling back to the default-language content when blank).
    /// </summary>
    Task<EmailTemplateInfo?> GetAsync(int websiteId, string templateKey, string? languageCode = null, CancellationToken ct = default);

    /// <summary>Creates or updates the website's own override row for template.TemplateKey.</summary>
    Task SaveAsync(int websiteId, EmailTemplateInfo template, CancellationToken ct = default);

    /// <summary>
    /// Replaces non-default language translations for the website's override of
    /// <paramref name="templateKey"/>. Blank subject entries remove that language. Creates the
    /// website override from the resolved default when the site has not customized yet.
    /// </summary>
    Task SetTranslationsAsync(
        int websiteId,
        string templateKey,
        IReadOnlyDictionary<string, (string Subject, string HtmlBody)> byLanguage,
        CancellationToken ct = default);

    /// <summary>Deletes the website's override row, reverting it to the master default.</summary>
    Task ResetToDefaultAsync(int websiteId, string templateKey, CancellationToken ct = default);
}
