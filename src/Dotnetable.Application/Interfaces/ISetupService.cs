using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

public interface ISetupService
{
    /// <summary>True once the database is configured and the master website exists.</summary>
    Task<bool> IsSetupCompletedAsync(CancellationToken ct = default);

    /// <summary>Opens a connection to the server to validate the supplied database credentials.</summary>
    Task<ConnectionTestResult> TestConnectionAsync(DatabaseConnectionInfo db, CancellationToken ct = default);

    /// <summary>
    /// Tests the connection, optionally creates the database, persists the connection, applies the
    /// schema (migrations), then creates the master website (WebsiteID 1), the full-access policy
    /// and the first administrator member. Throws if setup has already been completed.
    /// </summary>
    Task CompleteSetupAsync(SetupRequest request, CancellationToken ct = default);

    /// <summary>
    /// Inserts any permission keys from the role catalog that a configured database is missing
    /// (e.g. after a version upgrade that introduced new permissions). Additive and idempotent;
    /// existing roles and policy grants are left untouched. No-op when not yet configured.
    /// </summary>
    Task SyncRoleCatalogAsync(CancellationToken ct = default);
}
