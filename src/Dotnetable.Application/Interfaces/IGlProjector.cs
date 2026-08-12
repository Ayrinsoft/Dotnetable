namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Projects operational ledger (L1) event groups into balanced general-ledger journals (L2).
/// Failures must never break payments — callers wrap in try/catch.
/// </summary>
public interface IGlProjector
{
    /// <summary>Project all current L1 rows sharing <paramref name="eventGroupId"/> into one posted journal.</summary>
    Task ProjectEventGroupAsync(int websiteId, Guid eventGroupId, CancellationToken ct = default);

    /// <summary>Project a single L1 entry (e.g. refund / settlement paid) into a journal.</summary>
    Task ProjectLedgerEntryAsync(long financialLedgerEntryId, CancellationToken ct = default);
}
