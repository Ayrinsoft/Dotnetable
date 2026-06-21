using System.Text.Json;
using Dotnetable.Application.Interfaces;

namespace Dotnetable.Infrastructure.Data;

public class DatabaseConfigStore : IDatabaseConfigStore
{
    private readonly string _filePath;
    private readonly object _gate = new();

    private string _provider;
    private string? _connectionString;

    public DatabaseConfigStore(string filePath, string defaultProvider, string? defaultConnectionString)
    {
        _filePath = filePath;
        _provider = string.IsNullOrWhiteSpace(defaultProvider) ? "MariaDB" : defaultProvider;
        _connectionString = string.IsNullOrWhiteSpace(defaultConnectionString) ? null : defaultConnectionString;

        // A persisted dbsettings.json (written by the Setup page) takes precedence over appsettings.
        if (File.Exists(_filePath))
        {
            try
            {
                var persisted = JsonSerializer.Deserialize<PersistedConfig>(File.ReadAllText(_filePath));
                if (persisted is not null && !string.IsNullOrWhiteSpace(persisted.ConnectionString))
                {
                    _provider = string.IsNullOrWhiteSpace(persisted.Provider) ? _provider : persisted.Provider;
                    _connectionString = persisted.ConnectionString;
                }
            }
            catch
            {
                // Corrupt file: fall back to appsettings defaults; Setup will rewrite it.
            }
        }
    }

    public bool IsConfigured
    {
        get { lock (_gate) return !string.IsNullOrWhiteSpace(_connectionString); }
    }

    public string Provider
    {
        get { lock (_gate) return _provider; }
    }

    public string? ConnectionString
    {
        get { lock (_gate) return _connectionString; }
    }

    public async Task SaveAsync(string provider, string connectionString, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(
            new PersistedConfig { Provider = provider, ConnectionString = connectionString },
            new JsonSerializerOptions { WriteIndented = true });

        await File.WriteAllTextAsync(_filePath, json, ct);

        lock (_gate)
        {
            _provider = provider;
            _connectionString = connectionString;
        }
    }

    private sealed class PersistedConfig
    {
        public string Provider { get; set; } = "MariaDB";
        public string ConnectionString { get; set; } = string.Empty;
    }
}
