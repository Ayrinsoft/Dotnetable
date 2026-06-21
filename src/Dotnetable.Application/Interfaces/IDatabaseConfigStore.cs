namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Holds the active database connection. On first run it is empty; the Setup page fills it
/// and persists it to dbsettings.json so the DbContext can bind to it without an app restart.
/// </summary>
public interface IDatabaseConfigStore
{
    bool IsConfigured { get; }
    string Provider { get; }
    string? ConnectionString { get; }

    Task SaveAsync(string provider, string connectionString, CancellationToken ct = default);
}
