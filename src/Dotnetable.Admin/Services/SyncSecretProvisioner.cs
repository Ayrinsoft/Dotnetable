using System.Net;
using System.Net.Http.Json;
using Dotnetable.Hosting;

namespace Dotnetable.Admin.Services;

/// <summary>
/// Ensures Admin and the API end up sharing the same <c>Internal:SyncSecret</c> without anyone
/// copying a value between the two deployments by hand. Called right after Setup saves the database
/// connection — that is the moment <c>Api:BaseUrl</c> is known to be reachable and, in practice, the
/// moment this deployment is being wired together for the first time.
/// </summary>
public class SyncSecretProvisioner
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SyncSecretProvisioner> _logger;

    public SyncSecretProvisioner(
        IConfiguration configuration,
        IHostEnvironment environment,
        IHttpClientFactory httpClientFactory,
        ILogger<SyncSecretProvisioner> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>Best-effort: a slow/unreachable API here never blocks Setup — cache invalidation just
    /// keeps falling back to the API's own TTL until the secret is sorted out, same as at runtime.</summary>
    public async Task EnsureSyncedWithApiAsync(CancellationToken ct = default)
    {
        var current = _configuration["Internal:SyncSecret"];
        var secret = current;

        if (string.IsNullOrWhiteSpace(current) || StartupValidation.IsPlaceholderSecret(current))
        {
            secret = StartupValidation.GenerateSecretValue();
            if (!StartupValidation.TrySetLocalSetting(_configuration, _environment, "Internal:SyncSecret", secret, out var error))
            {
                _logger.LogWarning("Could not save a generated Internal:SyncSecret: {Error}", error);
                return;
            }
        }

        var apiBaseUrl = _configuration["Api:BaseUrl"];
        if (string.IsNullOrWhiteSpace(apiBaseUrl)) return;

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(5);

            using var response = await client.PostAsJsonAsync(
                "api/internal/bootstrap-sync-secret", new BootstrapSyncSecretRequest(secret!), ct);

            if (response.StatusCode == HttpStatusCode.Conflict)
                _logger.LogInformation("The API already has its own Internal:SyncSecret configured; leaving it as-is.");
            else if (!response.IsSuccessStatusCode)
                _logger.LogWarning("Failed to sync Internal:SyncSecret to the API ({Status}).", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not reach the API at {ApiBaseUrl} to sync Internal:SyncSecret.", apiBaseUrl);
        }
    }

    private record BootstrapSyncSecretRequest(string Secret);
}
