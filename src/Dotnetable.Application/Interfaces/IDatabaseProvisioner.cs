using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

/// <summary>Provider-specific connection handling used by the Setup page (test + create database).</summary>
public interface IDatabaseProvisioner
{
    /// <summary>The canonical provider key this provisioner handles (e.g. "sqlserver").</summary>
    bool Handles(string provider);

    string BuildConnectionString(DatabaseConnectionInfo info, bool includeDatabase);

    Task<ConnectionTestResult> TestConnectionAsync(DatabaseConnectionInfo info, CancellationToken ct = default);

    /// <summary>Creates the target database on the server if it does not already exist.</summary>
    Task CreateDatabaseAsync(DatabaseConnectionInfo info, CancellationToken ct = default);
}

/// <summary>Resolves the <see cref="IDatabaseProvisioner"/> for a given provider key.</summary>
public interface IDatabaseProvisionerRegistry
{
    IDatabaseProvisioner Get(string provider);
}
