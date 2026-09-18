namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Transactional WhatsApp messages (shipment tracking, admin alerts when used, etc.).
/// No gateway is shipped yet — runtime uses a no-op that only logs.
///
/// <para>Scoped to a website like <see cref="IEmailService"/>, and it follows the same fallback rule:
/// a website without its own WhatsApp gateway sends through the master website's
/// (<c>AppConstants.MasterWebsiteId</c>) — otherwise a site that never configured one would silently
/// drop every message to its owner. <see cref="ISmsSender"/> is deliberately the opposite: SMS stays
/// on the site's own gateway and never borrows another site's.</para>
///
/// <para>A site-scoped provider factory (same idea as storage backends and SMS gateways) will select
/// the active gateway from website settings when real providers are added; whatever lands must
/// implement the fallback above.</para>
/// </summary>
public interface IWhatsAppSender
{
    /// <summary>True when a real gateway is configured for the website (or, failing that, for the
    /// master website). The no-op returns false.</summary>
    Task<bool> IsConfiguredAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Sends <paramref name="message"/> to a phone number (country code + national number).
    /// Returns false when nothing was sent.</summary>
    Task<bool> SendAsync(int websiteId, string countryCode, string cellphone, string message, CancellationToken ct = default);
}
