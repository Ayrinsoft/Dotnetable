using System.Net.Http.Json;
using System.Text.Json;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;

namespace Dotnetable.Infrastructure.Payments;

/// <summary>
/// Shared plumbing for the HTTP-based gateways: settings parsing, amount conversion, and a single
/// place that turns a transport failure into a result instead of an exception.
///
/// <para>Exception handling here is not cosmetic. A gateway timing out during
/// <see cref="IPaymentGatewayProvider.VerifyAsync"/> must produce "not verified" and leave the order
/// unpaid, never an unhandled 500 that leaves the payment in a state nobody reconciles.</para>
/// </summary>
public abstract class PaymentGatewayProviderBase : IPaymentGatewayProvider
{
    private readonly IHttpClientFactory _httpClientFactory;

    protected PaymentGatewayProviderBase(IHttpClientFactory httpClientFactory) =>
        _httpClientFactory = httpClientFactory;

    public abstract string Key { get; }
    public abstract string DisplayName { get; }
    public virtual bool IsIranian => true;
    public virtual GatewayAmountUnit AmountUnit => GatewayAmountUnit.Rial;

    public abstract bool IsConfigured(PaymentGatewayContext ctx);

    public async Task<GatewayInitiateResult> InitiateAsync(PaymentGatewayContext ctx,
        GatewayPaymentRequest request, CancellationToken ct = default)
    {
        if (!IsConfigured(ctx))
            return GatewayInitiateResult.Fail($"{DisplayName} is missing required settings.");

        if (request.Amount <= 0)
            return GatewayInitiateResult.Fail("The amount must be greater than zero.");

        try
        {
            using var client = CreateClient();
            return await InitiateCoreAsync(client, ctx, request, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            return GatewayInitiateResult.Fail($"{DisplayName} could not start the payment: {ex.Message}");
        }
    }

    public async Task<GatewayVerifyResult> VerifyAsync(PaymentGatewayContext ctx, GatewayCallback callback,
        CancellationToken ct = default)
    {
        if (!IsConfigured(ctx))
            return GatewayVerifyResult.Fail($"{DisplayName} is missing required settings.");

        try
        {
            using var client = CreateClient();
            return await VerifyCoreAsync(client, ctx, callback, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            // Deliberately "not paid": an unreachable gateway is not evidence of payment.
            return GatewayVerifyResult.Fail($"{DisplayName} could not be reached to verify the payment: {ex.Message}");
        }
    }

    public virtual Task<GatewayRefundResult> RefundAsync(PaymentGatewayContext ctx,
        GatewayRefundRequest request, CancellationToken ct = default) =>
        Task.FromResult(GatewayRefundResult.NotSupported);

    protected abstract Task<GatewayInitiateResult> InitiateCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayPaymentRequest request, CancellationToken ct);

    protected abstract Task<GatewayVerifyResult> VerifyCoreAsync(HttpClient client,
        PaymentGatewayContext ctx, GatewayCallback callback, CancellationToken ct);

    protected HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        // Long enough for a slow PSP, short enough that a hung gateway does not tie up the request.
        client.Timeout = TimeSpan.FromSeconds(30);
        return client;
    }

    protected static T Parse<T>(PaymentGatewayContext ctx) where T : new()
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
    /// Converts a site-currency amount into the integer unit this gateway bills in. Rounds rather
    /// than truncates so a 1,999.5 total is not silently charged as 1,999.
    /// </summary>
    protected long ToGatewayAmount(decimal amount) => AmountUnit switch
    {
        GatewayAmountUnit.Rial => (long)Math.Round(amount * 10m, MidpointRounding.AwayFromZero),
        GatewayAmountUnit.Minor => (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero),
        _ => (long)Math.Round(amount, MidpointRounding.AwayFromZero),
    };

    /// <summary>Converts a gateway-reported amount back to the site's major unit, for the mismatch check.</summary>
    protected decimal FromGatewayAmount(decimal amount) => AmountUnit switch
    {
        GatewayAmountUnit.Rial => amount / 10m,
        GatewayAmountUnit.Minor => amount / 100m,
        _ => amount,
    };

    /// <summary>Reads a dotted path such as <c>data.authority</c> out of a JSON document.</summary>
    protected static string? ReadJsonPath(JsonElement root, string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;

        var current = root;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out var next))
                return null;
            current = next;
        }

        return current.ValueKind switch
        {
            JsonValueKind.String => current.GetString(),
            JsonValueKind.Number => current.ToString(),
            JsonValueKind.True or JsonValueKind.False => current.ToString(),
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            _ => current.ToString(),
        };
    }

    protected static async Task<JsonElement?> ReadJsonAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var document = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
            return document;
        }
        catch (Exception)
        {
            // Some PSPs answer an error with HTML or plain text; the caller reports the status instead.
            return null;
        }
    }

    protected static string Describe(HttpResponseMessage response, JsonElement? json) =>
        json is JsonElement element
            ? $"{(int)response.StatusCode}: {Truncate(element.ToString())}"
            : $"{(int)response.StatusCode}";

    protected static string Truncate(string? value, int max = 300) =>
        string.IsNullOrWhiteSpace(value) ? "" : (value.Length <= max ? value.Trim() : value[..max].Trim());
}
