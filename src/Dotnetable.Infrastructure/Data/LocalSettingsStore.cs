using System.Text.Json;
using System.Text.Json.Serialization;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;

namespace Dotnetable.Infrastructure.Data;

/// <summary>
/// Single writable JSON settings file (next to the app) holding everything that must persist
/// outside the database: the database connection itself and the anti-bot security settings.
/// Implements both <see cref="IDatabaseConfigStore"/> and <see cref="IAppSettingsStore"/> so a
/// single instance owns the file and there is never more than one writer.
/// </summary>
public class LocalSettingsStore : IDatabaseConfigStore, IAppSettingsStore
{
    private readonly string _filePath;
    private readonly object _gate = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private PersistedSettings _settings;

    public LocalSettingsStore(string filePath, string defaultProvider, string? defaultConnectionString)
    {
        _filePath = filePath;
        _settings = new PersistedSettings
        {
            Database = new DatabaseSection
            {
                Provider = string.IsNullOrWhiteSpace(defaultProvider) ? "SqlServer" : defaultProvider,
                ConnectionString = string.IsNullOrWhiteSpace(defaultConnectionString) ? null : defaultConnectionString,
            },
        };

        // A persisted file (written by Setup or the Settings page) takes precedence over appsettings.
        if (File.Exists(_filePath))
        {
            try
            {
                var persisted = JsonSerializer.Deserialize<PersistedSettings>(File.ReadAllText(_filePath), JsonOptions);
                if (persisted is not null)
                {
                    if (persisted.Database is { } db && !string.IsNullOrWhiteSpace(db.ConnectionString))
                        _settings.Database = db;
                    if (persisted.Security is { } sec)
                        _settings.Security = sec;
                }
            }
            catch
            {
                // Corrupt file: fall back to appsettings defaults; Setup/Settings will rewrite it.
            }
        }
    }

    // ── IDatabaseConfigStore ──────────────────────────────────────────────
    public bool IsConfigured
    {
        get { lock (_gate) return !string.IsNullOrWhiteSpace(_settings.Database.ConnectionString); }
    }

    public string Provider
    {
        get { lock (_gate) return _settings.Database.Provider; }
    }

    public string? ConnectionString
    {
        get { lock (_gate) return _settings.Database.ConnectionString; }
    }

    public async Task SaveAsync(string provider, string connectionString, CancellationToken ct = default)
    {
        lock (_gate)
        {
            _settings.Database.Provider = provider;
            _settings.Database.ConnectionString = connectionString;
        }
        await PersistAsync(ct);
    }

    // ── IAppSettingsStore ─────────────────────────────────────────────────
    public SecuritySettings Security
    {
        get
        {
            lock (_gate)
            {
                var s = _settings.Security;
                // Hand out a copy so callers can't mutate the cached instance without saving.
                return new SecuritySettings
                {
                    TurnstileSiteKey = s.TurnstileSiteKey,
                    TurnstileSecretKey = s.TurnstileSecretKey,
                    CaptchaMode = s.CaptchaMode,
                };
            }
        }
    }

    public async Task SaveSecurityAsync(SecuritySettings settings, CancellationToken ct = default)
    {
        lock (_gate)
        {
            _settings.Security = new SecuritySettings
            {
                TurnstileSiteKey = settings.TurnstileSiteKey?.Trim() ?? string.Empty,
                TurnstileSecretKey = settings.TurnstileSecretKey?.Trim() ?? string.Empty,
                CaptchaMode = settings.CaptchaMode,
            };
        }
        await PersistAsync(ct);
    }

    private async Task PersistAsync(CancellationToken ct)
    {
        string json;
        lock (_gate) json = JsonSerializer.Serialize(_settings, JsonOptions);
        await File.WriteAllTextAsync(_filePath, json, ct);
    }

    private sealed class PersistedSettings
    {
        public DatabaseSection Database { get; set; } = new();
        public SecuritySettings Security { get; set; } = new();
    }

    private sealed class DatabaseSection
    {
        public string Provider { get; set; } = "SqlServer";
        public string? ConnectionString { get; set; }
    }
}
