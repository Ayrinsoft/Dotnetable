namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Versioned database updates via EF Core migrations. When a new panel build ships new
/// migrations, the pending ones are applied additively so a running install upgrades cleanly.
/// </summary>
public interface IDatabaseUpdateService
{
    Task<IReadOnlyList<string>> GetAppliedUpdatesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetPendingUpdatesAsync(CancellationToken ct = default);

    /// <summary>Applies every pending migration. No-op when the database is already up to date.</summary>
    Task ApplyUpdatesAsync(CancellationToken ct = default);
}
