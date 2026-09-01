namespace Dotnetable.Application.DTOs;

/// <summary>Strongly-typed view of a <c>WebsiteSmsSetting</c> row, handed to a provider at send time.</summary>
public sealed class SmsSettingContext
{
    public int WebsiteSmsSettingID { get; init; }
    public int WebsiteID { get; init; }

    /// <summary>Provider key, matching <c>ISmsProvider.Key</c>.</summary>
    public string Provider { get; init; } = "";

    /// <summary>Raw provider credentials/options JSON (parsed by each provider).</summary>
    public string SettingsJson { get; init; } = "{}";

    /// <summary>Default sender line, when the provider settings do not carry one.</summary>
    public string? SenderNumber { get; init; }
}

/// <summary>Outcome of one send attempt.</summary>
/// <param name="Success">True when the gateway accepted the message for delivery.</param>
/// <param name="MessageId">Gateway-side identifier, when the response carries one.</param>
/// <param name="Error">Why the send failed. Null on success. Safe to show an admin, never a customer.</param>
public sealed record SmsSendResult(bool Success, string? MessageId = null, string? Error = null)
{
    public static SmsSendResult Ok(string? messageId = null) => new(true, messageId);
    public static SmsSendResult Fail(string error) => new(false, null, error);
}

/// <summary>A registered gateway as shown in the admin list.</summary>
public sealed class SmsSettingInfo
{
    public int WebsiteSmsSettingID { get; init; }
    public int WebsiteID { get; init; }
    public string Provider { get; init; } = "";
    public string ProviderDisplayName { get; init; } = "";
    public string Title { get; init; } = "";
    public string? SenderNumber { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }

    /// <summary>False when the stored JSON is missing a field the gateway cannot send without.</summary>
    public bool IsConfigured { get; init; }

    public DateTime CreatedAt { get; init; }
}

/// <summary>Create/update payload for a gateway registration.</summary>
public sealed class SmsSettingInput
{
    public int WebsiteID { get; set; }
    public string Provider { get; set; } = "";
    public string Title { get; set; } = "";
    public string SettingsJSON { get; set; } = "{}";
    public string? SenderNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

// ── Per-provider settings, persisted in WebsiteSmsSetting.SettingsJSON ──────────────────

/// <summary>Kavenegar (kavenegar.com) — Iranian. Uses the plain <c>sms/send.json</c> REST call.</summary>
public sealed class KavenegarSmsSettings
{
    public string ApiKey { get; set; } = "";

    /// <summary>Sender line. Optional — Kavenegar falls back to the account default when blank.</summary>
    public string Sender { get; set; } = "";
}

/// <summary>SMS.ir — Iranian. v1 REST API with an <c>X-API-KEY</c> header.</summary>
public sealed class SmsIrSettings
{
    public string ApiKey { get; set; } = "";
    public string LineNumber { get; set; } = "";
}

/// <summary>MelliPayamak / Farapayamak — Iranian. Classic username + password REST endpoint.</summary>
public sealed class MelliPayamakSettings
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string From { get; set; } = "";
}

/// <summary>Ghasedak (ghasedak.me) — Iranian. <c>apikey</c> header + form body.</summary>
public sealed class GhasedakSmsSettings
{
    public string ApiKey { get; set; } = "";
    public string LineNumber { get; set; } = "";
}

/// <summary>IPPanel / FarazSMS — Iranian. Bearer-key REST API.</summary>
public sealed class IpPanelSmsSettings
{
    public string ApiKey { get; set; } = "";
    public string From { get; set; } = "";
}

/// <summary>Twilio — international. Account SID + auth token, HTTP basic.</summary>
public sealed class TwilioSmsSettings
{
    public string AccountSid { get; set; } = "";
    public string AuthToken { get; set; } = "";

    /// <summary>Sending number in E.164, e.g. +15551234567. Or a Messaging Service SID.</summary>
    public string From { get; set; } = "";
}

/// <summary>Vonage (formerly Nexmo) — international.</summary>
public sealed class VonageSmsSettings
{
    public string ApiKey { get; set; } = "";
    public string ApiSecret { get; set; } = "";
    public string From { get; set; } = "";
}

/// <summary>
/// Template-driven HTTP gateway: covers any panel that accepts a plain GET/POST, without writing a
/// new provider class. <see cref="Url"/>, <see cref="Body"/> and <see cref="Headers"/> may contain
/// the placeholders <c>{to}</c>, <c>{from}</c>, <c>{text}</c> and <c>{countryCode}</c>, which are
/// substituted (and URL-encoded inside <see cref="Url"/>) at send time.
/// </summary>
public sealed class GenericHttpSmsSettings
{
    public string Url { get; set; } = "";

    /// <summary>GET or POST. Defaults to GET, which is what most legacy Iranian panels expose.</summary>
    public string Method { get; set; } = "GET";

    /// <summary>Request body for POST. Sent as-is under <see cref="ContentType"/>.</summary>
    public string? Body { get; set; }

    public string ContentType { get; set; } = "application/x-www-form-urlencoded";

    /// <summary>Extra request headers, e.g. an API key header.</summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    public string From { get; set; } = "";

    /// <summary>
    /// Optional substring that must appear in a 2xx response body for the send to count as accepted.
    /// Many panels answer HTTP 200 with an error code in the body.
    /// </summary>
    public string? SuccessContains { get; set; }
}
