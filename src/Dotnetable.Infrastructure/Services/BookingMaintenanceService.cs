using Dotnetable.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Services;

/// <summary>Releases expired booking holds and deletes visit rows past the retention window. Invoices stay.</summary>
public sealed class BookingMaintenanceService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDatabaseConfigStore _configStore;
    private readonly ILogger<BookingMaintenanceService> _logger;

    public BookingMaintenanceService(
        IServiceScopeFactory scopeFactory,
        IDatabaseConfigStore configStore,
        ILogger<BookingMaintenanceService> logger)
    {
        _scopeFactory = scopeFactory;
        _configStore = configStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(45), stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_configStore.IsConfigured)
                {
                    using var scope = _scopeFactory.CreateScope();
                    await scope.ServiceProvider.GetRequiredService<IBookingService>().SweepAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Booking maintenance sweep failed.");
            }

            try { await Task.Delay(Interval, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }
}
