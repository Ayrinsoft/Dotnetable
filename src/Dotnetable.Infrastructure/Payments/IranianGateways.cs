using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;

namespace Dotnetable.Infrastructure.Payments;

/// <summary>Zarinpal — the most widely used Iranian aggregator. Payment Gateway v4 REST API.</summary>
public sealed class ZarinpalGatewayProvider : PaymentGatewayProviderBase
{
    public ZarinpalGatewayProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "Zarinpal";
    public override string DisplayName => "ZarinPal";
    public override GatewayAmountUnit AmountUnit => GatewayAmountUnit.Rial;

    public override bool IsConfigured(PaymentGatewayContext ctx) => !string.IsNullOrWhiteSpace(MerchantId(ctx));

    private static string MerchantId(PaymentGatewayContext ctx)
    {
        var fromJson = Parse<ZarinpalGatewaySettings>(ctx).MerchantId;
        return !string.IsNullOrWhiteSpace(fromJson) ? fromJson : ctx.MerchantId ?? "";
    }

    private string ApiBase(PaymentGatewayContext ctx) =>
        ctx.IsSandbox ? "https://sandbox.zarinpal.com" : "https://payment.zarinpal.com";

    protected override async Task<GatewayInitiateResult> InitiateCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayPaymentRequest request, CancellationToken ct)
    {
        using var response = await client.PostAsJsonAsync($"{ApiBase(ctx)}/pg/v4/payment/request.json", new
        {
            merchant_id = MerchantId(ctx),
            amount = ToGatewayAmount(request.Amount),
            callback_url = request.ReturnUrl,
            description = request.Description ?? $"Order {request.OrderNumber}",
            metadata = new { email = request.PayerEmail, mobile = request.PayerMobile, order_id = request.OrderNumber },
        }, ct);

        var json = await ReadJsonAsync(response, ct);
        var authority = json is JsonElement e ? ReadJsonPath(e, "data.authority") : null;

        if (string.IsNullOrWhiteSpace(authority))
            return GatewayInitiateResult.Fail($"ZarinPal rejected the request ({Describe(response, json)}).");

        return GatewayInitiateResult.Ok($"{ApiBase(ctx)}/pg/StartPay/{authority}", authority);
    }

    protected override async Task<GatewayVerifyResult> VerifyCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayCallback callback, CancellationToken ct)
    {
        // Zarinpal only returns Status=OK when the payer completed the flow; anything else is a
        // cancellation and must not reach the verify endpoint at all.
        if (!string.Equals(callback.Value("Status", "status"), "OK", StringComparison.OrdinalIgnoreCase))
            return GatewayVerifyResult.Fail("The payment was cancelled at the gateway.");

        var amount = callback.Value("amount");
        using var response = await client.PostAsJsonAsync($"{ApiBase(ctx)}/pg/v4/payment/verify.json", new
        {
            merchant_id = MerchantId(ctx),
            authority = callback.Authority,
            amount = long.TryParse(amount, out var parsed) ? parsed : 0,
        }, ct);

        var json = await ReadJsonAsync(response, ct);
        if (json is not JsonElement element)
            return GatewayVerifyResult.Fail($"ZarinPal returned an unreadable verify response ({(int)response.StatusCode}).");

        var code = ReadJsonPath(element, "data.code");

        // 100 = verified now, 101 = already verified on an earlier call. Both mean the money moved;
        // 101 exists precisely so a repeated callback is not mistaken for a failure.
        if (code is "100" or "101")
        {
            return new GatewayVerifyResult(
                Paid: true,
                ReferenceNumber: ReadJsonPath(element, "data.ref_id"),
                CardMask: ReadJsonPath(element, "data.card_pan"),
                PaidAmount: null,
                Error: null,
                AlreadyVerified: code == "101");
        }

        return GatewayVerifyResult.Fail($"ZarinPal did not confirm the payment (code {code ?? "?"}).");
    }
}

/// <summary>Zibal — Iranian aggregator.</summary>
public sealed class ZibalGatewayProvider : PaymentGatewayProviderBase
{
    public ZibalGatewayProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "Zibal";
    public override string DisplayName => "Zibal";
    public override GatewayAmountUnit AmountUnit => GatewayAmountUnit.Rial;

    private static string Merchant(PaymentGatewayContext ctx)
    {
        var fromJson = Parse<ZibalGatewaySettings>(ctx).Merchant;
        return !string.IsNullOrWhiteSpace(fromJson) ? fromJson : ctx.MerchantId ?? "";
    }

    public override bool IsConfigured(PaymentGatewayContext ctx) => !string.IsNullOrWhiteSpace(Merchant(ctx));

    protected override async Task<GatewayInitiateResult> InitiateCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayPaymentRequest request, CancellationToken ct)
    {
        using var response = await client.PostAsJsonAsync("https://gateway.zibal.ir/v1/request", new
        {
            // "zibal" is Zibal's documented sandbox merchant id.
            merchant = ctx.IsSandbox ? "zibal" : Merchant(ctx),
            amount = ToGatewayAmount(request.Amount),
            callbackUrl = request.ReturnUrl,
            description = request.Description ?? $"Order {request.OrderNumber}",
            orderId = request.OrderNumber,
            mobile = request.PayerMobile,
        }, ct);

        var json = await ReadJsonAsync(response, ct);
        var track = json is JsonElement e ? ReadJsonPath(e, "trackId") : null;
        var result = json is JsonElement r ? ReadJsonPath(r, "result") : null;

        if (result != "100" || string.IsNullOrWhiteSpace(track))
            return GatewayInitiateResult.Fail($"Zibal rejected the request ({Describe(response, json)}).");

        return GatewayInitiateResult.Ok($"https://gateway.zibal.ir/start/{track}", track);
    }

    protected override async Task<GatewayVerifyResult> VerifyCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayCallback callback, CancellationToken ct)
    {
        using var response = await client.PostAsJsonAsync("https://gateway.zibal.ir/v1/verify", new
        {
            merchant = ctx.IsSandbox ? "zibal" : Merchant(ctx),
            trackId = long.TryParse(callback.Authority, out var track) ? track : 0,
        }, ct);

        var json = await ReadJsonAsync(response, ct);
        if (json is not JsonElement element)
            return GatewayVerifyResult.Fail($"Zibal returned an unreadable verify response ({(int)response.StatusCode}).");

        var result = ReadJsonPath(element, "result");

        // 100 = verified, 201 = already verified.
        if (result is "100" or "201")
        {
            var amount = decimal.TryParse(ReadJsonPath(element, "amount"), out var raw)
                ? FromGatewayAmount(raw)
                : (decimal?)null;

            return new GatewayVerifyResult(true, ReadJsonPath(element, "refNumber"),
                ReadJsonPath(element, "cardNumber"), amount, null, result == "201");
        }

        return GatewayVerifyResult.Fail($"Zibal did not confirm the payment (result {result ?? "?"}).");
    }
}

/// <summary>IDPay — Iranian aggregator.</summary>
public sealed class IdPayGatewayProvider : PaymentGatewayProviderBase
{
    public IdPayGatewayProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "IdPay";
    public override string DisplayName => "IDPay";
    public override GatewayAmountUnit AmountUnit => GatewayAmountUnit.Rial;

    private static string ApiKey(PaymentGatewayContext ctx)
    {
        var fromJson = Parse<IdPayGatewaySettings>(ctx).ApiKey;
        return !string.IsNullOrWhiteSpace(fromJson) ? fromJson : ctx.ApiKey ?? "";
    }

    public override bool IsConfigured(PaymentGatewayContext ctx) => !string.IsNullOrWhiteSpace(ApiKey(ctx));

    private static HttpRequestMessage Build(PaymentGatewayContext ctx, string url, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        request.Headers.TryAddWithoutValidation("X-API-KEY", ApiKey(ctx));
        request.Headers.TryAddWithoutValidation("X-SANDBOX", ctx.IsSandbox ? "1" : "0");
        return request;
    }

    protected override async Task<GatewayInitiateResult> InitiateCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayPaymentRequest request, CancellationToken ct)
    {
        using var message = Build(ctx, "https://api.idpay.ir/v1.1/payment", new
        {
            order_id = request.OrderNumber,
            amount = ToGatewayAmount(request.Amount),
            callback = request.ReturnUrl,
            desc = request.Description ?? $"Order {request.OrderNumber}",
            mail = request.PayerEmail,
            phone = request.PayerMobile,
        });

        using var response = await client.SendAsync(message, ct);
        var json = await ReadJsonAsync(response, ct);

        var id = json is JsonElement e ? ReadJsonPath(e, "id") : null;
        var link = json is JsonElement l ? ReadJsonPath(l, "link") : null;

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(link))
            return GatewayInitiateResult.Fail($"IDPay rejected the request ({Describe(response, json)}).");

        return GatewayInitiateResult.Ok(link, id);
    }

    protected override async Task<GatewayVerifyResult> VerifyCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayCallback callback, CancellationToken ct)
    {
        using var message = Build(ctx, "https://api.idpay.ir/v1.1/payment/verify", new
        {
            id = callback.Authority,
            order_id = callback.Value("order_id", "orderId"),
        });

        using var response = await client.SendAsync(message, ct);
        var json = await ReadJsonAsync(response, ct);
        if (json is not JsonElement element)
            return GatewayVerifyResult.Fail($"IDPay returned an unreadable verify response ({(int)response.StatusCode}).");

        var status = ReadJsonPath(element, "status");

        // 100 = verified, 101 = already verified, 200 = settled to the merchant.
        if (status is "100" or "101" or "200")
        {
            var amount = decimal.TryParse(ReadJsonPath(element, "amount"), out var raw)
                ? FromGatewayAmount(raw)
                : (decimal?)null;

            return new GatewayVerifyResult(true, ReadJsonPath(element, "track_id"),
                ReadJsonPath(element, "payment.card_no"), amount, null, status == "101");
        }

        return GatewayVerifyResult.Fail($"IDPay did not confirm the payment (status {status ?? "?"}).");
    }
}

/// <summary>NextPay — Iranian aggregator.</summary>
public sealed class NextPayGatewayProvider : PaymentGatewayProviderBase
{
    public NextPayGatewayProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "NextPay";
    public override string DisplayName => "NextPay";
    public override GatewayAmountUnit AmountUnit => GatewayAmountUnit.Rial;

    private static string ApiKey(PaymentGatewayContext ctx)
    {
        var fromJson = Parse<NextPayGatewaySettings>(ctx).ApiKey;
        return !string.IsNullOrWhiteSpace(fromJson) ? fromJson : ctx.ApiKey ?? "";
    }

    public override bool IsConfigured(PaymentGatewayContext ctx) => !string.IsNullOrWhiteSpace(ApiKey(ctx));

    protected override async Task<GatewayInitiateResult> InitiateCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayPaymentRequest request, CancellationToken ct)
    {
        using var response = await client.PostAsJsonAsync("https://nextpay.org/nx/gateway/token", new
        {
            api_key = ApiKey(ctx),
            order_id = request.OrderNumber,
            amount = ToGatewayAmount(request.Amount),
            callback_uri = request.ReturnUrl,
            currency = "IRR",
            customer_phone = request.PayerMobile,
            payer_desc = request.Description ?? $"Order {request.OrderNumber}",
        }, ct);

        var json = await ReadJsonAsync(response, ct);
        var code = json is JsonElement e ? ReadJsonPath(e, "code") : null;
        var trans = json is JsonElement t ? ReadJsonPath(t, "trans_id") : null;

        if (code != "-1" || string.IsNullOrWhiteSpace(trans))
            return GatewayInitiateResult.Fail($"NextPay rejected the request ({Describe(response, json)}).");

        return GatewayInitiateResult.Ok($"https://nextpay.org/nx/gateway/payment/{trans}", trans);
    }

    protected override async Task<GatewayVerifyResult> VerifyCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayCallback callback, CancellationToken ct)
    {
        var amountText = callback.Value("amount");
        using var response = await client.PostAsJsonAsync("https://nextpay.org/nx/gateway/verify", new
        {
            api_key = ApiKey(ctx),
            trans_id = callback.Authority,
            amount = long.TryParse(amountText, out var parsed) ? parsed : 0,
            currency = "IRR",
        }, ct);

        var json = await ReadJsonAsync(response, ct);
        if (json is not JsonElement element)
            return GatewayVerifyResult.Fail($"NextPay returned an unreadable verify response ({(int)response.StatusCode}).");

        var code = ReadJsonPath(element, "code");

        // 0 = verified now, -1 with an existing verification means it was already settled.
        if (code == "0")
            return GatewayVerifyResult.Ok(ReadJsonPath(element, "Shaparak_Ref_Id"), null,
                ReadJsonPath(element, "card_holder"));

        return GatewayVerifyResult.Fail($"NextPay did not confirm the payment (code {code ?? "?"}).");
    }
}

/// <summary>Pay.ir — Iranian aggregator.</summary>
public sealed class PayIrGatewayProvider : PaymentGatewayProviderBase
{
    public PayIrGatewayProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "PayIr";
    public override string DisplayName => "Pay.ir";
    public override GatewayAmountUnit AmountUnit => GatewayAmountUnit.Rial;

    private static string ApiKey(PaymentGatewayContext ctx)
    {
        var fromJson = Parse<PayIrGatewaySettings>(ctx).ApiKey;
        var key = !string.IsNullOrWhiteSpace(fromJson) ? fromJson : ctx.ApiKey ?? "";
        // "test" is Pay.ir's documented sandbox key.
        return ctx.IsSandbox ? "test" : key;
    }

    public override bool IsConfigured(PaymentGatewayContext ctx) =>
        ctx.IsSandbox || !string.IsNullOrWhiteSpace(Parse<PayIrGatewaySettings>(ctx).ApiKey) ||
        !string.IsNullOrWhiteSpace(ctx.ApiKey);

    protected override async Task<GatewayInitiateResult> InitiateCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayPaymentRequest request, CancellationToken ct)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["api"] = ApiKey(ctx),
            ["amount"] = ToGatewayAmount(request.Amount).ToString(),
            ["redirect"] = request.ReturnUrl,
            ["factorNumber"] = request.OrderNumber,
            ["mobile"] = request.PayerMobile ?? "",
            ["description"] = request.Description ?? $"Order {request.OrderNumber}",
        });

        using var response = await client.PostAsync("https://pay.ir/pg/send", content, ct);
        var json = await ReadJsonAsync(response, ct);
        var token = json is JsonElement e ? ReadJsonPath(e, "token") : null;

        if (string.IsNullOrWhiteSpace(token))
            return GatewayInitiateResult.Fail($"Pay.ir rejected the request ({Describe(response, json)}).");

        return GatewayInitiateResult.Ok($"https://pay.ir/pg/{token}", token);
    }

    protected override async Task<GatewayVerifyResult> VerifyCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayCallback callback, CancellationToken ct)
    {
        if (callback.Value("status") == "0")
            return GatewayVerifyResult.Fail("The payment was cancelled at the gateway.");

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["api"] = ApiKey(ctx),
            ["token"] = callback.Authority ?? "",
        });

        using var response = await client.PostAsync("https://pay.ir/pg/verify", content, ct);
        var json = await ReadJsonAsync(response, ct);
        if (json is not JsonElement element)
            return GatewayVerifyResult.Fail($"Pay.ir returned an unreadable verify response ({(int)response.StatusCode}).");

        if (ReadJsonPath(element, "status") == "1")
        {
            var amount = decimal.TryParse(ReadJsonPath(element, "amount"), out var raw)
                ? FromGatewayAmount(raw)
                : (decimal?)null;

            return GatewayVerifyResult.Ok(ReadJsonPath(element, "transId"), amount,
                ReadJsonPath(element, "cardNumber"));
        }

        return GatewayVerifyResult.Fail("Pay.ir did not confirm the payment.");
    }
}

/// <summary>PayPing — Iranian aggregator.</summary>
public sealed class PayPingGatewayProvider : PaymentGatewayProviderBase
{
    public PayPingGatewayProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "PayPing";
    public override string DisplayName => "PayPing";

    // PayPing bills in Toman, not Rial — the exception that makes the per-provider unit necessary.
    public override GatewayAmountUnit AmountUnit => GatewayAmountUnit.Major;

    private static string Token(PaymentGatewayContext ctx)
    {
        var fromJson = Parse<PayPingGatewaySettings>(ctx).Token;
        return !string.IsNullOrWhiteSpace(fromJson) ? fromJson : ctx.ApiKey ?? "";
    }

    public override bool IsConfigured(PaymentGatewayContext ctx) => !string.IsNullOrWhiteSpace(Token(ctx));

    private static HttpRequestMessage Build(PaymentGatewayContext ctx, string url, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token(ctx));
        return request;
    }

    protected override async Task<GatewayInitiateResult> InitiateCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayPaymentRequest request, CancellationToken ct)
    {
        using var message = Build(ctx, "https://api.payping.ir/v2/pay", new
        {
            amount = ToGatewayAmount(request.Amount),
            returnUrl = request.ReturnUrl,
            clientRefId = request.OrderNumber,
            payerIdentity = request.PayerMobile ?? request.PayerEmail,
            description = request.Description ?? $"Order {request.OrderNumber}",
        });

        using var response = await client.SendAsync(message, ct);
        var json = await ReadJsonAsync(response, ct);
        var code = json is JsonElement e ? ReadJsonPath(e, "code") : null;

        if (string.IsNullOrWhiteSpace(code))
            return GatewayInitiateResult.Fail($"PayPing rejected the request ({Describe(response, json)}).");

        return GatewayInitiateResult.Ok($"https://api.payping.ir/v2/pay/gotoipg/{code}", code);
    }

    protected override async Task<GatewayVerifyResult> VerifyCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayCallback callback, CancellationToken ct)
    {
        var refId = callback.Value("refid", "refId");
        if (string.IsNullOrWhiteSpace(refId))
            return GatewayVerifyResult.Fail("The payment was cancelled at the gateway.");

        var amountText = callback.Value("amount");
        using var message = Build(ctx, "https://api.payping.ir/v2/pay/verify", new
        {
            refId,
            amount = long.TryParse(amountText, out var parsed) ? parsed : 0,
        });

        using var response = await client.SendAsync(message, ct);
        if (!response.IsSuccessStatusCode)
            return GatewayVerifyResult.Fail($"PayPing did not confirm the payment ({(int)response.StatusCode}).");

        var json = await ReadJsonAsync(response, ct);
        var amount = json is JsonElement e && decimal.TryParse(ReadJsonPath(e, "amount"), out var raw)
            ? FromGatewayAmount(raw)
            : (decimal?)null;

        return GatewayVerifyResult.Ok(refId, amount,
            json is JsonElement c ? ReadJsonPath(c, "cardNumber") : null);
    }
}

/// <summary>
/// Template-driven gateway: any PSP whose start/verify calls are plain HTTP can be wired up from the
/// admin UI with no code. This is what makes "support every Iranian gateway" a configuration
/// exercise for the ones without a first-class class here — including bank PSPs behind an aggregator.
/// </summary>
public sealed class GenericRedirectGatewayProvider : PaymentGatewayProviderBase
{
    public GenericRedirectGatewayProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "GenericRedirect";
    public override string DisplayName => "Custom redirect gateway";
    public override bool IsIranian => false;

    // The unit is part of the template, so the base conversion is bypassed and applied per settings.
    public override GatewayAmountUnit AmountUnit => GatewayAmountUnit.Major;

    public override bool IsConfigured(PaymentGatewayContext ctx)
    {
        var s = Parse<GenericRedirectGatewaySettings>(ctx);
        return !string.IsNullOrWhiteSpace(s.InitiateUrl)
               && !string.IsNullOrWhiteSpace(s.RedirectUrlTemplate)
               && !string.IsNullOrWhiteSpace(s.VerifyUrl);
    }

    protected override async Task<GatewayInitiateResult> InitiateCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayPaymentRequest request, CancellationToken ct)
    {
        var s = Parse<GenericRedirectGatewaySettings>(ctx);
        var amount = ConvertAmount(request.Amount, s.AmountUnit);

        string Fill(string template, bool urlEncode) => Substitute(template, urlEncode, new()
        {
            ["amount"] = amount.ToString(),
            ["orderNumber"] = request.OrderNumber,
            ["returnUrl"] = request.ReturnUrl,
            ["paymentId"] = request.PaymentId.ToString(),
            ["email"] = request.PayerEmail ?? "",
            ["mobile"] = request.PayerMobile ?? "",
            ["authority"] = "",
        });

        using var message = BuildMessage(s.InitiateMethod, Fill(s.InitiateUrl, true),
            s.InitiateBody is null ? null : Fill(s.InitiateBody, false), s.InitiateContentType, s.Headers, Fill);

        using var response = await client.SendAsync(message, ct);
        var json = await ReadJsonAsync(response, ct);

        var authority = json is JsonElement e ? ReadJsonPath(e, s.AuthorityJsonPath) : null;
        if (string.IsNullOrWhiteSpace(authority))
            return GatewayInitiateResult.Fail($"The gateway did not return an authority ({Describe(response, json)}).");

        var redirect = s.RedirectUrlTemplate.Replace("{authority}", Uri.EscapeDataString(authority), StringComparison.Ordinal);
        return GatewayInitiateResult.Ok(redirect, authority);
    }

    protected override async Task<GatewayVerifyResult> VerifyCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayCallback callback, CancellationToken ct)
    {
        var s = Parse<GenericRedirectGatewaySettings>(ctx);

        string Fill(string template, bool urlEncode) => Substitute(template, urlEncode, new()
        {
            ["authority"] = callback.Authority ?? "",
            ["amount"] = callback.Value("amount") ?? "",
            ["orderNumber"] = callback.Value("order_id", "orderId", "orderNumber") ?? "",
            ["returnUrl"] = "",
            ["paymentId"] = "",
            ["email"] = "",
            ["mobile"] = "",
        });

        using var message = BuildMessage(s.VerifyMethod, Fill(s.VerifyUrl, true),
            s.VerifyBody is null ? null : Fill(s.VerifyBody, false), s.VerifyContentType, s.Headers, Fill);

        using var response = await client.SendAsync(message, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            return GatewayVerifyResult.Fail($"The gateway did not confirm the payment ({(int)response.StatusCode}).");

        // The success marker is required: a 200 with an error payload is the normal failure mode for
        // these panels, so treating any 200 as paid would credit unpaid orders.
        if (string.IsNullOrWhiteSpace(s.VerifySuccessContains) ||
            !body.Contains(s.VerifySuccessContains, StringComparison.OrdinalIgnoreCase))
            return GatewayVerifyResult.Fail($"The gateway did not confirm the payment ({Truncate(body)}).");

        string? reference = null;
        try
        {
            reference = ReadJsonPath(JsonDocument.Parse(body).RootElement, s.ReferenceJsonPath);
        }
        catch (JsonException)
        {
            // Non-JSON response; the reference stays null and the authority identifies the payment.
        }

        return GatewayVerifyResult.Ok(reference ?? callback.Authority);
    }

    private static long ConvertAmount(decimal amount, GatewayAmountUnit unit) => unit switch
    {
        GatewayAmountUnit.Rial => (long)Math.Round(amount * 10m, MidpointRounding.AwayFromZero),
        GatewayAmountUnit.Minor => (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero),
        _ => (long)Math.Round(amount, MidpointRounding.AwayFromZero),
    };

    private static string Substitute(string template, bool urlEncode, Dictionary<string, string> values)
    {
        var result = template;
        foreach (var (key, value) in values)
        {
            var replacement = urlEncode ? Uri.EscapeDataString(value) : value;
            result = result.Replace("{" + key + "}", replacement, StringComparison.Ordinal);
        }
        return result;
    }

    private static HttpRequestMessage BuildMessage(string method, string url, string? body, string contentType,
        Dictionary<string, string> headers, Func<string, bool, string> fill)
    {
        var verb = string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase) ? HttpMethod.Get : HttpMethod.Post;
        var message = new HttpRequestMessage(verb, url);

        if (verb == HttpMethod.Post && !string.IsNullOrWhiteSpace(body))
            message.Content = new StringContent(body, Encoding.UTF8, contentType);

        foreach (var (name, value) in headers)
            message.Headers.TryAddWithoutValidation(name, fill(value, false));

        return message;
    }
}
