using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Dotnetable.Infrastructure.Provisioning;
using Dotnetable.Infrastructure.Services;
using Dotnetable.Migrations.Sqlite;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dotnetable.Tests.Services;

public class SqliteMigrationTests
{
    [Fact]
    public async Task Upgrading_keeps_existing_decimal_rows_and_a_second_apply_is_a_noop()
    {
        var path = Path.Combine(Path.GetTempPath(), "dotnetable-sqlite-" + Guid.NewGuid().ToString("N") + ".sqlite");
        try
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = path,
                ForeignKeys = true,
            }.ToString();

            const decimal original = 10.123456m;
            await using (var db = Create(connectionString))
            {
                db.Model.GetEntityTypes()
                    .Single(entity => entity.GetTableName() == "CurrencyRates")
                    .FindProperty(nameof(CurrencyRate.USDToCurrency))!
                    .GetColumnType()
                    .Should().Be("TEXT");

                await db.Database.MigrateAsync("20260911044118_InitialCreate");
                await db.Database.OpenConnectionAsync();
                await db.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF;");
                await db.Database.ExecuteSqlRawAsync("""
                    INSERT INTO Currencies (CurrencyCode, Name, Symbol, DecimalDigits, IsActive)
                    VALUES ('USD', 'US Dollar', '$', 2, 1);
                    """);
                await using var insert = db.Database.GetDbConnection().CreateCommand();
                insert.CommandText = """
                    INSERT INTO CurrencyRates (WebsiteID, CurrencyCode, USDToCurrency, IsDefault, LastUpdate)
                    VALUES (1, 'USD', @rate, 1, '2026-01-02 00:00:00');
                    """;
                var rate = insert.CreateParameter();
                rate.ParameterName = "@rate";
                rate.Value = original;
                insert.Parameters.Add(rate);
                (await insert.ExecuteNonQueryAsync()).Should().Be(1);
                await db.Database.ExecuteSqlRawAsync("DROP TABLE IF EXISTS \"__EFMigrationsHistory\";");
            }

            await using (var services = CreateUpdater(connectionString))
            {
                var updater = services.GetRequiredService<IDatabaseUpdateService>();
                await updater.ApplyUpdatesAsync();
            }

            await using (var db = Create(connectionString))
            {
                var stored = await db.CurrencyRates.AsNoTracking().SingleAsync();
                stored.USDToCurrency.Should().Be(original);
                stored.CurrencyCode.Should().Be("USD");

                await using var typeCommand = db.Database.GetDbConnection().CreateCommand();
                typeCommand.CommandText = "SELECT typeof(USDToCurrency) FROM CurrencyRates;";
                if (typeCommand.Connection!.State != System.Data.ConnectionState.Open)
                    await typeCommand.Connection.OpenAsync();
                var storageType = (string)(await typeCommand.ExecuteScalarAsync())!;
                storageType.Should().Be("text");

                stored = await db.CurrencyRates.SingleAsync();
                stored.USDToCurrency = 0.1m + 0.2m;
                await db.SaveChangesAsync();
            }

            await using (var db = Create(connectionString))
            {
                (await db.CurrencyRates.AsNoTracking().SingleAsync()).USDToCurrency.Should().Be(0.3m);
                var before = await db.CurrencyRates.CountAsync();
                await db.Database.MigrateAsync();
                (await db.CurrencyRates.CountAsync()).Should().Be(before);
                (await db.CurrencyRates.AsNoTracking().SingleAsync()).USDToCurrency.Should().Be(0.3m);
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            TryDelete(path);
        }
    }

    [Fact]
    public async Task CreateDatabase_does_not_delete_an_existing_file_and_test_does_not_create_one()
    {
        var directory = Path.Combine(Path.GetTempPath(), "dotnetable-sqlite-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "keep.sqlite");
        var missing = Path.Combine(directory, "missing.sqlite");
        try
        {
            var connectionString = new SqliteConnectionStringBuilder { DataSource = path }.ToString();
            await using (var connection = new SqliteConnection(connectionString))
            {
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = "CREATE TABLE Keep (Id INTEGER PRIMARY KEY, Note TEXT); INSERT INTO Keep (Note) VALUES ('stay');";
                await command.ExecuteNonQueryAsync();
            }

            var provisioner = new SqliteProvisioner();
            var info = new DatabaseConnectionInfo { Provider = "SQLite", DatabaseName = path };
            var length = new FileInfo(path).Length;
            await provisioner.CreateDatabaseAsync(info);
            new FileInfo(path).Length.Should().Be(length);

            await using (var connection = new SqliteConnection(connectionString))
            {
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT Note FROM Keep;";
                (await command.ExecuteScalarAsync() as string).Should().Be("stay");
            }

            var missingInfo = new DatabaseConnectionInfo { Provider = "SQLite", DatabaseName = missing };
            var test = await provisioner.TestConnectionAsync(missingInfo);
            test.Success.Should().BeTrue();
            File.Exists(missing).Should().BeFalse();

            var act = () => provisioner.BuildConnectionString(
                new DatabaseConnectionInfo { Provider = "SQLite", DatabaseName = Path.Combine(directory, "..", "escape.sqlite") },
                includeDatabase: true);
            act.Should().Throw<InvalidOperationException>();
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            TryDelete(path);
            TryDelete(missing);
            try { Directory.Delete(directory, recursive: true); } catch { /* temp dir */ }
        }
    }

    private static AppDbContext Create(string connectionString)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>();
        ServiceCollectionExtensions.ConfigureProvider(options, "SQLite", connectionString);
        return new AppDbContext(options.Options);
    }

    private static ServiceProvider CreateUpdater(string connectionString)
    {
        var services = new ServiceCollection();
        services.AddDbContextFactory<AppDbContext>(options =>
            ServiceCollectionExtensions.ConfigureProvider(options, "sqlite", connectionString));
        services.AddSingleton<IDatabaseUpdateService, DatabaseUpdateService>();
        return services.BuildServiceProvider();
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch
        {
            // The assertion already ran. A locked temp file is not the user's database.
        }
    }
}
