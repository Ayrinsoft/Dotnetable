using System.Text.Json;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Infrastructure.Storage;

/// <summary>
/// Shared implementation for every S3-compatible backend (AWS S3, Cloudflare R2, Backblaze B2, MinIO).
/// Subclasses only declare their <see cref="Provider"/>; all credentials live in <see cref="S3StorageSettings"/>.
/// </summary>
public abstract class S3StorageProviderBase : IFileStorageProvider
{
    public abstract StorageProviderType Provider { get; }

    private static S3StorageSettings Parse(StorageSettingContext ctx) =>
        JsonSerializer.Deserialize<S3StorageSettings>(ctx.SettingsJson) ?? new S3StorageSettings();

    private static AmazonS3Client BuildClient(S3StorageSettings s)
    {
        var config = new AmazonS3Config { ForcePathStyle = s.ForcePathStyle };

        if (!string.IsNullOrWhiteSpace(s.Endpoint))
            config.ServiceURL = s.Endpoint;
        else if (!string.IsNullOrWhiteSpace(s.Region))
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(s.Region);

        return new AmazonS3Client(new BasicAWSCredentials(s.AccessKey, s.SecretKey), config);
    }

    public async Task<StorageUploadResult> UploadAsync(StorageSettingContext ctx, Stream data, string storedName,
        string mimeType, CancellationToken ct = default)
    {
        var s = Parse(ctx);
        using var client = BuildClient(s);

        await client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = s.BucketName,
            Key = storedName,
            InputStream = data,
            ContentType = mimeType,
            CannedACL = S3CannedACL.PublicRead,
            AutoCloseStream = false,
            DisablePayloadSigning = true,
        }, ct);

        var baseUrl = s.PublicBaseUrl.TrimEnd('/');
        return new StorageUploadResult
        {
            StoragePath = storedName,
            CdnUrl = $"{baseUrl}/{storedName}",
            CdnFileCode = storedName,
        };
    }

    public async Task DeleteAsync(StorageSettingContext ctx, string storedName, CancellationToken ct = default)
    {
        var s = Parse(ctx);
        using var client = BuildClient(s);
        await client.DeleteObjectAsync(new DeleteObjectRequest { BucketName = s.BucketName, Key = storedName }, ct);
    }

    public Task<StorageQuota> GetQuotaAsync(StorageSettingContext ctx, CancellationToken ct = default)
    {
        // S3 exposes no quota API: total is the admin-declared plan capacity; "used" is computed from our DB.
        var s = Parse(ctx);
        return Task.FromResult(new StorageQuota { UsedKB = 0, TotalKB = s.CapacityKB });
    }

    public async Task<bool> TestConnectionAsync(StorageSettingContext ctx, CancellationToken ct = default)
    {
        try
        {
            var s = Parse(ctx);
            using var client = BuildClient(s);
            await client.ListObjectsV2Async(new ListObjectsV2Request { BucketName = s.BucketName, MaxKeys = 1 }, ct);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>Amazon Web Services S3.</summary>
public sealed class AwsS3StorageProvider : S3StorageProviderBase
{
    public override StorageProviderType Provider => StorageProviderType.AwsS3;
}

/// <summary>Cloudflare R2 (S3-compatible).</summary>
public sealed class CloudflareR2StorageProvider : S3StorageProviderBase
{
    public override StorageProviderType Provider => StorageProviderType.CloudflareR2;
}

/// <summary>Backblaze B2 via its S3-compatible endpoint.</summary>
public sealed class BackBlazeStorageProvider : S3StorageProviderBase
{
    public override StorageProviderType Provider => StorageProviderType.BackBlaze;
}

/// <summary>MinIO self-hosted object storage (S3-compatible).</summary>
public sealed class MinioStorageProvider : S3StorageProviderBase
{
    public override StorageProviderType Provider => StorageProviderType.MinIO;
}
