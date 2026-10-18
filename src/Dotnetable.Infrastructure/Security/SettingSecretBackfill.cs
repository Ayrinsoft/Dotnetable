using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Dotnetable.Application.Interfaces;

namespace Dotnetable.Infrastructure.Security;

/// <summary>
/// Rewrites credential columns that are still plaintext. New saves go through the value converter;
/// rows written before that stay readable (the converter leaves unprefixed text alone) until this
/// pass stores them as <c>enc1.</c> ciphertext. Already-encrypted rows are skipped, so a restart
/// does not re-encrypt them with a new nonce.
/// </summary>
public sealed class SettingSecretBackfillService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDatabaseConfigStore _configStore;
    private readonly ILogger<SettingSecretBackfillService> _logger;

    public SettingSecretBackfillService(
        IServiceScopeFactory scopeFactory,
        IDatabaseConfigStore configStore,
        ILogger<SettingSecretBackfillService> logger)
    {
        _scopeFactory = scopeFactory;
        _configStore = configStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested) return;
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (!_configStore.IsConfigured) return;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            await using var context = await factory.CreateDbContextAsync(stoppingToken);
            if (context.SettingProtectionKey is null) return;
            if (!await context.Database.CanConnectAsync(stoppingToken)) return;

            var count = await SettingSecretBackfill.RunAsync(context, stoppingToken);
            if (count > 0)
                _logger.LogInformation("Encrypted {Count} stored setting secret(s) that were still plaintext.", count);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Encrypting existing setting secrets failed. Plaintext rows stay readable and will be retried on the next start.");
        }
    }
}

public static class SettingSecretBackfill
{
    public static async Task<int> RunAsync(AppDbContext context, CancellationToken ct = default)
    {
        if (context.SettingProtectionKey is null) return 0;

        var count = 0;
        count += await RewriteAsync(context, "WebsiteSmsSettings", "WebsiteSmsSettingID", "SettingsJSON", ct);
        count += await RewriteAsync(context, "WebsiteWhatsAppSettings", "WebsiteWhatsAppSettingID", "SettingsJSON", ct);
        count += await RewriteAsync(context, "MarketplaceChannels", "MarketplaceChannelID", "SettingsJSON", ct);
        count += await RewriteAsync(context, "WebsiteStorageSettings", "WebsiteStorageSettingsID", "StorageSettingsJSON", ct);
        count += await RewriteAsync(context, "EmailAccounts", "EmailAccountID", "Password", ct);
        count += await RewriteAsync(context, "PaymentGateways", "PaymentGatewayID", "MerchantID", ct);
        count += await RewriteAsync(context, "PaymentGateways", "PaymentGatewayID", "ApiKey", ct);
        count += await RewriteAsync(context, "PaymentGateways", "PaymentGatewayID", "ApiSecret", ct);
        count += await RewriteAsync(context, "PaymentGateways", "PaymentGatewayID", "SettingsJSON", ct);
        count += await RewriteAsync(context, "WebsiteCaptchaSettings", "WebsiteCaptchaSettingID", "TurnstileSecretKey", ct);
        return count;
    }

    private static async Task<int> RewriteAsync(
        AppDbContext context,
        string table,
        string idColumn,
        string valueColumn,
        CancellationToken ct)
    {
        List<int> ids;
        try
        {
            ids = await context.Database.SqlQueryRaw<int>(SelectPlaintextIds(context, table, idColumn, valueColumn))
                .ToListAsync(ct);
        }
        catch (Exception)
        {
            // The table is not there yet (database not migrated). The next start retries.
            return 0;
        }

        if (ids.Count == 0) return 0;

        var marked = table switch
        {
            "WebsiteSmsSettings" => await MarkSmsAsync(context, ids, ct),
            "WebsiteWhatsAppSettings" => await MarkWhatsAppAsync(context, ids, ct),
            "MarketplaceChannels" => await MarkMarketplaceAsync(context, ids, valueColumn, ct),
            "WebsiteStorageSettings" => await MarkStorageAsync(context, ids, ct),
            "EmailAccounts" => await MarkEmailAsync(context, ids, ct),
            "PaymentGateways" => await MarkPaymentAsync(context, ids, valueColumn, ct),
            "WebsiteCaptchaSettings" => await MarkCaptchaAsync(context, ids, ct),
            _ => 0,
        };

        if (marked > 0)
            await context.SaveChangesAsync(ct);
        return marked;
    }

    private static string SelectPlaintextIds(AppDbContext context, string table, string idColumn, string valueColumn)
    {
        var provider = context.Database.ProviderName ?? "";
        if (provider.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
            return $"SELECT [{idColumn}] AS [Value] FROM [{table}] WHERE [{valueColumn}] IS NOT NULL AND [{valueColumn}] <> N'' AND [{valueColumn}] NOT LIKE N'{SettingProtector.Prefix}%'";
        if (provider.Contains("MySql", StringComparison.OrdinalIgnoreCase) || provider.Contains("Maria", StringComparison.OrdinalIgnoreCase))
            return $"SELECT `{idColumn}` AS `Value` FROM `{table}` WHERE `{valueColumn}` IS NOT NULL AND `{valueColumn}` <> '' AND `{valueColumn}` NOT LIKE '{SettingProtector.Prefix}%'";
        return $"SELECT \"{idColumn}\" AS \"Value\" FROM \"{table}\" WHERE \"{valueColumn}\" IS NOT NULL AND \"{valueColumn}\" <> '' AND \"{valueColumn}\" NOT LIKE '{SettingProtector.Prefix}%'";
    }

    private static async Task<int> MarkSmsAsync(AppDbContext context, List<int> ids, CancellationToken ct)
    {
        var rows = await context.WebsiteSmsSettings.Where(s => ids.Contains(s.WebsiteSmsSettingID)).ToListAsync(ct);
        foreach (var row in rows)
            context.Entry(row).Property(s => s.SettingsJSON).IsModified = true;
        return rows.Count;
    }

    private static async Task<int> MarkWhatsAppAsync(AppDbContext context, List<int> ids, CancellationToken ct)
    {
        var rows = await context.WebsiteWhatsAppSettings.Where(s => ids.Contains(s.WebsiteWhatsAppSettingID)).ToListAsync(ct);
        foreach (var row in rows)
            context.Entry(row).Property(s => s.SettingsJSON).IsModified = true;
        return rows.Count;
    }

    private static async Task<int> MarkMarketplaceAsync(AppDbContext context, List<int> ids, string column, CancellationToken ct)
    {
        var rows = await context.MarketplaceChannels.Where(s => ids.Contains(s.MarketplaceChannelID)).ToListAsync(ct);
        foreach (var row in rows)
            context.Entry(row).Property(s => s.SettingsJSON).IsModified = true;
        return column == "SettingsJSON" ? rows.Count : 0;
    }

    private static async Task<int> MarkStorageAsync(AppDbContext context, List<int> ids, CancellationToken ct)
    {
        var rows = await context.WebsiteStorageSettings.Where(s => ids.Contains(s.WebsiteStorageSettingsID)).ToListAsync(ct);
        foreach (var row in rows)
            context.Entry(row).Property(s => s.StorageSettingsJSON).IsModified = true;
        return rows.Count;
    }

    private static async Task<int> MarkEmailAsync(AppDbContext context, List<int> ids, CancellationToken ct)
    {
        var rows = await context.EmailAccounts.Where(s => ids.Contains(s.EmailAccountID)).ToListAsync(ct);
        foreach (var row in rows)
            context.Entry(row).Property(s => s.Password).IsModified = true;
        return rows.Count;
    }

    private static async Task<int> MarkPaymentAsync(AppDbContext context, List<int> ids, string column, CancellationToken ct)
    {
        var rows = await context.PaymentGateways.Where(s => ids.Contains(s.PaymentGatewayID)).ToListAsync(ct);
        foreach (var row in rows)
        {
            var entry = context.Entry(row);
            switch (column)
            {
                case "MerchantID": entry.Property(s => s.MerchantID).IsModified = true; break;
                case "ApiKey": entry.Property(s => s.ApiKey).IsModified = true; break;
                case "ApiSecret": entry.Property(s => s.ApiSecret).IsModified = true; break;
                default: entry.Property(s => s.SettingsJSON).IsModified = true; break;
            }
        }
        return rows.Count;
    }

    private static async Task<int> MarkCaptchaAsync(AppDbContext context, List<int> ids, CancellationToken ct)
    {
        var rows = await context.WebsiteCaptchaSettings.Where(s => ids.Contains(s.WebsiteCaptchaSettingID)).ToListAsync(ct);
        foreach (var row in rows)
            context.Entry(row).Property(s => s.TurnstileSecretKey).IsModified = true;
        return rows.Count;
    }
}
