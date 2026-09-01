using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Payments;

/// <summary>Resolves a registered <see cref="IPaymentGatewayProvider"/> by its key.</summary>
public sealed class PaymentGatewayProviderRegistry : IPaymentGatewayProviderRegistry
{
    private readonly IReadOnlyList<IPaymentGatewayProvider> _providers;

    public PaymentGatewayProviderRegistry(IEnumerable<IPaymentGatewayProvider> providers) =>
        _providers = providers.ToList();

    public IReadOnlyList<IPaymentGatewayProvider> All => _providers;

    public IPaymentGatewayProvider? Find(string? key) =>
        string.IsNullOrWhiteSpace(key)
            ? null
            : _providers.FirstOrDefault(p => string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase));
}

/// <inheritdoc cref="IOnlinePaymentService" />
public sealed class OnlinePaymentService : IOnlinePaymentService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IPaymentGatewayProviderRegistry _registry;
    private readonly IOrderService _orders;
    private readonly ILogger<OnlinePaymentService> _logger;

    public OnlinePaymentService(
        IDbContextFactory<AppDbContext> contextFactory,
        IPaymentGatewayProviderRegistry registry,
        IOrderService orders,
        ILogger<OnlinePaymentService> logger)
    {
        _contextFactory = contextFactory;
        _registry = registry;
        _orders = orders;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PaymentGatewayInfo>> GetAvailableAsync(int websiteId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var rows = await context.PaymentGateways.AsNoTracking()
            .Where(g => g.WebsiteID == websiteId && g.IsActive)
            .OrderBy(g => g.SortOrder).ThenBy(g => g.PaymentGatewayID)
            .ToListAsync(ct);

        // A gateway missing its credentials is never offered: letting a customer pick it would send
        // them to a dead end mid-checkout.
        return rows
            .Select(row => (Row: row, Provider: _registry.Find(row.Provider)))
            .Where(x => x.Provider is not null && x.Provider.IsConfigured(ToContext(x.Row)))
            .Select(x => ToInfo(x.Row, x.Provider!))
            .ToList();
    }

    public async Task<GatewayInitiateResult> StartAsync(int websiteId, int clientId, int orderId, int gatewayId,
        string returnUrl, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var order = await context.Orders
            .Include(o => o.WebsiteClient)
            .FirstOrDefaultAsync(o => o.OrderID == orderId
                                      && o.WebsiteID == websiteId
                                      && o.WebsiteClientID == clientId, ct);

        if (order is null)
            return GatewayInitiateResult.Fail("Order not found.");

        if (order.Status != (byte)OrderStatus.PendingPayment)
            return GatewayInitiateResult.Fail("This order is not awaiting payment.");

        var gatewayRow = await context.PaymentGateways.AsNoTracking()
            .FirstOrDefaultAsync(g => g.PaymentGatewayID == gatewayId && g.WebsiteID == websiteId && g.IsActive, ct);

        if (gatewayRow is null)
            return GatewayInitiateResult.Fail("Payment gateway not found.");

        var provider = _registry.Find(gatewayRow.Provider);
        if (provider is null)
            return GatewayInitiateResult.Fail($"Unknown payment gateway '{gatewayRow.Provider}'.");

        var gatewayContext = ToContext(gatewayRow);
        if (!provider.IsConfigured(gatewayContext))
            return GatewayInitiateResult.Fail($"{provider.DisplayName} is not fully configured.");

        // The Payment row is created before the gateway call so an attempt that the payer abandons
        // still leaves a trace an admin can reconcile against the PSP's own dashboard.
        var payment = new Payment
        {
            WebsiteID = websiteId,
            OrderID = order.OrderID,
            WebsiteClientID = clientId,
            Method = (byte)PaymentMethod.OnlineGateway,
            PaymentGatewayID = gatewayRow.PaymentGatewayID,
            Amount = order.GrandTotal,
            CurrencyCode = order.CurrencyCode,
            ExchangeRateToUsd = order.ExchangeRateToUsd,
            AmountUsd = order.GrandTotalUsd,
            Status = (byte)PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };
        context.Payments.Add(payment);
        await context.SaveChangesAsync(ct);

        var effectiveReturnUrl = string.IsNullOrWhiteSpace(gatewayRow.CallbackUrl) ? returnUrl : gatewayRow.CallbackUrl;

        var result = await provider.InitiateAsync(gatewayContext, new GatewayPaymentRequest(
            PaymentId: payment.PaymentID,
            Amount: order.GrandTotal,
            CurrencyCode: order.CurrencyCode,
            OrderNumber: order.OrderNumber,
            ReturnUrl: effectiveReturnUrl,
            Description: $"Order {order.OrderNumber}",
            PayerEmail: order.WebsiteClient?.Email,
            PayerMobile: order.WebsiteClient?.Cellphone), ct);

        if (!result.Success || string.IsNullOrWhiteSpace(result.Authority))
        {
            payment.Status = (byte)PaymentStatus.Failed;
            await context.SaveChangesAsync(ct);
            _logger.LogWarning("Gateway {Provider} refused to start payment {PaymentId}: {Error}",
                provider.Key, payment.PaymentID, result.Error);
            return result;
        }

        // The authority is how the callback finds this row again — nothing else identifies it.
        payment.GatewayAuthority = result.Authority;
        await context.SaveChangesAsync(ct);

        return result;
    }

    public async Task<GatewayVerifyResult> CompleteAsync(int websiteId, GatewayCallback callback,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(callback.Authority))
            return GatewayVerifyResult.Fail("The gateway response did not identify a payment.");

        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var payment = await context.Payments
            .Include(p => p.PaymentGateway)
            .FirstOrDefaultAsync(p => p.WebsiteID == websiteId && p.GatewayAuthority == callback.Authority, ct);

        if (payment is null)
            return GatewayVerifyResult.Fail("No pending payment matches this gateway response.");

        // Callbacks are retried by PSPs and re-opened by users pressing back. Answering from the
        // stored state rather than re-verifying is what stops a second callback from crediting the
        // order twice.
        if (payment.Status == (byte)PaymentStatus.Paid)
            return new GatewayVerifyResult(true, payment.GatewayRefNumber, null, null, null, AlreadyVerified: true);

        if (payment.PaymentGateway is null)
            return GatewayVerifyResult.Fail("The gateway for this payment no longer exists.");

        var provider = _registry.Find(payment.PaymentGateway.Provider);
        if (provider is null)
            return GatewayVerifyResult.Fail($"Unknown payment gateway '{payment.PaymentGateway.Provider}'.");

        var result = await provider.VerifyAsync(ToContext(payment.PaymentGateway), callback, ct);

        if (!result.Paid)
        {
            payment.Status = (byte)PaymentStatus.Failed;
            await context.SaveChangesAsync(ct);
            return result;
        }

        // The gateway confirmed a payment; check it is the payment we asked for. A mismatch means
        // either a gateway bug or a tampered amount, and either way the order must not be marked
        // paid on the strength of it.
        if (result.PaidAmount is decimal charged && Math.Abs(charged - payment.Amount) > 0.01m)
        {
            _logger.LogError(
                "Gateway {Provider} confirmed {Charged} for payment {PaymentId}, which is for {Expected}. " +
                "The order has not been marked paid.",
                provider.Key, charged, payment.PaymentID, payment.Amount);

            payment.Status = (byte)PaymentStatus.Failed;
            await context.SaveChangesAsync(ct);
            return GatewayVerifyResult.Fail("The amount confirmed by the gateway does not match this order.");
        }

        payment.Status = (byte)PaymentStatus.Paid;
        payment.GatewayRefNumber = Truncate(result.ReferenceNumber, 100);
        payment.TrackingCode = Truncate(result.CardMask, 100);
        payment.PaidAt = DateTime.UtcNow;
        await context.SaveChangesAsync(ct);

        if (payment.OrderID is int orderId)
        {
            // Runs through the normal transition so stock, settlement and notifications happen exactly
            // as they do for a wallet or receipt payment.
            await _orders.TransitionStatusAsync(orderId, OrderStatus.Paid, memberId: null,
                note: $"Paid online via {provider.DisplayName} (ref {result.ReferenceNumber}).", ct);
        }

        return result;
    }

    private static PaymentGatewayContext ToContext(PaymentGateway row) => new()
    {
        PaymentGatewayID = row.PaymentGatewayID,
        WebsiteID = row.WebsiteID,
        Provider = row.Provider,
        SettingsJson = string.IsNullOrWhiteSpace(row.SettingsJSON) ? "{}" : row.SettingsJSON,
        MerchantId = row.MerchantID,
        ApiKey = row.ApiKey,
        ApiSecret = row.ApiSecret,
        IsSandbox = row.IsSandbox,
        CallbackUrl = row.CallbackUrl,
    };

    private static PaymentGatewayInfo ToInfo(PaymentGateway row, IPaymentGatewayProvider provider) => new()
    {
        PaymentGatewayID = row.PaymentGatewayID,
        Provider = row.Provider,
        ProviderDisplayName = provider.DisplayName,
        Name = row.Name,
        IsSandbox = row.IsSandbox,
        IsActive = row.IsActive,
        SortOrder = row.SortOrder,
        IsConfigured = true,
    };

    private static string? Truncate(string? value, int max) =>
        string.IsNullOrEmpty(value) ? null : (value.Length <= max ? value : value[..max]);
}
