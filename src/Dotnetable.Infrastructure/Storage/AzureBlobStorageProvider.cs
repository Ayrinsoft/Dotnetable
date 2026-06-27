using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Infrastructure.Storage;

/// <summary>Azure Blob Storage backend.</summary>
public sealed class AzureBlobStorageProvider : IFileStorageProvider
{
    public StorageProviderType Provider => StorageProviderType.Azure;

    private static AzureStorageSettings Parse(StorageSettingContext ctx) =>
        JsonSerializer.Deserialize<AzureStorageSettings>(ctx.SettingsJson) ?? new AzureStorageSettings();

    private static BlobContainerClient BuildContainer(AzureStorageSettings s) =>
        new(s.ConnectionString, s.ContainerName);

    public async Task<StorageUploadResult> UploadAsync(StorageSettingContext ctx, Stream data, string storedName,
        string mimeType, CancellationToken ct = default)
    {
        var s = Parse(ctx);
        var container = BuildContainer(s);
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);

        var blob = container.GetBlobClient(storedName);
        await blob.UploadAsync(data, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = mimeType },
        }, ct);

        var baseUrl = s.PublicBaseUrl.TrimEnd('/');
        return new StorageUploadResult
        {
            StoragePath = storedName,
            CdnUrl = string.IsNullOrEmpty(baseUrl) ? blob.Uri.ToString() : $"{baseUrl}/{storedName}",
            CdnFileCode = storedName,
        };
    }

    public async Task DeleteAsync(StorageSettingContext ctx, string storedName, CancellationToken ct = default)
    {
        var s = Parse(ctx);
        var container = BuildContainer(s);
        await container.DeleteBlobIfExistsAsync(storedName, cancellationToken: ct);
    }

    public Task<StorageQuota> GetQuotaAsync(StorageSettingContext ctx, CancellationToken ct = default)
    {
        // Azure exposes no simple per-container quota: total is admin-declared, used is computed from our DB.
        var s = Parse(ctx);
        return Task.FromResult(new StorageQuota { UsedKB = 0, TotalKB = s.CapacityKB });
    }

    public async Task<bool> TestConnectionAsync(StorageSettingContext ctx, CancellationToken ct = default)
    {
        try
        {
            var s = Parse(ctx);
            var container = BuildContainer(s);
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);
            return await container.ExistsAsync(ct);
        }
        catch
        {
            return false;
        }
    }
}
