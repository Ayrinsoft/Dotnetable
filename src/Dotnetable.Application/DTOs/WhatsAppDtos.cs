namespace Dotnetable.Application.DTOs;

/// <summary>Strongly-typed view of a <c>WebsiteWhatsAppSetting</c> row, handed to a provider at send time.</summary>
public sealed class WhatsAppSettingContext
{
    public int WebsiteWhatsAppSettingID { get; init; }
    public int WebsiteID { get; init; }

    /// <summary>Provider key, matching <c>IWhatsAppProvider.Key</c>.</summary>
    public string Provider { get; init; } = "";

    /// <summary>Raw provider credentials/options JSON (parsed by each provider).</summary>
    public string SettingsJson { get; init; } = "{}";

    /// <summary>Default sending number, when the provider settings do not carry one.</summary>
    public string? SenderNumber { get; init; }
}

/// <summary>A registered WhatsApp gateway as shown in the admin list.</summary>
public sealed class WhatsAppSettingInfo
{
    public int WebsiteWhatsAppSettingID { get; init; }
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

/// <summary>Create/update payload for a WhatsApp gateway registration.</summary>
public sealed class WhatsAppSettingInput
{
    public int WebsiteID { get; set; }
    public string Provider { get; set; } = "";
    public string Title { get; set; } = "";
    public string SettingsJSON { get; set; } = "{}";
    public string? SenderNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

// ── Per-provider settings, persisted in WebsiteWhatsAppSetting.SettingsJSON ─────────────

/// <summary>
/// Meta WhatsApp Cloud API (graph.facebook.com). Free-form text is only delivered inside the 24-hour
/// customer-service window; outside it Meta requires an approved template, so <see cref="TemplateName"/>
/// can be set to send every message as that template with the text as its single body parameter.
/// </summary>
public sealed class MetaCloudWhatsAppSettings
{
    /// <summary>Permanent (system user) access token.</summary>
    public string AccessToken { get; set; } = "";

    /// <summary>The Phone Number ID from WhatsApp Manager — not the phone number itself.</summary>
    public string PhoneNumberId { get; set; } = "";

    public string ApiVersion { get; set; } = "v21.0";

    /// <summary>Optional approved template with one <c>{{1}}</c> body variable.</summary>
    public string? TemplateName { get; set; }

    public string TemplateLanguage { get; set; } = "en_US";
}

/// <summary>Twilio WhatsApp — same account as Twilio SMS, sender is a WhatsApp-enabled number.</summary>
public sealed class TwilioWhatsAppSettings
{
    public string AccountSid { get; set; } = "";
    public string AuthToken { get; set; } = "";

    /// <summary>WhatsApp sender in E.164, e.g. +14155238886. The <c>whatsapp:</c> prefix is added automatically.</summary>
    public string From { get; set; } = "";
}

/// <summary>UltraMsg (ultramsg.com) — instance id + token.</summary>
public sealed class UltraMsgWhatsAppSettings
{
    public string InstanceId { get; set; } = "";
    public string Token { get; set; } = "";
}

/// <summary>Green API (green-api.com) — instance id + API token.</summary>
public sealed class GreenApiWhatsAppSettings
{
    public string InstanceId { get; set; } = "";
    public string ApiToken { get; set; } = "";

    /// <summary>API host; Green API assigns one per instance range.</summary>
    public string ApiUrl { get; set; } = "https://api.green-api.com";
}
