using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// The housekeeping nothing else owns: rows that are only ever inserted, never deleted, and that a
/// live shop generates continuously. Each of these grows without bound otherwise — the guest-cart
/// table in particular, which gets a new row for every anonymous visitor who touches a product page.
/// </summary>
public sealed class MaintenanceService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    /// <summary>Guest carts nobody has touched for this long are abandoned by any definition.</summary>
    private static readonly TimeSpan AbandonedCartAge = TimeSpan.FromDays(60);

    /// <summary>Activation/reset codes are valid for 30 minutes; a day of history is generous.</summary>
    private static readonly TimeSpan ExpiredCodeAge = TimeSpan.FromDays(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDatabaseConfigStore _configStore;
    private readonly ILogger<MaintenanceService> _logger;

    public MaintenanceService(
        IServiceScopeFactory scopeFactory,
        IDatabaseConfigStore configStore,
        ILogger<MaintenanceService> logger)
    {
        _scopeFactory = scopeFactory;
        _configStore = configStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_configStore.IsConfigured)
                    await RunAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Maintenance pass failed.");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        var refreshTokens = scope.ServiceProvider.GetRequiredService<IRefreshTokenService>();

        await using var context = await contextFactory.CreateDbContextAsync(ct);

        var now = DateTime.UtcNow;

        // Guest carts only. A signed-in customer's cart is theirs to keep.
        var cartCutoff = now - AbandonedCartAge;
        var abandonedCarts = await context.Carts
            .Where(c => c.WebsiteClientID == null && c.UpdatedAt < cartCutoff)
            .Select(c => c.CartID)
            .Take(500)
            .ToListAsync(ct);

        if (abandonedCarts.Count > 0)
        {
            // Items first: the FK would reject deleting the parent otherwise.
            await context.CartItems.Where(i => abandonedCarts.Contains(i.CartID)).ExecuteDeleteAsync(ct);
            await context.Carts.Where(c => abandonedCarts.Contains(c.CartID)).ExecuteDeleteAsync(ct);
            _logger.LogInformation("Removed {Count} abandoned guest cart(s).", abandonedCarts.Count);
        }

        var codeCutoff = now - ExpiredCodeAge;
        var codes = await context.WebsiteClientForgetPasswords
            .Where(f => f.LogTime < codeCutoff)
            .ExecuteDeleteAsync(ct);
        if (codes > 0)
            _logger.LogInformation("Removed {Count} expired one-time code(s).", codes);

        var tokens = await refreshTokens.PurgeExpiredAsync(ct);
        if (tokens > 0)
            _logger.LogInformation("Removed {Count} expired refresh token(s).", tokens);
    }
}
