namespace Dotnetable.Admin.Components.Pages.Messages;

/// <summary>How a gateway setting is edited in <see cref="GatewayDialog"/>.</summary>
public enum GatewayFieldKind
{
    Text,
    Secret,
    Multiline,
    /// <summary>GET / POST picker.</summary>
    Method,
    /// <summary>A JSON object of string → string, edited as "Name: value" lines.</summary>
    Headers,
}

/// <summary>One property of a provider's settings JSON (the name matches the settings class property).</summary>
public sealed record GatewayField(string Name, string LabelKey, string LabelDefault, GatewayFieldKind Kind = GatewayFieldKind.Text,
    bool Required = false, string? HintKey = null, string? HintDefault = null, string? DefaultValue = null);

/// <summary>
/// Which settings each SMS / WhatsApp provider takes, keyed by provider key. The providers themselves
/// (<c>Dotnetable.Infrastructure.Sms</c> / <c>.WhatsApp</c>) parse the same JSON; keep the names in
/// sync with their settings classes (<c>SmsDtos.cs</c>, <c>WhatsAppDtos.cs</c>). A provider missing
/// here still works — the dialog falls back to a raw JSON editor.
/// </summary>
public static class GatewayFieldCatalog
{
    private static readonly GatewayField[] GenericHttp =
    [
        new("Url", "gateway.f.url", "Request URL", Required: true,
            HintKey: "gateway.f.url_hint", HintDefault: "Placeholders: {to}, {from}, {text}, {countryCode} — URL-encoded automatically."),
        new("Method", "gateway.f.method", "HTTP method", GatewayFieldKind.Method, DefaultValue: "GET"),
        new("Body", "gateway.f.body", "Request body (POST)", GatewayFieldKind.Multiline,
            HintKey: "gateway.f.body_hint", HintDefault: "Sent as-is with the placeholders substituted."),
        new("ContentType", "gateway.f.content_type", "Body content type", DefaultValue: "application/x-www-form-urlencoded"),
        new("Headers", "gateway.f.headers", "Extra headers", GatewayFieldKind.Headers,
            HintKey: "gateway.f.headers_hint", HintDefault: "One per line, e.g. Authorization: Bearer abc"),
        new("From", "gateway.f.from", "Sender"),
        new("SuccessContains", "gateway.f.success_contains", "Success marker in response",
            HintKey: "gateway.f.success_contains_hint", HintDefault: "Optional text a 200 response must contain to count as sent."),
    ];

    public static readonly IReadOnlyDictionary<string, GatewayField[]> Sms = new Dictionary<string, GatewayField[]>(StringComparer.OrdinalIgnoreCase)
    {
        ["Kavenegar"] = [new("ApiKey", "gateway.f.api_key", "API key", GatewayFieldKind.Secret, true), new("Sender", "gateway.f.sender_line", "Sender line")],
        ["SmsIr"] = [new("ApiKey", "gateway.f.api_key", "API key", GatewayFieldKind.Secret, true), new("LineNumber", "gateway.f.line_number", "Line number", Required: true)],
        ["MelliPayamak"] =
        [
            new("Username", "gateway.f.username", "Username", Required: true),
            new("Password", "gateway.f.password", "Password", GatewayFieldKind.Secret, true),
            new("From", "gateway.f.sender_line", "Sender line", Required: true),
        ],
        ["Ghasedak"] = [new("ApiKey", "gateway.f.api_key", "API key", GatewayFieldKind.Secret, true), new("LineNumber", "gateway.f.line_number", "Line number")],
        ["IpPanel"] = [new("ApiKey", "gateway.f.api_key", "API key", GatewayFieldKind.Secret, true), new("From", "gateway.f.sender_line", "Sender line", Required: true)],
        ["Twilio"] =
        [
            new("AccountSid", "gateway.f.account_sid", "Account SID", Required: true),
            new("AuthToken", "gateway.f.auth_token", "Auth token", GatewayFieldKind.Secret, true),
            new("From", "gateway.f.from_number", "From number or Messaging Service SID",
                HintKey: "gateway.f.e164_hint", HintDefault: "E.164, e.g. +15551234567"),
        ],
        ["Vonage"] =
        [
            new("ApiKey", "gateway.f.api_key", "API key", Required: true),
            new("ApiSecret", "gateway.f.api_secret", "API secret", GatewayFieldKind.Secret, true),
            new("From", "gateway.f.from", "Sender"),
        ],
        ["GenericHttp"] = GenericHttp,
    };

    public static readonly IReadOnlyDictionary<string, GatewayField[]> WhatsApp = new Dictionary<string, GatewayField[]>(StringComparer.OrdinalIgnoreCase)
    {
        ["MetaCloud"] =
        [
            new("AccessToken", "gateway.f.access_token", "Access token", GatewayFieldKind.Secret, true,
                "gateway.f.meta_token_hint", "A permanent System User token with whatsapp_business_messaging permission."),
            new("PhoneNumberId", "gateway.f.phone_number_id", "Phone number ID", Required: true,
                HintKey: "gateway.f.phone_number_id_hint", HintDefault: "From WhatsApp Manager → API setup (not the phone number itself)."),
            new("ApiVersion", "gateway.f.api_version", "Graph API version", DefaultValue: "v21.0"),
            new("TemplateName", "gateway.f.template_name", "Template name (optional)",
                HintKey: "gateway.f.template_name_hint", HintDefault: "Meta only delivers free text within 24h of the customer's last message. Set an approved template with one {{1}} body variable to reach anyone at any time."),
            new("TemplateLanguage", "gateway.f.template_language", "Template language", DefaultValue: "en_US"),
        ],
        ["Twilio"] =
        [
            new("AccountSid", "gateway.f.account_sid", "Account SID", Required: true),
            new("AuthToken", "gateway.f.auth_token", "Auth token", GatewayFieldKind.Secret, true),
            new("From", "gateway.f.whatsapp_from", "WhatsApp sender number",
                HintKey: "gateway.f.e164_hint", HintDefault: "E.164, e.g. +15551234567"),
        ],
        ["UltraMsg"] =
        [
            new("InstanceId", "gateway.f.instance_id", "Instance ID", Required: true, HintDefault: "e.g. instance12345", HintKey: "gateway.f.instance_id_hint"),
            new("Token", "gateway.f.token", "Token", GatewayFieldKind.Secret, true),
        ],
        ["GreenApi"] =
        [
            new("InstanceId", "gateway.f.instance_id", "Instance ID", Required: true),
            new("ApiToken", "gateway.f.token", "Token", GatewayFieldKind.Secret, true),
            new("ApiUrl", "gateway.f.api_url", "API host", DefaultValue: "https://api.green-api.com"),
        ],
        ["GenericHttp"] = GenericHttp,
    };
}

/// <summary>Channel-neutral edit model for <see cref="GatewayDialog"/> (mapped to SMS / WhatsApp inputs by the list pages).</summary>
public sealed class GatewayEditModel
{
    public int Id { get; set; }
    public int WebsiteID { get; set; }
    public string Provider { get; set; } = "";
    public string Title { get; set; } = "";
    public string SettingsJSON { get; set; } = "{}";
    public string? SenderNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public GatewayEditModel Clone() => (GatewayEditModel)MemberwiseClone();
}
