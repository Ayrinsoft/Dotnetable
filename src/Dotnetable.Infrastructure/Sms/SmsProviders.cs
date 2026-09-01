using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;

namespace Dotnetable.Infrastructure.Sms;

/// <summary>
/// Shared plumbing for the HTTP-based gateways: settings parsing, number normalisation and a single
/// place that turns a transport failure into a <see cref="SmsSendResult"/> instead of an exception.
/// A gateway being down must never take an OTP request down with it.
/// </summary>
public abstract class HttpSmsProviderBase : ISmsProvider
{
    private readonly IHttpClientFactory _httpClientFactory;

    protected HttpSmsProviderBase(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public abstract string Key { get; }
    public abstract string DisplayName { get; }
    public virtual bool IsIranian => true;

    public abstract bool IsConfigured(SmsSettingContext ctx);

    public async Task<SmsSendResult> SendAsync(SmsSettingContext ctx, string countryCode, string cellphone,
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

    protected abstract Task<SmsSendResult> SendCoreAsync(HttpClient client, SmsSettingContext ctx,
        string countryCode, string cellphone, string message, CancellationToken ct);

    protected static T Parse<T>(SmsSettingContext ctx) where T : new()
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

    /// <summary>
    /// Iranian panels want a local <c>09xxxxxxxxx</c> number; international ones want E.164. This
    /// produces the local form, stripping a leading country code or +.
    /// </summary>
    protected static string LocalNumber(string countryCode, string cellphone)
    {
        var digits = new string(cellphone.Where(char.IsDigit).ToArray());
        var cc = new string((countryCode ?? "").Where(char.IsDigit).ToArray());

        if (cc.Length > 0 && digits.StartsWith(cc, StringComparison.Ordinal) && digits.Length > cc.Length)
            digits = digits[cc.Length..];

        return digits.StartsWith('0') ? digits : "0" + digits;
    }

    /// <summary>E.164 form (<c>+&lt;cc&gt;&lt;national&gt;</c>) for the international gateways.</summary>
    protected static string E164(string countryCode, string cellphone)
    {
        var digits = new string(cellphone.Where(char.IsDigit).ToArray()).TrimStart('0');
        var cc = new string((countryCode ?? "").Where(char.IsDigit).ToArray());

        if (cc.Length == 0)
            return "+" + digits;

        return digits.StartsWith(cc, StringComparison.Ordinal) ? "+" + digits : "+" + cc + digits;
    }

    /// <summary>2xx plus, when the gateway answers 200 with an error payload, a body check.</summary>
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

/// <summary>Kavenegar — the most widely deployed Iranian transactional gateway.</summary>
public sealed class KavenegarSmsProvider : HttpSmsProviderBase
{
    public KavenegarSmsProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "Kavenegar";
    public override string DisplayName => "Kavenegar";

    public override bool IsConfigured(SmsSettingContext ctx) =>
        !string.IsNullOrWhiteSpace(Parse<KavenegarSmsSettings>(ctx).ApiKey);

    protected override async Task<SmsSendResult> SendCoreAsync(HttpClient client, SmsSettingContext ctx,
        string countryCode, string cellphone, string message, CancellationToken ct)
    {
        var s = Parse<KavenegarSmsSettings>(ctx);
        var sender = !string.IsNullOrWhiteSpace(s.Sender) ? s.Sender : ctx.SenderNumber ?? "";
        var to = LocalNumber(countryCode, cellphone);

        var url = $"https://api.kavenegar.com/v1/{Uri.EscapeDataString(s.ApiKey)}/sms/send.json" +
                  $"?receptor={Uri.EscapeDataString(to)}" +
                  $"&message={Uri.EscapeDataString(message)}" +
                  (string.IsNullOrWhiteSpace(sender) ? "" : $"&sender={Uri.EscapeDataString(sender)}");

        using var response = await client.GetAsync(url, ct);
        // Kavenegar always wraps the outcome in return.status; 200 is the only accepted code.
        return await ReadResultAsync(response, DisplayName, "\"status\":200", ct);
    }
}

/// <summary>SMS.ir — Iranian, v1 REST API.</summary>
public sealed class SmsIrProvider : HttpSmsProviderBase
{
    public SmsIrProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "SmsIr";
    public override string DisplayName => "SMS.ir";

    public override bool IsConfigured(SmsSettingContext ctx)
    {
        var s = Parse<SmsIrSettings>(ctx);
        return !string.IsNullOrWhiteSpace(s.ApiKey) &&
               (!string.IsNullOrWhiteSpace(s.LineNumber) || !string.IsNullOrWhiteSpace(ctx.SenderNumber));
    }

    protected override async Task<SmsSendResult> SendCoreAsync(HttpClient client, SmsSettingContext ctx,
        string countryCode, string cellphone, string message, CancellationToken ct)
    {
        var s = Parse<SmsIrSettings>(ctx);
        var line = !string.IsNullOrWhiteSpace(s.LineNumber) ? s.LineNumber : ctx.SenderNumber!;

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sms.ir/v1/send/bulk")
        {
            Content = JsonContent.Create(new
            {
                lineNumber = line,
                messageText = message,
                mobiles = new[] { LocalNumber(countryCode, cellphone) },
            }),
        };
        request.Headers.TryAddWithoutValidation("X-API-KEY", s.ApiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await client.SendAsync(request, ct);
        return await ReadResultAsync(response, DisplayName, "\"status\":1", ct);
    }
}

/// <summary>MelliPayamak / Farapayamak — Iranian, classic username + password endpoint.</summary>
public sealed class MelliPayamakSmsProvider : HttpSmsProviderBase
{
    public MelliPayamakSmsProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "MelliPayamak";
    public override string DisplayName => "MelliPayamak / Farapayamak";

    public override bool IsConfigured(SmsSettingContext ctx)
    {
        var s = Parse<MelliPayamakSettings>(ctx);
        return !string.IsNullOrWhiteSpace(s.Username) && !string.IsNullOrWhiteSpace(s.Password) &&
               (!string.IsNullOrWhiteSpace(s.From) || !string.IsNullOrWhiteSpace(ctx.SenderNumber));
    }

    protected override async Task<SmsSendResult> SendCoreAsync(HttpClient client, SmsSettingContext ctx,
        string countryCode, string cellphone, string message, CancellationToken ct)
    {
        var s = Parse<MelliPayamakSettings>(ctx);
        var from = !string.IsNullOrWhiteSpace(s.From) ? s.From : ctx.SenderNumber!;

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = s.Username,
            ["password"] = s.Password,
            ["to"] = LocalNumber(countryCode, cellphone),
            ["from"] = from,
            ["text"] = message,
            ["isflash"] = "false",
        });

        using var response = await client.PostAsync("https://rest.payamak-panel.com/api/SendSMS/SendSMS", content, ct);
        // RetStatus 1 means queued; anything else is an error code.
        return await ReadResultAsync(response, DisplayName, "\"RetStatus\":1", ct);
    }
}

/// <summary>Ghasedak — Iranian.</summary>
public sealed class GhasedakSmsProvider : HttpSmsProviderBase
{
    public GhasedakSmsProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "Ghasedak";
    public override string DisplayName => "Ghasedak";

    public override bool IsConfigured(SmsSettingContext ctx) =>
        !string.IsNullOrWhiteSpace(Parse<GhasedakSmsSettings>(ctx).ApiKey);

    protected override async Task<SmsSendResult> SendCoreAsync(HttpClient client, SmsSettingContext ctx,
        string countryCode, string cellphone, string message, CancellationToken ct)
    {
        var s = Parse<GhasedakSmsSettings>(ctx);
        var line = !string.IsNullOrWhiteSpace(s.LineNumber) ? s.LineNumber : ctx.SenderNumber ?? "";

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.ghasedak.me/v2/sms/send/simple")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["receptor"] = LocalNumber(countryCode, cellphone),
                ["message"] = message,
                ["linenumber"] = line,
            }),
        };
        request.Headers.TryAddWithoutValidation("apikey", s.ApiKey);

        using var response = await client.SendAsync(request, ct);
        return await ReadResultAsync(response, DisplayName, "\"code\":200", ct);
    }
}

/// <summary>IPPanel / FarazSMS — Iranian, bearer-key REST.</summary>
public sealed class IpPanelSmsProvider : HttpSmsProviderBase
{
    public IpPanelSmsProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "IpPanel";
    public override string DisplayName => "IPPanel / FarazSMS";

    public override bool IsConfigured(SmsSettingContext ctx)
    {
        var s = Parse<IpPanelSmsSettings>(ctx);
        return !string.IsNullOrWhiteSpace(s.ApiKey) &&
               (!string.IsNullOrWhiteSpace(s.From) || !string.IsNullOrWhiteSpace(ctx.SenderNumber));
    }

    protected override async Task<SmsSendResult> SendCoreAsync(HttpClient client, SmsSettingContext ctx,
        string countryCode, string cellphone, string message, CancellationToken ct)
    {
        var s = Parse<IpPanelSmsSettings>(ctx);
        var from = !string.IsNullOrWhiteSpace(s.From) ? s.From : ctx.SenderNumber!;

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api2.ippanel.com/api/v1/sms/send/webservice/single")
        {
            Content = JsonContent.Create(new
            {
                recipient = new[] { LocalNumber(countryCode, cellphone) },
                sender = from,
                message,
            }),
        };
        request.Headers.TryAddWithoutValidation("Authorization", s.ApiKey);

        using var response = await client.SendAsync(request, ct);
        return await ReadResultAsync(response, DisplayName, null, ct);
    }
}

/// <summary>Twilio — international.</summary>
public sealed class TwilioSmsProvider : HttpSmsProviderBase
{
    public TwilioSmsProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "Twilio";
    public override string DisplayName => "Twilio";
    public override bool IsIranian => false;

    public override bool IsConfigured(SmsSettingContext ctx)
    {
        var s = Parse<TwilioSmsSettings>(ctx);
        return !string.IsNullOrWhiteSpace(s.AccountSid) && !string.IsNullOrWhiteSpace(s.AuthToken) &&
               (!string.IsNullOrWhiteSpace(s.From) || !string.IsNullOrWhiteSpace(ctx.SenderNumber));
    }

    protected override async Task<SmsSendResult> SendCoreAsync(HttpClient client, SmsSettingContext ctx,
        string countryCode, string cellphone, string message, CancellationToken ct)
    {
        var s = Parse<TwilioSmsSettings>(ctx);
        var from = !string.IsNullOrWhiteSpace(s.From) ? s.From : ctx.SenderNumber!;

        var form = new Dictionary<string, string>
        {
            ["To"] = E164(countryCode, cellphone),
            ["Body"] = message,
        };
        // A Messaging Service SID goes in a different field than a plain sending number.
        if (from.StartsWith("MG", StringComparison.Ordinal)) form["MessagingServiceSid"] = from;
        else form["From"] = from;

        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"https://api.twilio.com/2010-04-01/Accounts/{Uri.EscapeDataString(s.AccountSid)}/Messages.json")
        {
            Content = new FormUrlEncodedContent(form),
        };
        var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{s.AccountSid}:{s.AuthToken}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);

        using var response = await client.SendAsync(request, ct);
        return await ReadResultAsync(response, DisplayName, null, ct);
    }
}

/// <summary>Vonage (Nexmo) — international.</summary>
public sealed class VonageSmsProvider : HttpSmsProviderBase
{
    public VonageSmsProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "Vonage";
    public override string DisplayName => "Vonage (Nexmo)";
    public override bool IsIranian => false;

    public override bool IsConfigured(SmsSettingContext ctx)
    {
        var s = Parse<VonageSmsSettings>(ctx);
        return !string.IsNullOrWhiteSpace(s.ApiKey) && !string.IsNullOrWhiteSpace(s.ApiSecret) &&
               (!string.IsNullOrWhiteSpace(s.From) || !string.IsNullOrWhiteSpace(ctx.SenderNumber));
    }

    protected override async Task<SmsSendResult> SendCoreAsync(HttpClient client, SmsSettingContext ctx,
        string countryCode, string cellphone, string message, CancellationToken ct)
    {
        var s = Parse<VonageSmsSettings>(ctx);
        var from = !string.IsNullOrWhiteSpace(s.From) ? s.From : ctx.SenderNumber!;

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["api_key"] = s.ApiKey,
            ["api_secret"] = s.ApiSecret,
            ["to"] = E164(countryCode, cellphone).TrimStart('+'),
            ["from"] = from,
            ["text"] = message,
        });

        using var response = await client.PostAsync("https://rest.nexmo.com/sms/json", content, ct);
        return await ReadResultAsync(response, DisplayName, "\"status\":\"0\"", ct);
    }
}

/// <summary>
/// Template-driven HTTP gateway. Every Iranian panel not given its own class above — and any future
/// one — can be wired up here from the admin UI alone, because the URL, method, body and headers are
/// all configuration. This is what makes "support every gateway" a settings problem rather than a
/// release problem.
/// </summary>
public sealed class GenericHttpSmsProvider : HttpSmsProviderBase
{
    public GenericHttpSmsProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "GenericHttp";
    public override string DisplayName => "Custom HTTP gateway";
    public override bool IsIranian => false;

    public override bool IsConfigured(SmsSettingContext ctx) =>
        !string.IsNullOrWhiteSpace(Parse<GenericHttpSmsSettings>(ctx).Url);

    protected override async Task<SmsSendResult> SendCoreAsync(HttpClient client, SmsSettingContext ctx,
        string countryCode, string cellphone, string message, CancellationToken ct)
    {
        var s = Parse<GenericHttpSmsSettings>(ctx);
        var from = !string.IsNullOrWhiteSpace(s.From) ? s.From : ctx.SenderNumber ?? "";
        var to = LocalNumber(countryCode, cellphone);

        // Placeholders inside the URL must be percent-encoded; inside a body they must not be, or a
        // JSON template would end up with escaped values.
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
