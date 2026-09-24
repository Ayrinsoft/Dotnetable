using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;

namespace Dotnetable.Infrastructure.WhatsApp;

/// <summary>
/// Shared plumbing for the HTTP-based WhatsApp gateways — the same contract as
/// <c>HttpSmsProviderBase</c>: a transport failure becomes a failed <see cref="SmsSendResult"/>,
/// never an exception, so a gateway outage cannot take an order or notification down with it.
/// </summary>
public abstract class HttpWhatsAppProviderBase : IWhatsAppProvider
{
    private readonly IHttpClientFactory _httpClientFactory;

    protected HttpWhatsAppProviderBase(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public abstract string Key { get; }
    public abstract string DisplayName { get; }

    public abstract bool IsConfigured(WhatsAppSettingContext ctx);

    public async Task<SmsSendResult> SendAsync(WhatsAppSettingContext ctx, string countryCode, string cellphone,
        string message, CancellationToken ct = default)
    {
        if (!IsConfigured(ctx))
            return SmsSendResult.Fail($"{DisplayName} is missing required settings.");

        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(20);
            return await SendCoreAsync(client, ctx, countryCode, cellphone, message, ct);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return SmsSendResult.Fail($"{DisplayName} timed out.");
        }
        catch (HttpRequestException ex)
        {
            return SmsSendResult.Fail($"{DisplayName} could not be reached: {ex.Message}");
        }
        catch (Exception ex)
        {
            return SmsSendResult.Fail($"{DisplayName} failed: {ex.Message}");
        }
    }

    protected abstract Task<SmsSendResult> SendCoreAsync(HttpClient client, WhatsAppSettingContext ctx,
        string countryCode, string cellphone, string message, CancellationToken ct);

    protected static T Parse<T>(WhatsAppSettingContext ctx) where T : new()
    {
        if (string.IsNullOrWhiteSpace(ctx.SettingsJson)) return new T();
        try
        {
            return JsonSerializer.Deserialize<T>(ctx.SettingsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new T();
        }
        catch (JsonException)
        {
            return new T();
        }
    }

    /// <summary>International digits only (<c>&lt;cc&gt;&lt;national&gt;</c>, no +), which every WhatsApp API expects.</summary>
    protected static string InternationalDigits(string countryCode, string cellphone)
    {
        var digits = new string(cellphone.Where(char.IsDigit).ToArray()).TrimStart('0');
        var cc = new string((countryCode ?? "").Where(char.IsDigit).ToArray());

        if (cc.Length == 0 || digits.StartsWith(cc, StringComparison.Ordinal))
            return digits;
        return cc + digits;
    }

    protected static async Task<SmsSendResult> ReadResultAsync(HttpResponseMessage response, string providerName,
        string? successContains, CancellationToken ct)
    {
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            return SmsSendResult.Fail($"{providerName} returned {(int)response.StatusCode}: {Trim(body)}");

        if (!string.IsNullOrWhiteSpace(successContains) &&
            !body.Contains(successContains, StringComparison.OrdinalIgnoreCase))
            return SmsSendResult.Fail($"{providerName} rejected the message: {Trim(body)}");

        return SmsSendResult.Ok(Trim(body));
    }

    protected static string Trim(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "" : (value.Length <= 300 ? value.Trim() : value[..300].Trim());
}

/// <summary>
/// Meta's official WhatsApp Business Cloud API. Sends a plain text message, or — when a template name
/// is configured — the approved template with the text as its only body variable, which is what Meta
/// requires for business-initiated messages outside the 24-hour window.
/// </summary>
public sealed class MetaCloudWhatsAppProvider : HttpWhatsAppProviderBase
{
    public MetaCloudWhatsAppProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "MetaCloud";
    public override string DisplayName => "WhatsApp Cloud API (Meta)";

    public override bool IsConfigured(WhatsAppSettingContext ctx)
    {
        var s = Parse<MetaCloudWhatsAppSettings>(ctx);
        return !string.IsNullOrWhiteSpace(s.AccessToken) && !string.IsNullOrWhiteSpace(s.PhoneNumberId);
    }

    protected override async Task<SmsSendResult> SendCoreAsync(HttpClient client, WhatsAppSettingContext ctx,
        string countryCode, string cellphone, string message, CancellationToken ct)
    {
        var s = Parse<MetaCloudWhatsAppSettings>(ctx);
        var version = string.IsNullOrWhiteSpace(s.ApiVersion) ? "v21.0" : s.ApiVersion.Trim();
        var to = InternationalDigits(countryCode, cellphone);

        object payload = string.IsNullOrWhiteSpace(s.TemplateName)
            ? new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to,
                type = "text",
                text = new { preview_url = false, body = message },
            }
            : new
            {
                messaging_product = "whatsapp",
                to,
                type = "template",
                template = new
                {
                    name = s.TemplateName.Trim(),
                    language = new { code = string.IsNullOrWhiteSpace(s.TemplateLanguage) ? "en_US" : s.TemplateLanguage.Trim() },
                    components = new object[]
                    {
                        new { type = "body", parameters = new object[] { new { type = "text", text = message } } },
                    },
                },
            };

        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"https://graph.facebook.com/{Uri.EscapeDataString(version)}/{Uri.EscapeDataString(s.PhoneNumberId.Trim())}/messages")
        {
            Content = JsonContent.Create(payload),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", s.AccessToken.Trim());

        using var response = await client.SendAsync(request, ct);
        return await ReadResultAsync(response, DisplayName, "\"messages\"", ct);
    }
}

/// <summary>Twilio's WhatsApp channel — the SMS Messages endpoint with <c>whatsapp:</c>-prefixed numbers.</summary>
public sealed class TwilioWhatsAppProvider : HttpWhatsAppProviderBase
{
    public TwilioWhatsAppProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "Twilio";
    public override string DisplayName => "Twilio WhatsApp";

    public override bool IsConfigured(WhatsAppSettingContext ctx)
    {
        var s = Parse<TwilioWhatsAppSettings>(ctx);
        return !string.IsNullOrWhiteSpace(s.AccountSid) && !string.IsNullOrWhiteSpace(s.AuthToken) &&
               (!string.IsNullOrWhiteSpace(s.From) || !string.IsNullOrWhiteSpace(ctx.SenderNumber));
    }

    protected override async Task<SmsSendResult> SendCoreAsync(HttpClient client, WhatsAppSettingContext ctx,
        string countryCode, string cellphone, string message, CancellationToken ct)
    {
        var s = Parse<TwilioWhatsAppSettings>(ctx);
        var from = (!string.IsNullOrWhiteSpace(s.From) ? s.From : ctx.SenderNumber!).Trim();
        if (!from.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase))
            from = "whatsapp:" + (from.StartsWith('+') ? from : "+" + from);

        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"https://api.twilio.com/2010-04-01/Accounts/{Uri.EscapeDataString(s.AccountSid)}/Messages.json")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["From"] = from,
                ["To"] = "whatsapp:+" + InternationalDigits(countryCode, cellphone),
                ["Body"] = message,
            }),
        };
        var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{s.AccountSid}:{s.AuthToken}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);

        using var response = await client.SendAsync(request, ct);
        return await ReadResultAsync(response, DisplayName, null, ct);
    }
}

/// <summary>UltraMsg — a popular WhatsApp-Web based gateway that needs no Meta business verification.</summary>
public sealed class UltraMsgWhatsAppProvider : HttpWhatsAppProviderBase
{
    public UltraMsgWhatsAppProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "UltraMsg";
    public override string DisplayName => "UltraMsg";

    public override bool IsConfigured(WhatsAppSettingContext ctx)
    {
        var s = Parse<UltraMsgWhatsAppSettings>(ctx);
        return !string.IsNullOrWhiteSpace(s.InstanceId) && !string.IsNullOrWhiteSpace(s.Token);
    }

    protected override async Task<SmsSendResult> SendCoreAsync(HttpClient client, WhatsAppSettingContext ctx,
        string countryCode, string cellphone, string message, CancellationToken ct)
    {
        var s = Parse<UltraMsgWhatsAppSettings>(ctx);
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["token"] = s.Token.Trim(),
            ["to"] = "+" + InternationalDigits(countryCode, cellphone),
            ["body"] = message,
        });

        using var response = await client.PostAsync(
            $"https://api.ultramsg.com/{Uri.EscapeDataString(s.InstanceId.Trim())}/messages/chat", content, ct);
        // UltraMsg answers 200 with {"error": …} on a bad token or number.
        return await ReadResultAsync(response, DisplayName, "\"sent\"", ct);
    }
}

/// <summary>Green API — WhatsApp-Web based gateway, instance id + token.</summary>
public sealed class GreenApiWhatsAppProvider : HttpWhatsAppProviderBase
{
    public GreenApiWhatsAppProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "GreenApi";
    public override string DisplayName => "Green API";

    public override bool IsConfigured(WhatsAppSettingContext ctx)
    {
        var s = Parse<GreenApiWhatsAppSettings>(ctx);
        return !string.IsNullOrWhiteSpace(s.InstanceId) && !string.IsNullOrWhiteSpace(s.ApiToken);
    }

    protected override async Task<SmsSendResult> SendCoreAsync(HttpClient client, WhatsAppSettingContext ctx,
        string countryCode, string cellphone, string message, CancellationToken ct)
    {
        var s = Parse<GreenApiWhatsAppSettings>(ctx);
        var host = string.IsNullOrWhiteSpace(s.ApiUrl) ? "https://api.green-api.com" : s.ApiUrl.Trim().TrimEnd('/');
        var url = $"{host}/waInstance{Uri.EscapeDataString(s.InstanceId.Trim())}/sendMessage/{Uri.EscapeDataString(s.ApiToken.Trim())}";

        using var response = await client.PostAsJsonAsync(url, new
        {
            chatId = InternationalDigits(countryCode, cellphone) + "@c.us",
            message,
        }, ct);
        return await ReadResultAsync(response, DisplayName, "\"idMessage\"", ct);
    }
}

/// <summary>
/// Template-driven HTTP gateway, same settings shape as the SMS one (<see cref="GenericHttpSmsSettings"/>):
/// URL, method, body and headers with <c>{to}</c>, <c>{from}</c>, <c>{text}</c> and <c>{countryCode}</c>
/// placeholders. <c>{to}</c> is the international number without +. Covers any WhatsApp panel that
/// takes a plain GET/POST without a new build.
/// </summary>
public sealed class GenericHttpWhatsAppProvider : HttpWhatsAppProviderBase
{
    public GenericHttpWhatsAppProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "GenericHttp";
    public override string DisplayName => "Custom HTTP gateway";

    public override bool IsConfigured(WhatsAppSettingContext ctx) =>
        !string.IsNullOrWhiteSpace(Parse<GenericHttpSmsSettings>(ctx).Url);

    protected override async Task<SmsSendResult> SendCoreAsync(HttpClient client, WhatsAppSettingContext ctx,
        string countryCode, string cellphone, string message, CancellationToken ct)
    {
        var s = Parse<GenericHttpSmsSettings>(ctx);
        var from = !string.IsNullOrWhiteSpace(s.From) ? s.From : ctx.SenderNumber ?? "";
        var to = InternationalDigits(countryCode, cellphone);

        string Fill(string template, bool urlEncode)
        {
            string Enc(string v) => urlEncode ? Uri.EscapeDataString(v) : v;
            return template
                .Replace("{to}", Enc(to), StringComparison.Ordinal)
                .Replace("{from}", Enc(from), StringComparison.Ordinal)
                .Replace("{text}", Enc(message), StringComparison.Ordinal)
                .Replace("{countryCode}", Enc(countryCode ?? ""), StringComparison.Ordinal);
        }

        var method = string.Equals(s.Method, "POST", StringComparison.OrdinalIgnoreCase)
            ? HttpMethod.Post
            : HttpMethod.Get;

        using var request = new HttpRequestMessage(method, Fill(s.Url, urlEncode: true));

        if (method == HttpMethod.Post && !string.IsNullOrWhiteSpace(s.Body))
            request.Content = new StringContent(Fill(s.Body, urlEncode: false), Encoding.UTF8, s.ContentType);

        foreach (var (name, value) in s.Headers)
            request.Headers.TryAddWithoutValidation(name, Fill(value, urlEncode: false));

        using var response = await client.SendAsync(request, ct);
        return await ReadResultAsync(response, DisplayName, s.SuccessContains, ct);
    }
}
