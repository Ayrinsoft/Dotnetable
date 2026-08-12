namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Sends transactional WhatsApp messages to website customers.
/// No real provider ships yet — <c>NoOpWhatsAppSender</c> only logs. Swap in Meta Cloud API,
/// Twilio, etc. later without touching order/auth flows.
/// </summary>
public interface IWhatsAppSender
{
    /// <summary>True when a real WhatsApp gateway is configured. The stub returns false.</summary>
    bool IsConfigured { get; }

    /// <summary>Sends <paramref name="message"/> to a phone number (country code + national number).</summary>
    Task SendAsync(string countryCode, string cellphone, string message, CancellationToken ct = default);
}
