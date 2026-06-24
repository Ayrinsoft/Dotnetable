using System.Text.Json;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Enums;
using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Sharing;

namespace Dotnetable.Infrastructure.Storage;

/// <summary>Dropbox storage backend.</summary>
public sealed class DropboxStorageProvider : IFileStorageProvider
{
    public StorageProviderType Provider => StorageProviderType.Dropbox;

    private static DropboxStorageSettings Parse(StorageSettingContext ctx) =>
        JsonSerializer.Deserialize<DropboxStorageSettings>(ctx.SettingsJson) ?? new DropboxStorageSettings();

    private static DropboxClient BuildClient(DropboxStorageSettings s) =>
        !string.IsNullOrWhiteSpace(s.RefreshToken) && !string.IsNullOrWhiteSpace(s.AppKey) && !string.IsNullOrWhiteSpace(s.AppSecret)
            ? new DropboxClient(s.RefreshToken, s.AppKey, s.AppSecret)
            : new DropboxClient(s.AccessToken);

    private static string BuildPath(DropboxStorageSettings s, string storedName)
    {
        var root = (s.RootFolder ?? "").Trim().Trim('/');
        return root.Length == 0 ? $"/{storedName}" : $"/{root}/{storedName}";
    }

    /// <summary>Turns a Dropbox share URL into a direct-content URL.</summary>
    private static string ToDirectUrl(string sharedUrl) =>
        sharedUrl.Replace("www.dropbox.com", "dl.dropboxusercontent.com")
                 .Replace("?dl=0", "")
                 .Replace("&dl=0", "");

    public async Task<StorageUploadResult> UploadAsync(StorageSettingContext ctx, Stream data, string storedName,
        string mimeType, CancellationToken ct = default)
    {
        var s = Parse(ctx);
        using var client = BuildClient(s);
        var path = BuildPath(s, storedName);

        var meta = await client.Files.UploadAsync(path, WriteMode.Overwrite.Instance, body: data);

        string? url = null;
        try
        {
            var link = await client.Sharing.CreateSharedLinkWithSettingsAsync(path);
            url = ToDirectUrl(link.Url);
        }
        catch (ApiException<CreateSharedLinkWithSettingsError> ex) when (ex.ErrorResponse.IsSharedLinkAlreadyExists)
        {
            var existing = await client.Sharing.ListSharedLinksAsync(path, directOnly: true);
            if (existing.Links.Count > 0) url = ToDirectUrl(existing.Links[0].Url);
        }

        return new StorageUploadResult
        {
            StoragePath = path,
            CdnUrl = url,
            CdnFileCode = meta.AsFile?.Id,
        };
    }

    public async Task DeleteAsync(StorageSettingContext ctx, string storedName, CancellationToken ct = default)
    {
        var s = Parse(ctx);
        using var client = BuildClient(s);
        // storedName here is the full Dropbox path saved at upload time.
        var path = storedName.StartsWith('/') ? storedName : BuildPath(s, storedName);
        await client.Files.DeleteV2Async(path);
    }

    public async Task<StorageQuota> GetQuotaAsync(StorageSettingContext ctx, CancellationToken ct = default)
    {
        var s = Parse(ctx);
        using var client = BuildClient(s);
        var usage = await client.Users.GetSpaceUsageAsync();

        long? totalKb = null;
        if (usage.Allocation.IsIndividual)
            totalKb = (long)(usage.Allocation.AsIndividual.Value.Allocated / 1024);
        else if (usage.Allocation.IsTeam)
            totalKb = (long)(usage.Allocation.AsTeam.Value.Allocated / 1024);

        return new StorageQuota { UsedKB = (long)(usage.Used / 1024), TotalKB = totalKb };
    }

    public async Task<bool> TestConnectionAsync(StorageSettingContext ctx, CancellationToken ct = default)
    {
        try
        {
            var s = Parse(ctx);
            using var client = BuildClient(s);
            await client.Users.GetCurrentAccountAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
