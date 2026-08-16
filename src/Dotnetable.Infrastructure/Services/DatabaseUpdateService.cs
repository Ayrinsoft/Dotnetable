using Dotnetable.Application.Interfaces;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dotnetable.Infrastructure.Services;

public class DatabaseUpdateService : IDatabaseUpdateService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public DatabaseUpdateService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<string>> GetAppliedUpdatesAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return (await context.Database.GetAppliedMigrationsAsync(ct)).ToList();
    }

    public async Task<IReadOnlyList<string>> GetPendingUpdatesAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        await BaselineLegacySchemaIfNeededAsync(context, ct);
        return (await context.Database.GetPendingMigrationsAsync(ct)).ToList();
    }

    public async Task ApplyUpdatesAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        // Existing installs often have the InitialCreate schema without __EFMigrationsHistory.
        // Baseline that migration when core tables already exist, then apply only real deltas.
        await BaselineLegacySchemaIfNeededAsync(context, ct);
        await context.Database.MigrateAsync(ct);
    }

    /// <summary>
    /// When the database was created before EF history existed (or history was wiped),
    /// <c>InitialCreate</c> is still "pending" and re-running it fails with "object already exists".
    /// Mark already-present schema migrations as applied so later migrations can run.
    /// </summary>
    internal static async Task BaselineLegacySchemaIfNeededAsync(AppDbContext context, CancellationToken ct)
    {
        if (!await context.Database.CanConnectAsync(ct))
            return;

        var pending = (await context.Database.GetPendingMigrationsAsync(ct)).ToList();
        if (pending.Count == 0) return;

        var history = context.GetService<IHistoryRepository>();
        // Ensure history table exists (provider-specific script).
        var createHistory = history.GetCreateIfNotExistsScript();
        if (!string.IsNullOrWhiteSpace(createHistory))
            await context.Database.ExecuteSqlRawAsync(createHistory, ct);

        var productVersion = ProductVersion(context);

        // Squashed InitialCreate: existing databases already have tables; only record history.
        var initial = pending.FirstOrDefault(m =>
            m.Contains("InitialCreate", StringComparison.OrdinalIgnoreCase));
        if (initial is not null
            && (await TableExistsAsync(context, "Websites", ct) || await TableExistsAsync(context, "Countries", ct)))
        {
            await MarkAppliedAsync(context, history, initial, productVersion, ct);
        }
    }

    private static async Task MarkAppliedAsync(
        AppDbContext context,
        IHistoryRepository history,
        string migrationId,
        string productVersion,
        CancellationToken ct)
    {
        var applied = await context.Database.GetAppliedMigrationsAsync(ct);
        if (applied.Contains(migrationId)) return;

        var insert = history.GetInsertScript(new HistoryRow(migrationId, productVersion));
        if (!string.IsNullOrWhiteSpace(insert))
            await context.Database.ExecuteSqlRawAsync(insert, ct);
    }

    private static string ProductVersion(AppDbContext context)
    {
        try
        {
            var assembly = typeof(Microsoft.EntityFrameworkCore.DbContext).Assembly.GetName().Version;
            return assembly is null ? "10.0.0" : $"{assembly.Major}.{assembly.Minor}.{assembly.Build}";
        }
        catch
        {
            return "10.0.0";
        }
    }

    private static async Task<bool> TableExistsAsync(AppDbContext context, string table, CancellationToken ct)
    {
        try
        {
            var conn = context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_NAME = @t
                ) THEN 1 ELSE 0 END
                """;
            var p = cmd.CreateParameter();
            p.ParameterName = "@t";
            p.Value = table;
            cmd.Parameters.Add(p);
            var result = await cmd.ExecuteScalarAsync(ct);
            return Convert.ToInt32(result) == 1;
        }
        catch
        {
            return false;
        }
    }
}
