namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Sends transactional SMS (activation / password-reset codes) to website customers.
/// No real provider ships yet — <c>NoOpSmsSender</c> only logs the message. Swap in a real
/// implementation (Kavenegar, Twilio, …) later without touching the auth flow.
/// </summary>
public interface ISmsSender
{
    /// <summary>True when a real SMS gateway is configured. The stub returns false.</summary>
    bool IsConfigured { get; }

    /// <summary>Sends <paramref name="message"/> to a phone number (country code + national number).</summary>
    Task SendAsync(string countryCode, string cellphone, string message, CancellationToken ct = default);
}
