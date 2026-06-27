using System.Text.Json;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Infrastructure.Storage;

/// <summary>Bunny.net Edge Storage backend served through a CDN pull zone (REST API over HTTPS).</summary>
public sealed class BunnyStorageProvider : IFileStorageProvider
{
    private readonly IHttpClientFactory _httpFactory;

    public BunnyStorageProvider(IHttpClientFactory httpFactory) => _httpFactory = httpFactory;

    public StorageProviderType Provider => StorageProviderType.BunnyCDN;

    private static BunnyStorageSettings Parse(StorageSettingContext ctx) =>
        JsonSerializer.Deserialize<BunnyStorageSettings>(ctx.SettingsJson) ?? new BunnyStorageSettings();

    private static string StorageHost(BunnyStorageSettings s) =>
        string.IsNullOrWhiteSpace(s.Region)
            ? "storage.bunnycdn.com"
            : $"{s.Region.Trim().ToLowerInvariant()}.storage.bunnycdn.com";

    private static string ObjectUrl(BunnyStorageSettings s, string storedName) =>
        $"https://{StorageHost(s)}/{s.StorageZoneName.Trim('/')}/{storedName}";

    public async Task<StorageUploadResult> UploadAsync(StorageSettingContext ctx, Stream data, string storedName,
        string mimeType, CancellationToken ct = default)
    {
        var s = Parse(ctx);
        var client = _httpFactory.CreateClient();

        using var req = new HttpRequestMessage(HttpMethod.Put, ObjectUrl(s, storedName))
        {
            Content = new StreamContent(data),
        };
        req.Content.Headers.TryAddWithoutValidation("Content-Type", string.IsNullOrWhiteSpace(mimeType) ? "application/octet-stream" : mimeType);
        req.Headers.Add("AccessKey", s.AccessKey);

        using var resp = await client.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();

        var pull = s.PullZoneUrl.TrimEnd('/');
        return new StorageUploadResult
        {
            StoragePath = storedName,
            CdnUrl = $"{pull}/{storedName}",
            CdnFileCode = storedName,
        };
    }

    public async Task DeleteAsync(StorageSettingContext ctx, string storedName, CancellationToken ct = default)
    {
        var s = Parse(ctx);
        var client = _httpFactory.CreateClient();

        using var req = new HttpRequestMessage(HttpMethod.Delete, ObjectUrl(s, storedName));
        req.Headers.Add("AccessKey", s.AccessKey);
        using var resp = await client.SendAsync(req, ct);
        // A missing object (404) is treated as already deleted.
        if (resp.StatusCode != System.Net.HttpStatusCode.NotFound)
            resp.EnsureSuccessStatusCode();
    }

    public Task<StorageQuota> GetQuotaAsync(StorageSettingContext ctx, CancellationToken ct = default)
    {
        // Storage-zone usage requires an account API key; total is admin-declared, used is computed from our DB.
        var s = Parse(ctx);
        return Task.FromResult(new StorageQuota { UsedKB = 0, TotalKB = s.CapacityKB });
    }

    public async Task<bool> TestConnectionAsync(StorageSettingContext ctx, CancellationToken ct = default)
    {
        try
        {
            var s = Parse(ctx);
            var client = _httpFactory.CreateClient();
            // Listing the zone root validates both the host and the access key.
            using var req = new HttpRequestMessage(HttpMethod.Get,
                $"https://{StorageHost(s)}/{s.StorageZoneName.Trim('/')}/");
            req.Headers.Add("AccessKey", s.AccessKey);
            using var resp = await client.SendAsync(req, ct);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
