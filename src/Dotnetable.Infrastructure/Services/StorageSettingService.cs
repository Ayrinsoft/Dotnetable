using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class StorageSettingService : IStorageSettingService
{
    private readonly AppDbContext _context;
    private readonly IFileStorageProviderRegistry _providers;

    public StorageSettingService(AppDbContext context, IFileStorageProviderRegistry providers)
    {
        _context = context;
        _providers = providers;
    }

    public async Task<IReadOnlyList<WebstieStorageSetting>> GetForWebsiteAsync(int websiteId, CancellationToken ct = default) =>
        await _context.WebstieStorageSettings.AsNoTracking()
            .Where(s => s.WebsiteID == websiteId)
            .OrderBy(s => s.WebsiteStorageSettingsID)
            .ToListAsync(ct);

    public async Task<WebstieStorageSetting?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.WebstieStorageSettings.FindAsync([id], ct);

    public async Task<IReadOnlyList<StorageSettingInfo>> GetActiveForWebsiteAsync(int websiteId, CancellationToken ct = default)
    {
        var settings = await _context.WebstieStorageSettings.AsNoTracking()
            .Where(s => s.WebsiteID == websiteId && s.Active)
            .OrderBy(s => s.WebsiteStorageSettingsID)
            .ToListAsync(ct);

        var result = new List<StorageSettingInfo>(settings.Count);
        foreach (var s in settings)
            result.Add(await ToInfoAsync(s, ct));
        return result;
    }

    public async Task<StorageQuota> GetQuotaAsync(int id, CancellationToken ct = default)
    {
        var setting = await _context.WebstieStorageSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.WebsiteStorageSettingsID == id, ct)
            ?? throw new InvalidOperationException($"Storage setting {id} not found.");
        return await ResolveQuotaAsync(setting, ct);
    }

    public async Task<WebstieStorageSetting> CreateAsync(WebstieStorageSetting setting, CancellationToken ct = default)
    {
        _context.WebstieStorageSettings.Add(setting);
        await _context.SaveChangesAsync(ct);
        return setting;
    }

    public async Task UpdateAsync(WebstieStorageSetting setting, CancellationToken ct = default)
    {
        _context.WebstieStorageSettings.Update(setting);
        await _context.SaveChangesAsync(ct);
    }

    public async Task SetActiveAsync(int id, bool active, CancellationToken ct = default) =>
        await _context.WebstieStorageSettings.Where(s => s.WebsiteStorageSettingsID == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Active, active), ct);

    public async Task DeleteAsync(int id, CancellationToken ct = default) =>
        await _context.WebstieStorageSettings.Where(s => s.WebsiteStorageSettingsID == id)
            .ExecuteDeleteAsync(ct);

    public async Task<bool> TestAsync(int id, CancellationToken ct = default)
    {
        var setting = await GetByIdAsync(id, ct);
        return setting is not null && await TestAsync(ToContext(setting), ct);
    }

    public async Task<bool> TestAsync(StorageSettingContext ctx, CancellationToken ct = default) =>
        await _providers.Get(ctx.Provider).TestConnectionAsync(ctx, ct);

    /// <summary>Live quota: provider value, falling back to summed DB usage when the backend reports none.</summary>
    private async Task<StorageQuota> ResolveQuotaAsync(WebstieStorageSetting setting, CancellationToken ct)
    {
        StorageQuota providerQuota;
        try
        {
            providerQuota = await _providers.Get((StorageProviderType)setting.StorageProvider)
                .GetQuotaAsync(ToContext(setting), ct);
        }
        catch
        {
            providerQuota = new StorageQuota { UsedKB = 0, TotalKB = null };
        }

        var usedKb = providerQuota.UsedKB;
        if (usedKb <= 0)
        {
            usedKb = await _context.FileRecords.AsNoTracking()
                .Where(f => f.WebsiteStorageSettingsID == setting.WebsiteStorageSettingsID && !f.IsDeleted)
                .SumAsync(f => (long?)f.FileSizeKB, ct) ?? 0;
        }

        return new StorageQuota { UsedKB = usedKb, TotalKB = providerQuota.TotalKB };
    }

    private async Task<StorageSettingInfo> ToInfoAsync(WebstieStorageSetting s, CancellationToken ct)
    {
        var provider = (StorageProviderType)s.StorageProvider;
        StorageQuota? quota = null;
        try { quota = await ResolveQuotaAsync(s, ct); } catch { /* surface as null quota */ }

        return new StorageSettingInfo
        {
            WebsiteStorageSettingsID = s.WebsiteStorageSettingsID,
            WebsiteID = s.WebsiteID,
            Provider = provider,
            Label = provider.ToString(),
            Active = s.Active,
            MaxFileSizeKB = s.MaxFileSizeKB,
            AllowedExtensions = s.AllowedExtensions,
            AutoGenerateThumbnails = s.AutoGenerateThumbnails,
            Quota = quota,
        };
    }

    private static StorageSettingContext ToContext(WebstieStorageSetting s) => new()
    {
        WebsiteStorageSettingsID = s.WebsiteStorageSettingsID,
        WebsiteID = s.WebsiteID,
        Provider = (StorageProviderType)s.StorageProvider,
        SettingsJson = s.StorageSettingsJSON,
        MaxFileSizeKB = s.MaxFileSizeKB,
        AllowedExtensions = s.AllowedExtensions,
        AutoGenerateThumbnails = s.AutoGenerateThumbnails,
    };
}
