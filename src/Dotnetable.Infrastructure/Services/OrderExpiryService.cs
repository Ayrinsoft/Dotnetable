using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Cancels orders that were never paid and gives their stock back.
///
/// <para>Checkout reserves stock the moment an order is created, before any payment. Nothing ever
/// released those reservations for orders that were simply abandoned, so every unpaid checkout
/// permanently removed its items from the sellable pool: a shop with real traffic would drift
/// towards showing "out of stock" for products sitting on the shelf, with no way to tell why. This
/// is the other half of that transaction, and it is a background job rather than a manual admin
/// action because abandonment is the normal case, not the exception.</para>
///
/// <para>Runs in whichever host is configured to own scheduled work. Each pass takes only orders
/// whose <c>ReservationExpiresAt</c> has passed and which are still <see cref="OrderStatus.PendingPayment"/>,
/// so two hosts racing produce the same outcome rather than a double release.</para>
/// </summary>
public sealed class OrderExpiryService : BackgroundService
{
    /// <summary>How long an unpaid order holds its stock. Long enough for a bank transfer receipt.</summary>
    public static readonly TimeSpan DefaultReservationWindow = TimeSpan.FromHours(24);

    private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(10);

    /// <summary>Bounded so one pass cannot hold a long transaction over thousands of rows.</summary>
    private const int BatchSize = 100;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDatabaseConfigStore _configStore;
    private readonly ILogger<OrderExpiryService> _logger;

    public OrderExpiryService(
        IServiceScopeFactory scopeFactory,
        IDatabaseConfigStore configStore,
        ILogger<OrderExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _configStore = configStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Give the host a moment to finish starting; a first-run install has no database yet.
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_configStore.IsConfigured)
                    await SweepAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // A failed sweep must never take the host down; the next pass retries.
                _logger.LogError(ex, "Order expiry sweep failed.");
            }

            try
            {
                await Task.Delay(SweepInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task SweepAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        var orders = scope.ServiceProvider.GetRequiredService<IOrderService>();

        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var now = DateTime.UtcNow;
        var expired = await context.Orders.AsNoTracking()
            .Where(o => o.Status == (byte)OrderStatus.PendingPayment
                        && o.ReservationExpiresAt != null
                        && o.ReservationExpiresAt < now)
            .OrderBy(o => o.OrderID)
            .Select(o => o.OrderID)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (expired.Count == 0) return;

        var released = 0;
        foreach (var orderId in expired)
        {
            try
            {
                // The Cancelled transition is the existing path that releases every reservation this
                // order took — listing, inventory and warehouse — so expiry reuses it rather than
                // re-deriving the release logic and drifting from it.
                var cancelled = await orders.TransitionStatusAsync(
                    orderId, OrderStatus.Cancelled, memberId: null,
                    note: "Automatically cancelled: payment window expired.", ct);

                if (cancelled) released++;
            }
            catch (Exception ex)
            {
                // One bad order must not stop the batch.
                _logger.LogError(ex, "Could not expire unpaid order {OrderId}.", orderId);
            }
        }

        if (released > 0)
            _logger.LogInformation("Expired {Count} unpaid order(s) and released their stock.", released);
    }
}
