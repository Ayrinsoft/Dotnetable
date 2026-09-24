using Dotnetable.Application;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Messaging;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dotnetable.Infrastructure.WhatsApp;

/// <summary>Resolves a registered <see cref="IWhatsAppProvider"/> by its key.</summary>
public sealed class WhatsAppProviderRegistry : IWhatsAppProviderRegistry
{
    private readonly IReadOnlyList<IWhatsAppProvider> _providers;

    public WhatsAppProviderRegistry(IEnumerable<IWhatsAppProvider> providers) => _providers = providers.ToList();

    public IReadOnlyList<IWhatsAppProvider> All => _providers;

    public IWhatsAppProvider? Find(string? key) =>
        string.IsNullOrWhiteSpace(key)
            ? null
            : _providers.FirstOrDefault(p => string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase));
}

/// <summary>
/// The website-scoped WhatsApp sender. Picks the site's active gateway (lowest sort order that is both
/// enabled and fully configured) and, when the site has none, the master website's — the same fallback
/// as email and the opposite of SMS (see <see cref="IWhatsAppSender"/>). Every attempt is recorded in
/// the message log; bodies never reach the application log.
/// </summary>
public sealed class WhatsAppSender : IWhatsAppSender
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IWhatsAppProviderRegistry _registry;
    private readonly IMessageLogService _log;
    private readonly ILogger<WhatsAppSender> _logger;

    public WhatsAppSender(
        IDbContextFactory<AppDbContext> contextFactory,
        IWhatsAppProviderRegistry registry,
        IMessageLogService log,
        ILogger<WhatsAppSender> logger)
    {
        _contextFactory = contextFactory;
        _registry = registry;
        _log = log;
        _logger = logger;
    }

    public async Task<bool> IsConfiguredAsync(int websiteId, CancellationToken ct = default) =>
        await ResolveAsync(websiteId, ct) is not null;

    public async Task<bool> SendAsync(int websiteId, string countryCode, string cellphone, string message,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cellphone))
            return false;

        var recipient = FormatNumber(countryCode, cellphone);
        var resolved = await ResolveAsync(websiteId, ct);
        if (resolved is null)
        {
            _logger.LogWarning("No WhatsApp gateway is configured for website {WebsiteId}; message not sent.", websiteId);
            await _log.WriteAsync(new MessageLogEntry
            {
                WebsiteID = websiteId, Channel = MessageChannel.WhatsApp, Success = false,
                Recipient = recipient, Body = message, Error = "No WhatsApp gateway is configured.",
            }, ct);
            return false;
        }

        var (provider, ctx) = resolved.Value;
        var result = await provider.SendAsync(ctx, countryCode, cellphone, message, ct);

        await _log.WriteAsync(new MessageLogEntry
        {
            WebsiteID = websiteId, Channel = MessageChannel.WhatsApp, Success = result.Success,
            Recipient = recipient, Body = message, Provider = provider.DisplayName, Error = result.Error,
        }, ct);

        if (!result.Success)
        {
            _logger.LogError("WhatsApp via {Provider} for website {WebsiteId} failed: {Error}",
                provider.Key, websiteId, result.Error);
            return false;
        }

        _logger.LogInformation("WhatsApp sent via {Provider} for website {WebsiteId}.", provider.Key, websiteId);
        return true;
    }

    /// <summary>The website's first usable gateway, else the master website's.</summary>
    private async Task<(IWhatsAppProvider Provider, WhatsAppSettingContext Context)?> ResolveAsync(int websiteId, CancellationToken ct)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var rows = await _context.WebsiteWhatsAppSettings.AsNoTracking()
            .Where(s => s.IsActive && (s.WebsiteID == websiteId || s.WebsiteID == AppConstants.MasterWebsiteId))
            .OrderBy(s => s.SortOrder).ThenBy(s => s.WebsiteWhatsAppSettingID)
            .ToListAsync(ct);

        return Pick(rows.Where(r => r.WebsiteID == websiteId))
            ?? Pick(rows.Where(r => r.WebsiteID == AppConstants.MasterWebsiteId));
    }

    private (IWhatsAppProvider, WhatsAppSettingContext)? Pick(IEnumerable<WebsiteWhatsAppSetting> rows)
    {
        foreach (var row in rows)
        {
            var provider = _registry.Find(row.Provider);
            if (provider is null) continue;

            var ctx = ToContext(row);
            if (provider.IsConfigured(ctx)) return (provider, ctx);
        }
        return null;
    }

    internal static string FormatNumber(string? countryCode, string cellphone)
    {
        var cc = new string((countryCode ?? "").Where(char.IsDigit).ToArray());
        return cc.Length == 0 ? cellphone.Trim() : $"+{cc} {cellphone.Trim()}";
    }

    internal static WhatsAppSettingContext ToContext(WebsiteWhatsAppSetting row) => new()
    {
        WebsiteWhatsAppSettingID = row.WebsiteWhatsAppSettingID,
        WebsiteID = row.WebsiteID,
        Provider = row.Provider,
        SettingsJson = string.IsNullOrWhiteSpace(row.SettingsJSON) ? "{}" : row.SettingsJSON,
        SenderNumber = row.SenderNumber,
    };
}

/// <summary>Admin-side CRUD over a website's registered WhatsApp gateways.</summary>
public sealed class WhatsAppSettingService : IWhatsAppSettingService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IWhatsAppProviderRegistry _registry;
    private readonly IMessageLogService _log;

    public WhatsAppSettingService(IDbContextFactory<AppDbContext> contextFactory, IWhatsAppProviderRegistry registry,
        IMessageLogService log)
    {
        _contextFactory = contextFactory;
        _registry = registry;
        _log = log;
    }

    public async Task<IReadOnlyList<WhatsAppSettingInfo>> GetForWebsiteAsync(int websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        var rows = await _context.WebsiteWhatsAppSettings.AsNoTracking()
            .Where(s => s.WebsiteID == websiteId)
            .OrderBy(s => s.SortOrder).ThenBy(s => s.WebsiteWhatsAppSettingID)
            .ToListAsync(ct);
        return rows.Select(ToInfo).ToList();
    }

    public async Task<WhatsAppSettingInput?> GetInputAsync(int id, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        var row = await _context.WebsiteWhatsAppSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.WebsiteWhatsAppSettingID == id, ct);
        return row is null ? null : new WhatsAppSettingInput
        {
            WebsiteID = row.WebsiteID,
            Provider = row.Provider,
            Title = row.Title,
            SettingsJSON = row.SettingsJSON,
            SenderNumber = row.SenderNumber,
            IsActive = row.IsActive,
            SortOrder = row.SortOrder,
        };
    }

    public async Task<int> CreateAsync(WhatsAppSettingInput input, CancellationToken ct = default)
    {
        if (_registry.Find(input.Provider) is null)
            throw new InvalidOperationException($"Unknown WhatsApp provider '{input.Provider}'.");

        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        var row = new WebsiteWhatsAppSetting
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
        _context.WebsiteWhatsAppSettings.Add(row);
        await _context.SaveChangesAsync(ct);
        return row.WebsiteWhatsAppSettingID;
    }

    public async Task UpdateAsync(int id, WhatsAppSettingInput input, CancellationToken ct = default)
    {
        if (_registry.Find(input.Provider) is null)
            throw new InvalidOperationException($"Unknown WhatsApp provider '{input.Provider}'.");

        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        var row = await _context.WebsiteWhatsAppSettings.FirstOrDefaultAsync(s => s.WebsiteWhatsAppSettingID == id, ct)
            ?? throw new InvalidOperationException("WhatsApp gateway not found.");

        row.Provider = input.Provider;
        row.Title = input.Title;
        row.SettingsJSON = string.IsNullOrWhiteSpace(input.SettingsJSON) ? "{}" : input.SettingsJSON;
        row.SenderNumber = input.SenderNumber;
        row.IsActive = input.IsActive;
        row.SortOrder = input.SortOrder;
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        await _context.WebsiteWhatsAppSettings.Where(s => s.WebsiteWhatsAppSettingID == id).ExecuteDeleteAsync(ct);
    }

    public async Task<SmsSendResult> TestAsync(int id, string countryCode, string cellphone, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        var row = await _context.WebsiteWhatsAppSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.WebsiteWhatsAppSettingID == id, ct);
        if (row is null) return SmsSendResult.Fail("WhatsApp gateway not found.");

        var provider = _registry.Find(row.Provider);
        if (provider is null) return SmsSendResult.Fail($"Unknown WhatsApp provider '{row.Provider}'.");

        const string text = "Dotnetable test message.";
        using var scope = MessageLogScope.Begin(MessageLogSources.Test);
        var result = await provider.SendAsync(WhatsAppSender.ToContext(row), countryCode, cellphone, text, ct);
        await _log.WriteAsync(new MessageLogEntry
        {
            WebsiteID = row.WebsiteID, Channel = MessageChannel.WhatsApp, Success = result.Success,
            Recipient = WhatsAppSender.FormatNumber(countryCode, cellphone), Body = text,
            Provider = provider.DisplayName, Error = result.Error,
        }, ct);
        return result;
    }

    private WhatsAppSettingInfo ToInfo(WebsiteWhatsAppSetting row)
    {
        var provider = _registry.Find(row.Provider);
        return new WhatsAppSettingInfo
        {
            WebsiteWhatsAppSettingID = row.WebsiteWhatsAppSettingID,
            WebsiteID = row.WebsiteID,
            Provider = row.Provider,
            ProviderDisplayName = provider?.DisplayName ?? row.Provider,
            Title = row.Title,
            SenderNumber = row.SenderNumber,
            IsActive = row.IsActive,
            SortOrder = row.SortOrder,
            IsConfigured = provider?.IsConfigured(WhatsAppSender.ToContext(row)) ?? false,
            CreatedAt = row.CreatedAt,
        };
    }
}
