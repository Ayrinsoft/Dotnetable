using System;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// One attempted push (or feed build) for a channel, so an admin can see what the last run did
/// without reading server logs.
/// </summary>
public partial class MarketplaceSyncLog
{
    public int MarketplaceSyncLogID { get; set; }

    public int MarketplaceChannelID { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    /// <summary>Mirrors <c>MarketplaceSyncStatus</c>: 0 running, 1 success, 2 partial, 3 failed.</summary>
    public byte Status { get; set; }

    /// <summary>What started the run: <c>Manual</c>, <c>Schedule</c> or <c>Feed</c>.</summary>
    public string TriggeredBy { get; set; } = null!;

    public int ItemCount { get; set; }

    public int FailedCount { get; set; }

    public string? Message { get; set; }

    public virtual MarketplaceChannel MarketplaceChannel { get; set; } = null!;
}
