using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
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

public class SqliteProvisioner : IDatabaseProvisioner
{
    public bool Handles(string provider) => provider.Equals("sqlite", StringComparison.OrdinalIgnoreCase);

    public string BuildConnectionString(DatabaseConnectionInfo info, bool includeDatabase)
    {
        var path = ValidatePath(info.DatabaseName);
        return new SqliteConnectionStringBuilder
        {
            DataSource = path,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true,
        }.ToString();
    }

    public async Task<ConnectionTestResult> TestConnectionAsync(DatabaseConnectionInfo info, CancellationToken ct = default)
    {
        try
        {
            var path = ValidatePath(info.DatabaseName);
            // Opening ReadWriteCreate would create the file. A missing file is a valid setup target.
            if (!File.Exists(path))
                return ConnectionTestResult.Ok($"SQLite file '{path}' will be created.");

            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = path,
                Mode = SqliteOpenMode.ReadOnly,
                ForeignKeys = true,
            }.ToString();
            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync(ct);
            return ConnectionTestResult.Ok($"Opened SQLite file '{path}'.");
        }
        catch (Exception ex)
        {
            return ConnectionTestResult.Fail(ex.Message);
        }
    }

    public async Task CreateDatabaseAsync(DatabaseConnectionInfo info, CancellationToken ct = default)
    {
        var path = ValidatePath(info.DatabaseName);
        // An existing file is the database. Never delete or recreate it.
        if (File.Exists(path))
            return;

        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = path,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true,
        }.ToString();
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(ct);
    }

    internal static string ValidatePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException("SQLite database path is required.");

        var trimmed = path.Trim();
        if (trimmed.Contains('\0'))
            throw new InvalidOperationException("SQLite database path is invalid.");

        var segments = trimmed.Split(['/', '\\']);
        if (segments.Any(segment => segment == ".."))
            throw new InvalidOperationException("SQLite database path must not contain '..'.");

        return trimmed;
    }
}
