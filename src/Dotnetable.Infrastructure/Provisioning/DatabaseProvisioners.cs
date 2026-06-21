using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;

namespace Dotnetable.Infrastructure.Provisioning;

internal static class DbIdentifier
{
    public static string Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
            throw new InvalidOperationException($"Invalid database name '{name}'. Use letters, digits and underscores only.");
        return name;
    }
}

public class DatabaseProvisionerRegistry : IDatabaseProvisionerRegistry
{
    private readonly IEnumerable<IDatabaseProvisioner> _provisioners;

    public DatabaseProvisionerRegistry(IEnumerable<IDatabaseProvisioner> provisioners) => _provisioners = provisioners;

    public IDatabaseProvisioner Get(string provider) =>
        _provisioners.FirstOrDefault(p => p.Handles(provider))
        ?? throw new NotSupportedException($"No provisioner for database provider '{provider}'.");
}

public class SqlServerProvisioner : IDatabaseProvisioner
{
    public bool Handles(string provider) => provider.Equals("sqlserver", StringComparison.OrdinalIgnoreCase);

    public string BuildConnectionString(DatabaseConnectionInfo info, bool includeDatabase)
    {
        var database = includeDatabase ? info.DatabaseName : "master";
        var auth = info.IntegratedSecurity
            ? "Trusted_Connection=True;"
            : $"User Id={info.Username};Password={info.Password};";
        return $"Server={info.Server};Database={database};{auth}TrustServerCertificate={info.TrustServerCertificate};";
    }

    public async Task<ConnectionTestResult> TestConnectionAsync(DatabaseConnectionInfo info, CancellationToken ct = default)
    {
        try
        {
            await using var connection = new SqlConnection(BuildConnectionString(info, includeDatabase: false));
            await connection.OpenAsync(ct);
            return ConnectionTestResult.Ok($"Connected to SQL Server '{info.Server}'.");
        }
        catch (Exception ex)
        {
            return ConnectionTestResult.Fail(ex.Message);
        }
    }

    public async Task CreateDatabaseAsync(DatabaseConnectionInfo info, CancellationToken ct = default)
    {
        var name = DbIdentifier.Validate(info.DatabaseName);
        await using var connection = new SqlConnection(BuildConnectionString(info, includeDatabase: false));
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = $"IF DB_ID(N'{name}') IS NULL CREATE DATABASE [{name}];";
        await command.ExecuteNonQueryAsync(ct);
    }
}

public class PostgreSqlProvisioner : IDatabaseProvisioner
{
    public bool Handles(string provider) =>
        provider.Equals("postgresql", StringComparison.OrdinalIgnoreCase) ||
        provider.Equals("postgres", StringComparison.OrdinalIgnoreCase);

    public string BuildConnectionString(DatabaseConnectionInfo info, bool includeDatabase)
    {
        var database = includeDatabase ? info.DatabaseName : "postgres";
        return $"Host={info.Server};Port={info.Port};Database={database};Username={info.Username};Password={info.Password};";
    }

    public async Task<ConnectionTestResult> TestConnectionAsync(DatabaseConnectionInfo info, CancellationToken ct = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(BuildConnectionString(info, includeDatabase: false));
            await connection.OpenAsync(ct);
            return ConnectionTestResult.Ok($"Connected to PostgreSQL '{info.Server}:{info.Port}'.");
        }
        catch (Exception ex)
        {
            return ConnectionTestResult.Fail(ex.Message);
        }
    }

    public async Task CreateDatabaseAsync(DatabaseConnectionInfo info, CancellationToken ct = default)
    {
        var name = DbIdentifier.Validate(info.DatabaseName);
        await using var connection = new NpgsqlConnection(BuildConnectionString(info, includeDatabase: false));
        await connection.OpenAsync(ct);

        await using var check = connection.CreateCommand();
        check.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{name}';";
        var exists = await check.ExecuteScalarAsync(ct);
        if (exists is not null) return;

        await using var create = connection.CreateCommand();
        create.CommandText = $"CREATE DATABASE \"{name}\";";
        await create.ExecuteNonQueryAsync(ct);
    }
}

public class MySqlProvisioner : IDatabaseProvisioner
{
    public bool Handles(string provider) =>
        provider.Equals("mysql", StringComparison.OrdinalIgnoreCase) ||
        provider.Equals("mariadb", StringComparison.OrdinalIgnoreCase);

    public string BuildConnectionString(DatabaseConnectionInfo info, bool includeDatabase)
    {
        var database = includeDatabase ? $"Database={info.DatabaseName};" : string.Empty;
        return $"Server={info.Server};Port={info.Port};{database}User Id={info.Username};Password={info.Password};";
    }

    public async Task<ConnectionTestResult> TestConnectionAsync(DatabaseConnectionInfo info, CancellationToken ct = default)
    {
        try
        {
            await using var connection = new MySqlConnection(BuildConnectionString(info, includeDatabase: false));
            await connection.OpenAsync(ct);
            return ConnectionTestResult.Ok($"Connected to MySQL/MariaDB '{info.Server}:{info.Port}'.");
        }
        catch (Exception ex)
        {
            return ConnectionTestResult.Fail(ex.Message);
        }
    }

    public async Task CreateDatabaseAsync(DatabaseConnectionInfo info, CancellationToken ct = default)
    {
        var name = DbIdentifier.Validate(info.DatabaseName);
        await using var connection = new MySqlConnection(BuildConnectionString(info, includeDatabase: false));
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE IF NOT EXISTS `{name}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
        await command.ExecuteNonQueryAsync(ct);
    }
}
