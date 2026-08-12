namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Transactional SMS (OTP, shipment tracking, etc.).
/// No gateway is shipped yet — runtime uses a no-op that only logs.
/// A site-scoped provider factory (same idea as storage backends) will select the active gateway
/// from website settings when real providers are added later.
/// </summary>
public interface ISmsSender
{
    /// <summary>True when a real SMS gateway is configured for the current send path. The no-op returns false.</summary>
    bool IsConfigured { get; }

    /// <summary>Sends <paramref name="message"/> to a phone number (country code + national number).</summary>
    Task SendAsync(string countryCode, string cellphone, string message, CancellationToken ct = default);
}
