using System.Net;
using Amazon.S3;
using Amazon.S3.Model;

namespace Dotnetable.Infrastructure.Storage;

/// <summary>
/// Object-level operations shared by every S3-compatible backend (<see cref="S3StorageProviderBase"/>
/// and <see cref="ArvanStorageProvider"/>), so the header and ACL rules live in one place.
/// </summary>
internal static class S3ObjectOps
{
    private const string RobotsKey = "robots.txt";

    /// <summary>Keeps every crawler off the bucket: files are linked from the site, never indexed on their own.</summary>
    private const string RobotsBody = "User-agent: *\nDisallow: /\n";

    public static async Task PutAsync(AmazonS3Client client, string bucket, Stream data, string key,
        string mimeType, string? cacheControl, CancellationToken ct)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucket,
            Key = key,
            InputStream = data,
            ContentType = mimeType,
            CannedACL = S3CannedACL.PublicRead,
            AutoCloseStream = false,
            DisablePayloadSigning = true,
        };
        if (!string.IsNullOrWhiteSpace(cacheControl))
            request.Headers.CacheControl = cacheControl;

        await client.PutObjectAsync(request, ct);
    }

    /// <summary>
    /// Server-side self-copy with <c>MetadataDirective=REPLACE</c>: the only way S3 lets you change an
    /// existing object's headers. Nothing is downloaded. Content-Type, Content-Disposition,
    /// Content-Encoding and user metadata are carried over explicitly because REPLACE drops them, and
    /// the public-read ACL is re-applied because a copy does not inherit it.
    /// </summary>
    public static async Task<bool> SetCacheControlAsync(AmazonS3Client client, string bucket, string key,
        string cacheControl, CancellationToken ct)
    {
        GetObjectMetadataResponse head;
        try
        {
            head = await client.GetObjectMetadataAsync(new GetObjectMetadataRequest { BucketName = bucket, Key = key }, ct);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        // Already carries the header — skip the copy (idempotent, and saves a billed operation).
        if (string.Equals(head.Headers.CacheControl, cacheControl, StringComparison.OrdinalIgnoreCase))
            return true;

        var copy = new CopyObjectRequest
        {
            SourceBucket = bucket,
            SourceKey = key,
            DestinationBucket = bucket,
            DestinationKey = key,
            MetadataDirective = S3MetadataDirective.REPLACE,
            ContentType = head.Headers.ContentType,
            CannedACL = S3CannedACL.PublicRead,
        };
        copy.Headers.CacheControl = cacheControl;
        if (!string.IsNullOrWhiteSpace(head.Headers.ContentDisposition))
            copy.Headers.ContentDisposition = head.Headers.ContentDisposition;
        if (!string.IsNullOrWhiteSpace(head.Headers.ContentEncoding))
            copy.Headers.ContentEncoding = head.Headers.ContentEncoding;
        foreach (var metaKey in head.Metadata.Keys)
            copy.Metadata.Add(metaKey, head.Metadata[metaKey]);

        await client.CopyObjectAsync(copy, ct);
        return true;
    }

    /// <summary>Uploads a deny-all robots.txt unless the bucket already has one (an admin's own file wins).</summary>
    public static async Task EnsureRobotsTxtAsync(AmazonS3Client client, string bucket, CancellationToken ct)
    {
        try
        {
            await client.GetObjectMetadataAsync(new GetObjectMetadataRequest { BucketName = bucket, Key = RobotsKey }, ct);
            return;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // Missing — create it below.
        }

        var request = new PutObjectRequest
        {
            BucketName = bucket,
            Key = RobotsKey,
            ContentBody = RobotsBody,
            ContentType = "text/plain; charset=utf-8",
            CannedACL = S3CannedACL.PublicRead,
            DisablePayloadSigning = true,
        };
        request.Headers.CacheControl = "public, max-age=86400";
        await client.PutObjectAsync(request, ct);
    }
}
