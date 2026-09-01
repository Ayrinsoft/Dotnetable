using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.Sms;

/// <summary>Resolves a registered <see cref="ISmsProvider"/> by its key.</summary>
public sealed class SmsProviderRegistry : ISmsProviderRegistry
{
    private readonly IReadOnlyList<ISmsProvider> _providers;

    public SmsProviderRegistry(IEnumerable<ISmsProvider> providers) => _providers = providers.ToList();

    public IReadOnlyList<ISmsProvider> All => _providers;

    public ISmsProvider? Find(string? key) =>
        string.IsNullOrWhiteSpace(key)
            ? null
            : _providers.FirstOrDefault(p => string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase));
}

/// <summary>
/// The website-scoped SMS sender. Picks the site's active gateway (lowest sort order that is both
/// enabled and fully configured) and hands the message to its provider.
///
/// <para>The previous implementation was a no-op that logged the message and returned — which meant
/// an OTP was written to the application log in plaintext and the customer was told a code had been
/// sent that never existed. Nothing is logged here beyond the destination and the outcome; message
/// bodies carry one-time codes and must not reach the log.</para>
/// </summary>
public sealed class SmsSender : ISmsSender
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ISmsProviderRegistry _registry;
    private readonly ILogger<SmsSender> _logger;

    public SmsSender(
        IDbContextFactory<AppDbContext> contextFactory,
        ISmsProviderRegistry registry,
        ILogger<SmsSender> logger)
    {
        _contextFactory = contextFactory;
        _registry = registry;
        _logger = logger;
    }

    public async Task<bool> IsConfiguredAsync(int websiteId, CancellationToken ct = default) =>
        await ResolveAsync(websiteId, ct) is not null;

    public async Task<bool> SendAsync(int websiteId, string countryCode, string cellphone, string message,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cellphone))
            return false;

        var resolved = await ResolveAsync(websiteId, ct);
        if (resolved is null)
        {
            _logger.LogWarning("No SMS gateway is configured for website {WebsiteId}; message not sent.", websiteId);
            return false;
        }

        var (provider, ctx) = resolved.Value;
        var result = await provider.SendAsync(ctx, countryCode, cellphone, message, ct);
        if (!result.Success)
        {
            _logger.LogError("SMS via {Provider} for website {WebsiteId} failed: {Error}",
                provider.Key, websiteId, result.Error);
            return false;
        }

        _logger.LogInformation("SMS sent via {Provider} for website {WebsiteId}.", provider.Key, websiteId);
        return true;
    }

    /// <summary>The first active, registered and fully-configured gateway for the website.</summary>
    private async Task<(ISmsProvider Provider, SmsSettingContext Context)?> ResolveAsync(int websiteId, CancellationToken ct)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var rows = await context.WebsiteSmsSettings.AsNoTracking()
            .Where(s => s.WebsiteID == websiteId && s.IsActive)
            .OrderBy(s => s.SortOrder).ThenBy(s => s.WebsiteSmsSettingID)
            .ToListAsync(ct);

        foreach (var row in rows)
        {
            var provider = _registry.Find(row.Provider);
            if (provider is null) continue;

            var ctx = ToContext(row);
            if (provider.IsConfigured(ctx)) return (provider, ctx);
        }

        return null;
    }

    internal static SmsSettingContext ToContext(WebsiteSmsSetting row) => new()
    {
        WebsiteSmsSettingID = row.WebsiteSmsSettingID,
        WebsiteID = row.WebsiteID,
        Provider = row.Provider,
        SettingsJson = string.IsNullOrWhiteSpace(row.SettingsJSON) ? "{}" : row.SettingsJSON,
        SenderNumber = row.SenderNumber,
    };
}

/// <summary>Admin-side CRUD over a website's registered SMS gateways.</summary>
public sealed class SmsSettingService : ISmsSettingService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ISmsProviderRegistry _registry;

    public SmsSettingService(IDbContextFactory<AppDbContext> contextFactory, ISmsProviderRegistry registry)
    {
        _contextFactory = contextFactory;
        _registry = registry;
    }

    public async Task<IReadOnlyList<SmsSettingInfo>> GetForWebsiteAsync(int websiteId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var rows = await context.WebsiteSmsSettings.AsNoTracking()
            .Where(s => s.WebsiteID == websiteId)
            .OrderBy(s => s.SortOrder).ThenBy(s => s.WebsiteSmsSettingID)
            .ToListAsync(ct);
        return rows.Select(ToInfo).ToList();
    }

    public async Task<IReadOnlyList<SmsSettingInfo>> GetActiveForWebsiteAsync(int websiteId, CancellationToken ct = default) =>
        (await GetForWebsiteAsync(websiteId, ct)).Where(s => s.IsActive).ToList();

    public async Task<SmsSettingInfo?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var row = await context.WebsiteSmsSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.WebsiteSmsSettingID == id, ct);
        return row is null ? null : ToInfo(row);
    }

    public async Task<int> CreateAsync(SmsSettingInput input, CancellationToken ct = default)
    {
        if (_registry.Find(input.Provider) is null)
            throw new InvalidOperationException($"Unknown SMS provider '{input.Provider}'.");

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var row = new WebsiteSmsSetting
        {
            WebsiteID = input.WebsiteID,
            Provider = input.Provider,
            Title = input.Title,
            SettingsJSON = string.IsNullOrWhiteSpace(input.SettingsJSON) ? "{}" : input.SettingsJSON,
            SenderNumber = input.SenderNumber,
            IsActive = input.IsActive,
            SortOrder = input.SortOrder,
            CreatedAt = DateTime.UtcNow,
        };
        context.WebsiteSmsSettings.Add(row);
        await context.SaveChangesAsync(ct);
        return row.WebsiteSmsSettingID;
    }

    public async Task UpdateAsync(int id, SmsSettingInput input, CancellationToken ct = default)
    {
        if (_registry.Find(input.Provider) is null)
            throw new InvalidOperationException($"Unknown SMS provider '{input.Provider}'.");

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var row = await context.WebsiteSmsSettings.FirstOrDefaultAsync(s => s.WebsiteSmsSettingID == id, ct)
            ?? throw new InvalidOperationException("SMS gateway not found.");

        row.Provider = input.Provider;
        row.Title = input.Title;
        row.SettingsJSON = string.IsNullOrWhiteSpace(input.SettingsJSON) ? "{}" : input.SettingsJSON;
        row.SenderNumber = input.SenderNumber;
        row.IsActive = input.IsActive;
        row.SortOrder = input.SortOrder;
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        await context.WebsiteSmsSettings.Where(s => s.WebsiteSmsSettingID == id).ExecuteDeleteAsync(ct);
    }

    public async Task<SmsSendResult> TestAsync(int id, string countryCode, string cellphone, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var row = await context.WebsiteSmsSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.WebsiteSmsSettingID == id, ct);
        if (row is null) return SmsSendResult.Fail("SMS gateway not found.");

        var provider = _registry.Find(row.Provider);
        if (provider is null) return SmsSendResult.Fail($"Unknown SMS provider '{row.Provider}'.");

        return await provider.SendAsync(SmsSender.ToContext(row), countryCode, cellphone,
            "Dotnetable test message.", ct);
    }

    private SmsSettingInfo ToInfo(WebsiteSmsSetting row)
    {
        var provider = _registry.Find(row.Provider);
        return new SmsSettingInfo
        {
            WebsiteSmsSettingID = row.WebsiteSmsSettingID,
            WebsiteID = row.WebsiteID,
            Provider = row.Provider,
            ProviderDisplayName = provider?.DisplayName ?? row.Provider,
            Title = row.Title,
            SenderNumber = row.SenderNumber,
            IsActive = row.IsActive,
            SortOrder = row.SortOrder,
            IsConfigured = provider?.IsConfigured(SmsSender.ToContext(row)) ?? false,
            CreatedAt = row.CreatedAt,
        };
    }
}
