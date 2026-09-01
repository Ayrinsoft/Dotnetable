namespace Dotnetable.Application.DTOs;

/// <summary>
/// The unit a gateway expects an amount in.
///
/// <para>This exists because Iranian gateways are split: the shop prices in Toman, most aggregators
/// bill in Rial (×10), and a few take Toman directly. Encoding it per provider is what keeps a
/// customer from being charged ten times the order total.</para>
/// </summary>
public enum GatewayAmountUnit
{
    /// <summary>The site currency's ordinary major unit (Toman, Euro, ...).</summary>
    Major = 0,

    /// <summary>Rial — ten times the Toman amount.</summary>
    Rial = 1,

    /// <summary>Hundredths of the major unit (Stripe cents, PayPal has its own two-decimal string).</summary>
    Minor = 2,
}

/// <summary>Strongly-typed view of a <c>PaymentGateway</c> row, handed to a provider at call time.</summary>
public sealed class PaymentGatewayContext
{
    public int PaymentGatewayID { get; init; }
    public int WebsiteID { get; init; }
    public string Provider { get; init; } = "";

    /// <summary>Provider credentials/options JSON. Preferred over the flat fields below.</summary>
    public string SettingsJson { get; init; } = "{}";

    /// <summary>Legacy flat credential columns, still honoured when the JSON omits them.</summary>
    public string? MerchantId { get; init; }
    public string? ApiKey { get; init; }
    public string? ApiSecret { get; init; }

    /// <summary>Test mode: providers switch to the gateway's sandbox endpoints.</summary>
    public bool IsSandbox { get; init; }

    /// <summary>Configured return URL, when the site pins one instead of using the per-request value.</summary>
    public string? CallbackUrl { get; init; }
}

/// <summary>What a provider needs in order to start a payment.</summary>
/// <param name="Amount">Order total in the site currency's major unit; the provider converts per <see cref="GatewayAmountUnit"/>.</param>
/// <param name="CurrencyCode">ISO code of the order currency.</param>
/// <param name="OrderNumber">The shop's own order number, sent so the PSP's dashboard is reconcilable.</param>
/// <param name="ReturnUrl">Absolute URL the payer comes back to.</param>
public sealed record GatewayPaymentRequest(
    int PaymentId,
    decimal Amount,
    string CurrencyCode,
    string OrderNumber,
    string ReturnUrl,
    string? Description = null,
    string? PayerEmail = null,
    string? PayerMobile = null);

/// <summary>Where to send the payer, or why the payment could not be started.</summary>
/// <param name="RedirectUrl">The gateway page to send the payer to.</param>
/// <param name="Authority">The gateway's handle for this attempt, stored on the payment for verification.</param>
public sealed record GatewayInitiateResult(
    bool Success,
    string? RedirectUrl = null,
    string? Authority = null,
    string? Error = null)
{
    public static GatewayInitiateResult Ok(string redirectUrl, string authority) =>
        new(true, redirectUrl, authority);

    public static GatewayInitiateResult Fail(string error) => new(false, Error: error);
}

/// <summary>Everything the gateway sent back on return, both query string and posted form.</summary>
/// <param name="Authority">The handle the gateway echoed, matched against the stored payment.</param>
public sealed record GatewayCallback(
    string? Authority,
    string? Status,
    IReadOnlyDictionary<string, string> Values)
{
    /// <summary>Case-insensitive lookup for the field names each PSP happens to use.</summary>
    public string? Value(params string[] names)
    {
        foreach (var name in names)
        {
            var match = Values.FirstOrDefault(kv => string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(match.Value)) return match.Value;
        }
        return null;
    }
}

/// <summary>The verified outcome of a payment attempt.</summary>
/// <param name="Paid">True only when the gateway confirmed, server to server, that it took the money.</param>
/// <param name="ReferenceNumber">The gateway's settlement reference, shown to the customer and used for reconciliation.</param>
/// <param name="PaidAmount">Amount the gateway reports charging, in the site's major unit, for the mismatch check.</param>
public sealed record GatewayVerifyResult(
    bool Paid,
    string? ReferenceNumber = null,
    string? CardMask = null,
    decimal? PaidAmount = null,
    string? Error = null,
    bool AlreadyVerified = false)
{
    public static GatewayVerifyResult Ok(string? reference, decimal? paidAmount = null, string? cardMask = null) =>
        new(true, reference, cardMask, paidAmount);

    public static GatewayVerifyResult Fail(string error) => new(false, Error: error);
}

/// <summary>A refund request against a settled payment.</summary>
public sealed record GatewayRefundRequest(
    string? Authority,
    string? ReferenceNumber,
    decimal Amount,
    string CurrencyCode,
    string? Reason = null);

/// <summary>Outcome of a refund attempt.</summary>
public sealed record GatewayRefundResult(bool Success, string? ReferenceNumber = null, string? Error = null,
    bool Supported = true)
{
    public static GatewayRefundResult Ok(string? reference) => new(true, reference);
    public static GatewayRefundResult Fail(string error) => new(false, Error: error);

    /// <summary>The gateway offers no refund API; the admin must refund through the PSP's own panel.</summary>
    public static readonly GatewayRefundResult NotSupported =
        new(false, null, "This gateway does not support refunds through the API. Refund it in the gateway's own panel.", false);
}

/// <summary>A gateway as offered to a customer at checkout / listed in the admin.</summary>
public sealed class PaymentGatewayInfo
{
    public int PaymentGatewayID { get; init; }
    public string Provider { get; init; } = "";
    public string ProviderDisplayName { get; init; } = "";
    public string Name { get; init; } = "";
    public bool IsSandbox { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }

    /// <summary>False when the row is missing credentials the provider needs; such a gateway is never offered.</summary>
    public bool IsConfigured { get; init; }
}

// ── Per-provider settings, persisted in PaymentGateway.SettingsJSON ─────────────────────

/// <summary>Zarinpal — Iranian aggregator. Merchant id is a GUID issued by Zarinpal.</summary>
public sealed class ZarinpalGatewaySettings
{
    public string MerchantId { get; set; } = "";
}

/// <summary>Zibal — Iranian aggregator.</summary>
public sealed class ZibalGatewaySettings
{
    public string Merchant { get; set; } = "";
}

/// <summary>IDPay — Iranian aggregator.</summary>
public sealed class IdPayGatewaySettings
{
    public string ApiKey { get; set; } = "";
}

/// <summary>NextPay — Iranian aggregator.</summary>
public sealed class NextPayGatewaySettings
{
    public string ApiKey { get; set; } = "";
}

/// <summary>Pay.ir — Iranian aggregator.</summary>
public sealed class PayIrGatewaySettings
{
    public string ApiKey { get; set; } = "";
}

/// <summary>PayPing — Iranian aggregator.</summary>
public sealed class PayPingGatewaySettings
{
    public string Token { get; set; } = "";
}

/// <summary>Stripe — international. Uses a hosted Checkout Session.</summary>
public sealed class StripeGatewaySettings
{
    public string SecretKey { get; set; } = "";

    /// <summary>Optional endpoint secret, when webhooks are used alongside the return-URL verify.</summary>
    public string? WebhookSecret { get; set; }
}

/// <summary>PayPal — international. Orders v2 with client-credentials auth.</summary>
public sealed class PayPalGatewaySettings
{
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
}

/// <summary>
/// Template-driven redirect gateway: covers any PSP whose start/verify calls are plain HTTP, without
/// writing a provider class. This is what lets a bank PSP or a new aggregator be wired up from the
/// admin UI while a first-class provider is written.
///
/// <para>Placeholders <c>{amount}</c>, <c>{orderNumber}</c>, <c>{returnUrl}</c>, <c>{paymentId}</c>,
/// <c>{authority}</c>, <c>{email}</c> and <c>{mobile}</c> are substituted in URLs and bodies.</para>
/// </summary>
public sealed class GenericRedirectGatewaySettings
{
    /// <summary>Endpoint that starts a payment and returns a token/authority.</summary>
    public string InitiateUrl { get; set; } = "";

    public string InitiateMethod { get; set; } = "POST";
    public string? InitiateBody { get; set; }
    public string InitiateContentType { get; set; } = "application/json";

    /// <summary>JSON path (dotted) or regex group name that holds the authority in the initiate response.</summary>
    public string AuthorityJsonPath { get; set; } = "";

    /// <summary>Where to send the payer. <c>{authority}</c> is substituted.</summary>
    public string RedirectUrlTemplate { get; set; } = "";

    /// <summary>Endpoint that confirms the payment, server to server.</summary>
    public string VerifyUrl { get; set; } = "";

    public string VerifyMethod { get; set; } = "POST";
    public string? VerifyBody { get; set; }
    public string VerifyContentType { get; set; } = "application/json";

    /// <summary>Substring the verify response must contain for the payment to count as settled.</summary>
    public string VerifySuccessContains { get; set; } = "";

    /// <summary>JSON path holding the settlement reference in the verify response.</summary>
    public string? ReferenceJsonPath { get; set; }

    /// <summary>Unit this PSP bills in.</summary>
    public GatewayAmountUnit AmountUnit { get; set; } = GatewayAmountUnit.Rial;

    public Dictionary<string, string> Headers { get; set; } = new();
}
