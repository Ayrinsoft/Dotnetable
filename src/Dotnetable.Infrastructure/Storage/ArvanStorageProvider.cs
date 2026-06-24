using System.Text.Json;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Infrastructure.Storage;

/// <summary>ArvanCloud object storage via the S3-compatible API (AWS SDK).</summary>
public sealed class ArvanStorageProvider : IFileStorageProvider
{
    public StorageProviderType Provider => StorageProviderType.Arvan;

    private static ArvanStorageSettings Parse(StorageSettingContext ctx) =>
        JsonSerializer.Deserialize<ArvanStorageSettings>(ctx.SettingsJson) ?? new ArvanStorageSettings();

    private static AmazonS3Client BuildClient(ArvanStorageSettings s)
    {
        var config = new AmazonS3Config
        {
            ServiceURL = s.Endpoint,
            ForcePathStyle = true,
        };
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
