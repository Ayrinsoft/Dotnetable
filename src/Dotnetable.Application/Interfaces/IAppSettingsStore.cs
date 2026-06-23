using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Reads/writes application settings that live in the local settings file next to the database
/// connection (not in the database itself): currently the anti-bot security settings.
/// </summary>
public interface IAppSettingsStore
{
    /// <summary>Current security settings (never null; defaults when nothing persisted).</summary>
    SecuritySettings Security { get; }

    Task SaveSecurityAsync(SecuritySettings settings, CancellationToken ct = default);
}
