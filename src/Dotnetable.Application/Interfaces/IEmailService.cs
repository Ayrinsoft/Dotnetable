using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

/// <summary>Sends mail for a website, picking the right <c>EmailAccount</c> and rendering <c>EmailTemplate</c>s.</summary>
public interface IEmailService
{
    /// <summary>True when the website (or the master website, as fallback) has a usable email account.</summary>
    Task<bool> IsConfiguredAsync(int websiteId, CancellationToken ct = default);

    /// <summary>
    /// Sends a raw email through the account resolved for (websiteId, accountType) — the website's own
    /// account of that type, else its default account, else the master website's equivalent.
    /// Throws when no usable account can be resolved.
    /// </summary>
    Task SendAsync(
        int websiteId, EmailAccountType accountType, string toAddress, string subject, string htmlBody,
        CancellationToken ct = default);

    /// <summary>
    /// Resolves and renders the template for <paramref name="templateKey"/> (own override or master
    /// default), substitutes <c>{{Token}}</c> placeholders from <paramref name="tokens"/> plus the
    /// sending website's own SiteName/SiteUrl, and sends it through the template's preferred account.
    /// Throws when the key is unknown or no usable account can be resolved.
    /// </summary>
    Task SendTemplateAsync(
        int websiteId, string templateKey, string toAddress, IDictionary<string, string>? tokens = null,
        CancellationToken ct = default);
}
