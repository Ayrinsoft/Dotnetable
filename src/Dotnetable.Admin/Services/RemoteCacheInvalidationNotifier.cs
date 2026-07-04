using System.Net.Http.Json;
using Dotnetable.Application.Caching;
using Dotnetable.Application.Interfaces;

namespace Dotnetable.Admin.Services;

/// <summary>
/// Pushes cache-tag invalidation to the central API right after Admin writes a Menu/Category/Page/Post
/// record — Admin and API each hold their own Infrastructure/DB stack and their own in-memory cache, so
/// Admin's local invalidation never reaches the API process on its own. Best-effort: a short timeout and
/// swallowed failures mean a slow/unreachable API never blocks the admin save, it just falls back to the
/// API's own cache TTL.
/// </summary>
public class RemoteCacheInvalidationNotifier : ICacheInvalidationNotifier
{
    private static readonly TimeSpan CallTimeout = TimeSpan.FromSeconds(3);

    private readonly HttpClient _http;
    private readonly ILogger<RemoteCacheInvalidationNotifier> _logger;
    private readonly string? _secret;

    public RemoteCacheInvalidationNotifier(HttpClient http, IConfiguration configuration, ILogger<RemoteCacheInvalidationNotifier> logger)
    {
        _http = http;
        _logger = logger;
        _secret = configuration["Internal:SyncSecret"];
    }

    public async Task NotifyAsync(string tag, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_secret)) return;

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(CallTimeout);

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/cache/invalidate");
            request.Headers.Add(CacheSyncContracts.InternalKeyHeader, _secret);
            request.Content = JsonContent.Create(new CacheInvalidateRequest(tag));

            using var response = await _http.SendAsync(request, cts.Token);
            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("API cache invalidation for tag {Tag} returned {Status}.", tag, response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to notify the API to invalidate cache tag {Tag}.", tag);
        }
    }
}
