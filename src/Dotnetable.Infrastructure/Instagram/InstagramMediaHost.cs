using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Dotnetable.Application.Instagram;
using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Instagram;

public interface IInstagramMediaHost
{
    Task<string> HostImageAsync(int websiteId, byte[] content, string fileName, ImageCropRect? crop, CancellationToken ct);

    Task<string> HostVideoAsync(int websiteId, byte[] content, string fileName, string? mime, CancellationToken ct);

    Task<byte[]> DownloadAsync(string url, CancellationToken ct);

    Task DeleteHostedAsync(IReadOnlyCollection<string> urls, CancellationToken ct);
}

/// <summary>Puts temporary JPEG and video files on the website's public storage, then deletes those rows.</summary>
public sealed class InstagramMediaHost : IInstagramMediaHost
{
    private const int MaxImageBytes = 8 * 1024 * 1024;
    private const int MaxVideoBytes = 50 * 1024 * 1024;

    private readonly IDbContextFactory<AppDbContext> _contexts;
    private readonly IFileService _files;
    private readonly IHttpClientFactory _http;
    private readonly ConcurrentDictionary<string, int> _hosted = new(StringComparer.Ordinal);

    public InstagramMediaHost(IDbContextFactory<AppDbContext> contexts, IFileService files, IHttpClientFactory http)
    {
        _contexts = contexts;
        _files = files;
        _http = http;
    }

    public async Task<string> HostImageAsync(int websiteId, byte[] content, string fileName, ImageCropRect? crop, CancellationToken ct)
    {
        if (content.Length == 0 || content.Length > MaxImageBytes)
            throw new InstagramException("Each photo must be a public link or a file under 8 MB.");

        var storageId = await ActiveStorageIdAsync(websiteId, ct);
        var baseName = Path.GetFileNameWithoutExtension(string.IsNullOrWhiteSpace(fileName) ? "instagram" : fileName);
        if (string.IsNullOrWhiteSpace(baseName)) baseName = "instagram";
        await using var stream = new MemoryStream(content, writable: false);
        var saved = await _files.UploadAsync(new FileUploadRequest
        {
            WebsiteID = websiteId,
            StorageSettingID = storageId,
            Content = stream,
            OriginalFileName = baseName + ".jpg",
            MimeType = "image/jpeg",
            Title = "Instagram temporary",
            Crop = crop,
            ApplyWatermark = false,
            KeepOriginalFormat = true,
        }, ct);

        if (string.IsNullOrWhiteSpace(saved.CNDUrl))
            throw new InstagramException("Storage did not return a public address for the photo.");
        _hosted[saved.CNDUrl] = saved.FileRecordID;
        return saved.CNDUrl;
    }

    public async Task<string> HostVideoAsync(int websiteId, byte[] content, string fileName, string? mime, CancellationToken ct)
    {
        if (content.Length == 0 || content.Length > MaxVideoBytes)
            throw new InstagramException("A video file must be under 50 MB, or paste a public https link instead.");

        var storageId = await ActiveStorageIdAsync(websiteId, ct);
        var name = string.IsNullOrWhiteSpace(fileName) ? "instagram.mp4" : fileName;
        await using var stream = new MemoryStream(content, writable: false);
        var saved = await _files.UploadAsync(new FileUploadRequest
        {
            WebsiteID = websiteId,
            StorageSettingID = storageId,
            Content = stream,
            OriginalFileName = name,
            MimeType = string.IsNullOrWhiteSpace(mime) ? "video/mp4" : mime,
            Title = "Instagram temporary",
            ApplyWatermark = false,
        }, ct);

        if (string.IsNullOrWhiteSpace(saved.CNDUrl))
            throw new InstagramException("Storage did not return a public address for the video.");
        _hosted[saved.CNDUrl] = saved.FileRecordID;
        return saved.CNDUrl;
    }

    public async Task<byte[]> DownloadAsync(string url, CancellationToken ct)
    {
        if (!InstagramUrls.IsPublicHttps(url))
            throw new InstagramException("Instagram can only fetch a public https address.");

        var client = _http.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(30);
        using var response = await client.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode)
            throw new InstagramException("Could not read that photo in order to crop it.");
        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        if (bytes.Length == 0 || bytes.Length > MaxImageBytes)
            throw new InstagramException("Each photo must be under 8 MB.");
        return bytes;
    }

    public async Task DeleteHostedAsync(IReadOnlyCollection<string> urls, CancellationToken ct)
    {
        foreach (var url in urls.Distinct(StringComparer.Ordinal))
        {
            if (!_hosted.TryRemove(url, out var id)) continue;
            try
            {
                await _files.SoftDeleteAsync(id, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // The Instagram post is already recorded. A leftover row is titled "Instagram temporary".
            }
        }
    }

    private async Task<int> ActiveStorageIdAsync(int websiteId, CancellationToken ct)
    {
        await using var context = await _contexts.CreateDbContextAsync(ct);
        var id = await context.WebsiteStorageSettings.AsNoTracking()
            .Where(s => s.WebsiteID == websiteId && s.Active)
            .OrderBy(s => s.WebsiteStorageSettingsID)
            .Select(s => (int?)s.WebsiteStorageSettingsID)
            .FirstOrDefaultAsync(ct);
        return id ?? throw new InstagramException("Add an active storage under Media before uploading a file. A public link does not need storage.");
    }
}

public static class InstagramUrls
{
    public static string? Absolute(string? url, string? websiteAddress)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        if (Uri.TryCreate(url.Trim(), UriKind.Absolute, out var absolute))
            return absolute.ToString();
        if (string.IsNullOrWhiteSpace(websiteAddress)) return null;

        var root = websiteAddress.Trim();
        if (!root.Contains("://", StringComparison.Ordinal)) root = "https://" + root;
        return Uri.TryCreate(root.TrimEnd('/') + "/" + url.Trim().TrimStart('/'), UriKind.Absolute, out var combined)
            ? combined.ToString()
            : null;
    }

    public static bool IsPublicHttps(string? url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) return false;
        if (uri.IsLoopback) return false;
        var host = uri.IdnHost;
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || host.EndsWith(".local", StringComparison.OrdinalIgnoreCase))
            return false;
        if (!IPAddress.TryParse(host, out var ip)) return true;
        if (IPAddress.IsLoopback(ip)) return false;
        if (ip.AddressFamily != AddressFamily.InterNetwork) return !ip.IsIPv6LinkLocal;
        var bytes = ip.GetAddressBytes();
        if (bytes[0] == 10 || bytes[0] == 127) return false;
        if (bytes[0] == 192 && bytes[1] == 168) return false;
        if (bytes[0] == 172 && bytes[1] is >= 16 and <= 31) return false;
        if (bytes[0] == 169 && bytes[1] == 254) return false;
        return true;
    }
}
