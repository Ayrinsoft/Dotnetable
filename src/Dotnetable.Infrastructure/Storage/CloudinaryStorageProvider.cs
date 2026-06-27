using System.Text.Json;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Infrastructure.Storage;

/// <summary>Cloudinary media-platform backend.</summary>
public sealed class CloudinaryStorageProvider : IFileStorageProvider
{
    public StorageProviderType Provider => StorageProviderType.Cloudinary;

    private static CloudinaryStorageSettings Parse(StorageSettingContext ctx) =>
        JsonSerializer.Deserialize<CloudinaryStorageSettings>(ctx.SettingsJson) ?? new CloudinaryStorageSettings();

    private static Cloudinary BuildClient(CloudinaryStorageSettings s) =>
        new(new Account(s.CloudName, s.ApiKey, s.ApiSecret));

    public async Task<StorageUploadResult> UploadAsync(StorageSettingContext ctx, Stream data, string storedName,
        string mimeType, CancellationToken ct = default)
    {
        var s = Parse(ctx);
        var client = BuildClient(s);
        var folder = string.IsNullOrWhiteSpace(s.Folder) ? null : s.Folder.Trim().Trim('/');

        // Images go through the image pipeline (transformations/thumbnails); everything else is a raw asset.
        if (mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            var result = await client.UploadAsync(new ImageUploadParams
            {
                File = new FileDescription(storedName, data),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = true,
            }, ct);
            return new StorageUploadResult
            {
                StoragePath = result.PublicId,
                CdnUrl = result.SecureUrl?.ToString(),
                CdnFileCode = result.PublicId,
            };
        }
        else
        {
            var result = await client.UploadAsync(new RawUploadParams
            {
                File = new FileDescription(storedName, data),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = true,
            });
            return new StorageUploadResult
            {
                StoragePath = result.PublicId,
                CdnUrl = result.SecureUrl?.ToString(),
                CdnFileCode = result.PublicId,
            };
        }
    }

    public async Task DeleteAsync(StorageSettingContext ctx, string storedName, CancellationToken ct = default)
    {
        var s = Parse(ctx);
        var client = BuildClient(s);
        // storedName is the Cloudinary PublicId saved at upload time.
        await client.DestroyAsync(new DeletionParams(storedName) { ResourceType = ResourceType.Auto });
    }

    public Task<StorageQuota> GetQuotaAsync(StorageSettingContext ctx, CancellationToken ct = default)
    {
        // Total is admin-declared; used is computed from our DB.
        var s = Parse(ctx);
        return Task.FromResult(new StorageQuota { UsedKB = 0, TotalKB = s.CapacityKB });
    }

    public async Task<bool> TestConnectionAsync(StorageSettingContext ctx, CancellationToken ct = default)
    {
        try
        {
            var s = Parse(ctx);
            var client = BuildClient(s);
            var usage = await client.GetUsageAsync();
            return usage?.Error is null;
        }
        catch
        {
            return false;
        }
    }
}
