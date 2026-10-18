using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Security;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class SettingProtectorTests
{
    [Fact]
    public void DeriveKey_IgnoresPlaceholderAndBlank()
    {
        SettingProtector.DeriveKey(null, "dev-only-change-me-cache-sync-secret").Should().BeNull();
        SettingProtector.DeriveKey("", null).Should().BeNull();
        SettingProtector.DeriveKey("short", null).Should().BeNull();
    }

    [Fact]
    public void RoundTrip_AndLeavesLegacyPlaintext()
    {
        var key = SettingProtector.DeriveKey(null, "test-secret-value-0123456789abcd");
        key.Should().NotBeNull();

        var cipher = SettingProtector.Protect("api-key-1", key!);
        cipher.Should().StartWith(SettingProtector.Prefix);
        SettingProtector.Unprotect(cipher, key!).Should().Be("api-key-1");
        SettingProtector.Unprotect("still-plain", key!).Should().Be("still-plain");
        SettingProtector.Protect(cipher, key!).Should().Be(cipher);
    }

    [Fact]
    public void DifferentSecrets_CannotDecrypt()
    {
        var a = SettingProtector.DeriveKey(null, "test-secret-value-0123456789abcd")!;
        var b = SettingProtector.DeriveKey("other-protection-key-value", null)!;
        var cipher = SettingProtector.Protect("api-key-1", a);
        var act = () => SettingProtector.Unprotect(cipher, b);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task Database_StoresCiphertext_AndBackfillRewritesPlaintext()
    {
        var key = SettingProtector.DeriveKey(null, "test-secret-value-0123456789abcd")!;
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = ":memory:",
            Mode = SqliteOpenMode.Memory,
            ForeignKeys = false,
        }.ToString());
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .UseSettingProtection(key)
            .Options;

        await using (var created = new AppDbContext(options))
            await created.Database.EnsureCreatedAsync();

        await using (var context = new AppDbContext(options))
        {
            context.WebsiteSmsSettings.Add(new WebsiteSmsSetting
            {
                WebsiteID = 1,
                Provider = "Kavenegar",
                Title = "live",
                SettingsJSON = "{\"apiKey\":\"secret-value\"}",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            });
            await context.SaveChangesAsync();
        }

        var stored = ReadSmsJson(connection);
        stored.Should().StartWith(SettingProtector.Prefix);
        stored.Should().NotContain("secret-value");

        await using (var context = new AppDbContext(options))
        {
            var live = await context.WebsiteSmsSettings.SingleAsync(s => s.Title == "live");
            live.SortOrder = 4;
            await context.SaveChangesAsync();
        }

        ReadSmsJson(connection).Should().Be(stored, "an unchanged secret must not be rewritten with a new nonce");

        using (var insert = connection.CreateCommand())
        {
            insert.CommandText =
                "INSERT INTO WebsiteSmsSettings (WebsiteID, Provider, Title, SettingsJSON, IsActive, SortOrder, CreatedAt) " +
                "VALUES (1, 'Kavenegar', 'legacy', '{\"apiKey\":\"legacy-secret\"}', 1, 0, '2026-01-01')";
            insert.ExecuteNonQuery();
        }

        await using (var context = new AppDbContext(options))
        {
            var legacy = await context.WebsiteSmsSettings.SingleAsync(s => s.Title == "legacy");
            legacy.SettingsJSON.Should().Be("{\"apiKey\":\"legacy-secret\"}");
            (await SettingSecretBackfill.RunAsync(context)).Should().Be(1);
        }

        var rows = ReadAllSmsJson(connection);
        rows.Should().OnlyContain(v => v.StartsWith(SettingProtector.Prefix, StringComparison.Ordinal));

        await using (var context = new AppDbContext(options))
        {
            var legacy = await context.WebsiteSmsSettings.SingleAsync(s => s.Title == "legacy");
            legacy.SettingsJSON.Should().Be("{\"apiKey\":\"legacy-secret\"}");
        }
    }

    private static string ReadSmsJson(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT SettingsJSON FROM WebsiteSmsSettings WHERE Title = 'live'";
        return (string)cmd.ExecuteScalar()!;
    }

    private static List<string> ReadAllSmsJson(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT SettingsJSON FROM WebsiteSmsSettings";
        using var reader = cmd.ExecuteReader();
        var values = new List<string>();
        while (reader.Read())
            values.Add(reader.GetString(0));
        return values;
    }
}
