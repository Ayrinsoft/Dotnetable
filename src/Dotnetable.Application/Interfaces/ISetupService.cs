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
    /// (e.g. after a version upgrade that introduced new permissions), then grants every catalog
    /// key to each Administrators policy that does not already have it. Additive and idempotent;
    /// other policies and extra grants are left untouched. No-op when not yet configured.
    /// </summary>
    Task SyncRoleCatalogAsync(CancellationToken ct = default);

    /// <summary>
    /// One-time data fix: re-derives any Category/Post/Page/Tag slug (main row or per-language
    /// translation) that isn't already URL-safe — e.g. one saved verbatim from a raw title before
    /// slug sanitizing existed, containing spaces/punctuation that get percent-encoded in public
    /// URLs — through the same normalizer new writes use, resolving any resulting collision the
    /// same way. Idempotent: already-clean slugs are left untouched, so this is cheap on repeat
    /// startups once every row has been fixed once. No-op when not yet configured.
    /// </summary>
    Task ReslugifyContentAsync(CancellationToken ct = default);
}
