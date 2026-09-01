using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Dotnetable.Application.DTOs;

namespace Dotnetable.Infrastructure.Payments;

/// <summary>Stripe — international. Hosted Checkout Session, verified by retrieving the session.</summary>
public sealed class StripeGatewayProvider : PaymentGatewayProviderBase
{
    public StripeGatewayProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "Stripe";
    public override string DisplayName => "Stripe";
    public override bool IsIranian => false;
    public override GatewayAmountUnit AmountUnit => GatewayAmountUnit.Minor;

    private static string SecretKey(PaymentGatewayContext ctx)
    {
        var fromJson = Parse<StripeGatewaySettings>(ctx).SecretKey;
        return !string.IsNullOrWhiteSpace(fromJson) ? fromJson : ctx.ApiSecret ?? ctx.ApiKey ?? "";
    }

    public override bool IsConfigured(PaymentGatewayContext ctx) => !string.IsNullOrWhiteSpace(SecretKey(ctx));

    /// <summary>
    /// Zero-decimal currencies (JPY, KRW, …) are quoted in whole units by Stripe. Multiplying those
    /// by 100 would charge a hundred times the order total.
    /// </summary>
    private static readonly HashSet<string> ZeroDecimalCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "BIF", "CLP", "DJF", "GNF", "JPY", "KMF", "KRW", "MGA", "PYG", "RWF", "UGX", "VND", "VUV", "XAF", "XOF", "XPF",
    };

    private static long StripeAmount(decimal amount, string currencyCode) =>
        ZeroDecimalCurrencies.Contains(currencyCode)
            ? (long)Math.Round(amount, MidpointRounding.AwayFromZero)
            : (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);

    protected override async Task<GatewayInitiateResult> InitiateCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayPaymentRequest request, CancellationToken ct)
    {
        // Stripe appends its own session id to the success URL so the return can be verified.
        var successUrl = AppendQuery(request.ReturnUrl, "session_id", "{CHECKOUT_SESSION_ID}");

        var form = new Dictionary<string, string>
        {
            ["mode"] = "payment",
            ["success_url"] = successUrl,
            ["cancel_url"] = AppendQuery(request.ReturnUrl, "cancelled", "1"),
            ["client_reference_id"] = request.OrderNumber,
            ["line_items[0][quantity]"] = "1",
            ["line_items[0][price_data][currency]"] = request.CurrencyCode.ToLowerInvariant(),
            ["line_items[0][price_data][unit_amount]"] = StripeAmount(request.Amount, request.CurrencyCode).ToString(),
            ["line_items[0][price_data][product_data][name]"] = request.Description ?? $"Order {request.OrderNumber}",
        };

        if (!string.IsNullOrWhiteSpace(request.PayerEmail))
            form["customer_email"] = request.PayerEmail;

        using var message = new HttpRequestMessage(HttpMethod.Post, "https://api.stripe.com/v1/checkout/sessions")
        {
            Content = new FormUrlEncodedContent(form),
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SecretKey(ctx));

        using var response = await client.SendAsync(message, ct);
        var json = await ReadJsonAsync(response, ct);

        var id = json is JsonElement e ? ReadJsonPath(e, "id") : null;
        var url = json is JsonElement u ? ReadJsonPath(u, "url") : null;

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(url))
            return GatewayInitiateResult.Fail($"Stripe rejected the request ({Describe(response, json)}).");

        return GatewayInitiateResult.Ok(url, id);
    }

    protected override async Task<GatewayVerifyResult> VerifyCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayCallback callback, CancellationToken ct)
    {
        var sessionId = callback.Authority ?? callback.Value("session_id");
        if (string.IsNullOrWhiteSpace(sessionId))
            return GatewayVerifyResult.Fail("The payment was cancelled at the gateway.");

        using var message = new HttpRequestMessage(HttpMethod.Get,
            $"https://api.stripe.com/v1/checkout/sessions/{Uri.EscapeDataString(sessionId)}");
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SecretKey(ctx));

        using var response = await client.SendAsync(message, ct);
        var json = await ReadJsonAsync(response, ct);
        if (json is not JsonElement element)
            return GatewayVerifyResult.Fail($"Stripe returned an unreadable verify response ({(int)response.StatusCode}).");

        // payment_status is the authoritative field; "complete" on the session alone can also mean a
        // still-processing async method.
        if (!string.Equals(ReadJsonPath(element, "payment_status"), "paid", StringComparison.OrdinalIgnoreCase))
            return GatewayVerifyResult.Fail("Stripe reports the payment as not completed.");

        return GatewayVerifyResult.Ok(ReadJsonPath(element, "payment_intent"));
    }

    private static string AppendQuery(string url, string key, string value) =>
        url.Contains('?', StringComparison.Ordinal)
            ? $"{url}&{key}={value}"
            : $"{url}?{key}={value}";
}

/// <summary>PayPal — international. Orders v2 with client-credentials auth.</summary>
public sealed class PayPalGatewayProvider : PaymentGatewayProviderBase
{
    public PayPalGatewayProvider(IHttpClientFactory f) : base(f) { }

    public override string Key => "PayPal";
    public override string DisplayName => "PayPal";
    public override bool IsIranian => false;
    public override GatewayAmountUnit AmountUnit => GatewayAmountUnit.Major;

    private static PayPalGatewaySettings Settings(PaymentGatewayContext ctx)
    {
        var s = Parse<PayPalGatewaySettings>(ctx);
        if (string.IsNullOrWhiteSpace(s.ClientId)) s.ClientId = ctx.ApiKey ?? "";
        if (string.IsNullOrWhiteSpace(s.ClientSecret)) s.ClientSecret = ctx.ApiSecret ?? "";
        return s;
    }

    public override bool IsConfigured(PaymentGatewayContext ctx)
    {
        var s = Settings(ctx);
        return !string.IsNullOrWhiteSpace(s.ClientId) && !string.IsNullOrWhiteSpace(s.ClientSecret);
    }

    private static string ApiBase(PaymentGatewayContext ctx) =>
        ctx.IsSandbox ? "https://api-m.sandbox.paypal.com" : "https://api-m.paypal.com";

    private static async Task<string?> GetAccessTokenAsync(HttpClient client, PaymentGatewayContext ctx,
        CancellationToken ct)
    {
        var s = Settings(ctx);
        using var message = new HttpRequestMessage(HttpMethod.Post, $"{ApiBase(ctx)}/v1/oauth2/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
            }),
        };
        var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{s.ClientId}:{s.ClientSecret}"));
        message.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);

        using var response = await client.SendAsync(message, ct);
        var json = await ReadJsonAsync(response, ct);
        return json is JsonElement e ? ReadJsonPath(e, "access_token") : null;
    }

    protected override async Task<GatewayInitiateResult> InitiateCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayPaymentRequest request, CancellationToken ct)
    {
        var token = await GetAccessTokenAsync(client, ctx, ct);
        if (string.IsNullOrWhiteSpace(token))
            return GatewayInitiateResult.Fail("PayPal rejected the credentials.");

        using var message = new HttpRequestMessage(HttpMethod.Post, $"{ApiBase(ctx)}/v2/checkout/orders")
        {
            Content = JsonContent.Create(new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        reference_id = request.OrderNumber,
                        description = request.Description ?? $"Order {request.OrderNumber}",
                        amount = new
                        {
                            currency_code = request.CurrencyCode.ToUpperInvariant(),
                            // PayPal wants a two-decimal string, not a number.
                            value = request.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                        },
                    },
                },
                payment_source = new
                {
                    paypal = new
                    {
                        experience_context = new
                        {
                            return_url = request.ReturnUrl,
                            cancel_url = request.ReturnUrl,
                            user_action = "PAY_NOW",
                        },
                    },
                },
            }),
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(message, ct);
        var json = await ReadJsonAsync(response, ct);
        if (json is not JsonElement element)
            return GatewayInitiateResult.Fail($"PayPal rejected the request ({(int)response.StatusCode}).");

        var id = ReadJsonPath(element, "id");
        var approveUrl = element.TryGetProperty("links", out var links) && links.ValueKind == JsonValueKind.Array
            ? links.EnumerateArray()
                .FirstOrDefault(l => ReadJsonPath(l, "rel") == "payer-action" || ReadJsonPath(l, "rel") == "approve")
            : default;

        var href = approveUrl.ValueKind == JsonValueKind.Object ? ReadJsonPath(approveUrl, "href") : null;

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(href))
            return GatewayInitiateResult.Fail($"PayPal rejected the request ({Describe(response, json)}).");

        return GatewayInitiateResult.Ok(href, id);
    }

    protected override async Task<GatewayVerifyResult> VerifyCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayCallback callback, CancellationToken ct)
    {
        var orderId = callback.Authority ?? callback.Value("token");
        if (string.IsNullOrWhiteSpace(orderId))
            return GatewayVerifyResult.Fail("The payment was cancelled at the gateway.");

        var token = await GetAccessTokenAsync(client, ctx, ct);
        if (string.IsNullOrWhiteSpace(token))
            return GatewayVerifyResult.Fail("PayPal rejected the credentials.");

        // Capture is what actually takes the money; an approved-but-uncaptured order is not paid.
        using var message = new HttpRequestMessage(HttpMethod.Post,
            $"{ApiBase(ctx)}/v2/checkout/orders/{Uri.EscapeDataString(orderId)}/capture")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json"),
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(message, ct);
        var json = await ReadJsonAsync(response, ct);
        if (json is not JsonElement element)
            return GatewayVerifyResult.Fail($"PayPal returned an unreadable capture response ({(int)response.StatusCode}).");

        var status = ReadJsonPath(element, "status");

        // ORDER_ALREADY_CAPTURED is what a duplicated callback produces; it means paid, not failed.
        if (!string.Equals(status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
        {
            var issue = ReadJsonPath(element, "details.0.issue") ?? ReadJsonPath(element, "name");
            if (string.Equals(issue, "ORDER_ALREADY_CAPTURED", StringComparison.OrdinalIgnoreCase))
                return new GatewayVerifyResult(true, orderId, null, null, null, AlreadyVerified: true);

            return GatewayVerifyResult.Fail($"PayPal reports the payment as {status ?? issue ?? "not completed"}.");
        }

        return GatewayVerifyResult.Ok(
            ReadJsonPath(element, "purchase_units.0.payments.captures.0.id") ?? orderId);
    }
}
