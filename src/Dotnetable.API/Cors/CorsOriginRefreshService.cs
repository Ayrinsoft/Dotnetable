namespace Dotnetable.API.Cors;

/// <summary>
/// Keeps <see cref="DynamicCorsOriginProvider"/> warm: an immediate refresh so newly deployed
/// instances do not reject legitimate storefronts while waiting out the first interval, then a
/// refresh every <paramref name="interval"/> so a website's address change (or a brand-new site)
/// becomes an allowed CORS origin without an API restart.
/// </summary>
public class CorsOriginRefreshService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    private readonly DynamicCorsOriginProvider _provider;

    public CorsOriginRefreshService(DynamicCorsOriginProvider provider) => _provider = provider;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        await _provider.RefreshAsync(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
            await _provider.RefreshAsync(stoppingToken);
    }
}
