using System.Text.Json;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Infrastructure.Storage;

/// <summary>
/// Stores files on the API host's local disk under a per-website folder. Objects are reached through the
/// API download endpoint (see <c>FilesController</c>); the public URL points at that endpoint.
/// </summary>
public sealed class LocalStorageProvider : IFileStorageProvider
{
    private readonly string _contentRoot;

    public LocalStorageProvider(string contentRoot) => _contentRoot = contentRoot;

    public StorageProviderType Provider => StorageProviderType.LocalHost;

    public static LocalStorageSettings Parse(string settingsJson) =>
        JsonSerializer.Deserialize<LocalStorageSettings>(settingsJson) ?? new LocalStorageSettings();

    /// <summary>Resolves the absolute root directory holding all uploads for this storage.</summary>
    public static string ResolveRoot(string contentRoot, LocalStorageSettings s)
    {
        var basePath = string.IsNullOrWhiteSpace(s.BasePath) ? "App_Data/uploads" : s.BasePath;
        return Path.IsPathRooted(basePath) ? basePath : Path.Combine(contentRoot, basePath);
    }

    private string FullPath(LocalStorageSettings s, int websiteId, string storedName)
    {
        var root = ResolveRoot(_contentRoot, s);
        return Path.Combine(root, websiteId.ToString(), storedName);
    }

    public async Task<StorageUploadResult> UploadAsync(StorageSettingContext ctx, Stream data, string storedName,
        string mimeType, CancellationToken ct = default)
    {
        var s = Parse(ctx.SettingsJson);
        var full = FullPath(s, ctx.WebsiteID, storedName);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);

        await using (var fs = new FileStream(full, FileMode.Create, FileAccess.Write))
            await data.CopyToAsync(fs, ct);

        var relPath = $"{ctx.WebsiteID}/{storedName}";
        var baseUrl = s.PublicBaseUrl.TrimEnd('/');
        return new StorageUploadResult
        {
            StoragePath = relPath,
            CdnUrl = string.IsNullOrEmpty(baseUrl) ? $"/api/files/{relPath}" : $"{baseUrl}/{relPath}",
            CdnFileCode = storedName,
        };
    }

    public Task DeleteAsync(StorageSettingContext ctx, string storedName, CancellationToken ct = default)
    {
        var s = Parse(ctx.SettingsJson);
        // storedName may be a bare name or a "{websiteId}/{name}" relative path saved at upload time.
        var name = storedName.Contains('/') ? Path.GetFileName(storedName) : storedName;
        var full = FullPath(s, ctx.WebsiteID, name);
        if (File.Exists(full)) File.Delete(full);
        return Task.CompletedTask;
    }

    public Task<StorageQuota> GetQuotaAsync(StorageSettingContext ctx, CancellationToken ct = default)
    {
        // Disk usage is computed from our DB; total is the admin-declared capacity.
        var s = Parse(ctx.SettingsJson);
        return Task.FromResult(new StorageQuota { UsedKB = 0, TotalKB = s.CapacityKB });
    }

    public Task<bool> TestConnectionAsync(StorageSettingContext ctx, CancellationToken ct = default)
    {
        try
        {
            var s = Parse(ctx.SettingsJson);
            var dir = Path.Combine(ResolveRoot(_contentRoot, s), ctx.WebsiteID.ToString());
            Directory.CreateDirectory(dir);
            var probe = Path.Combine(dir, $".write-test-{Guid.NewGuid():N}");
            File.WriteAllText(probe, "ok");
            File.Delete(probe);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
