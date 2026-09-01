using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// One online payment gateway.
///
/// <para>Almost every PSP — Iranian aggregators, Iranian bank PSPs, Stripe, PayPal — follows the same
/// three beats: ask the gateway to start a payment and get back a handle; send the payer to the
/// gateway; when they come back, ask the gateway what actually happened. The differences are in
/// endpoints, field names and how the amount is denominated, which is exactly what a provider class
/// is for. Adding a gateway is a new implementation of this interface and a row in
/// <c>PaymentGateways</c> — no changes to checkout, orders or the payment service.</para>
///
/// <para>The third beat is the one that matters for correctness: an order is marked paid on the
/// result of <see cref="VerifyAsync"/>, never on the callback's own query string. A callback URL is
/// something the payer's browser is redirected to and can therefore be forged; only the
/// server-to-server verify call is evidence that money moved.</para>
/// </summary>
public interface IPaymentGatewayProvider
{
    /// <summary>Stable key stored in <c>PaymentGateway.Provider</c>, e.g. <c>Zarinpal</c>.</summary>
    string Key { get; }

    /// <summary>Human-readable name for the admin gateway picker.</summary>
    string DisplayName { get; }

    /// <summary>Grouping hint for the admin UI only.</summary>
    bool IsIranian { get; }

    /// <summary>
    /// The currency unit this gateway expects amounts in. Iranian gateways historically take Rial
    /// while shops price in Toman, and getting it wrong charges ten times too much or too little —
    /// so it is declared per provider rather than assumed.
    /// </summary>
    GatewayAmountUnit AmountUnit { get; }

    /// <summary>False when the stored settings are missing something the gateway cannot start without.</summary>
    bool IsConfigured(PaymentGatewayContext ctx);

    /// <summary>Starts a payment and returns where to send the payer.</summary>
    Task<GatewayInitiateResult> InitiateAsync(PaymentGatewayContext ctx, GatewayPaymentRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Asks the gateway, server to server, whether the payment completed — the only thing an order
    /// status may be changed on.
    /// </summary>
    Task<GatewayVerifyResult> VerifyAsync(PaymentGatewayContext ctx, GatewayCallback callback,
        CancellationToken ct = default);

    /// <summary>
    /// Refunds a settled payment. Gateways that offer no refund API return
    /// <see cref="GatewayRefundResult.NotSupported"/> so the admin is told to refund out of band
    /// rather than the shop silently recording a refund that never happened.
    /// </summary>
    Task<GatewayRefundResult> RefundAsync(PaymentGatewayContext ctx, GatewayRefundRequest request,
        CancellationToken ct = default);
}

/// <summary>Resolves the <see cref="IPaymentGatewayProvider"/> registered for a provider key.</summary>
public interface IPaymentGatewayProviderRegistry
{
    /// <summary>The provider for <paramref name="key"/>, or null when none is registered.</summary>
    IPaymentGatewayProvider? Find(string? key);

    /// <summary>Every registered gateway, for the admin picker.</summary>
    IReadOnlyList<IPaymentGatewayProvider> All { get; }
}

/// <summary>
/// Drives an online payment end to end for a website: picks the configured gateway, creates the
/// <c>Payment</c> row, and applies the verified outcome to the order.
/// </summary>
public interface IOnlinePaymentService
{
    /// <summary>Gateways a customer may pay this website with.</summary>
    Task<IReadOnlyList<PaymentGatewayInfo>> GetAvailableAsync(int websiteId, CancellationToken ct = default);

    /// <summary>
    /// Starts a payment for an order the customer owns and returns the URL to send them to. Creates a
    /// <c>Payment</c> row in <c>Pending</c> so the attempt is recorded even if the payer never returns.
    /// </summary>
    Task<GatewayInitiateResult> StartAsync(int websiteId, int clientId, int orderId, int gatewayId,
        string returnUrl, CancellationToken ct = default);

    /// <summary>
    /// Completes a payment from the gateway's callback: verifies server to server, marks the payment
    /// and the order, and is safe to call twice — a repeated callback returns the first outcome
    /// rather than crediting the order again.
    /// </summary>
    Task<GatewayVerifyResult> CompleteAsync(int websiteId, GatewayCallback callback, CancellationToken ct = default);
}
