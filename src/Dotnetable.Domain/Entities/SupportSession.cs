using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// One support conversation / ticket opened for a customer (typically after a phone call).
/// Snapshot fields keep the dialed number even if the client profile later changes.
/// </summary>
public partial class SupportSession
{
    public int SupportSessionID { get; set; }

    public int WebsiteID { get; set; }

    public int? WebsiteClientID { get; set; }

    /// <summary>Human-readable ticket id, e.g. SUP-20260726-00042.</summary>
    public string SessionNumber { get; set; } = null!;

    /// <summary>See <c>SupportSessionStatus</c> in Application layer.</summary>
    public byte Status { get; set; }

    /// <summary>See <c>SupportPriority</c>.</summary>
    public byte Priority { get; set; }

    /// <summary>See <c>SupportChannel</c>.</summary>
    public byte Channel { get; set; }

    /// <summary>See <c>SupportCategory</c>.</summary>
    public byte Category { get; set; }

    public string? Subject { get; set; }

    /// <summary>Comma-separated free-form tags for filtering.</summary>
    public string? Tags { get; set; }

    /// <summary>National mobile digits as entered / matched (no country code).</summary>
    public string? CellphoneSnapshot { get; set; }

    public string? CountryCodeSnapshot { get; set; }

    public string? EmailSnapshot { get; set; }

    public string? CustomerNameSnapshot { get; set; }

    /// <summary>Primary order this ticket is about (optional).</summary>
    public int? RelatedOrderID { get; set; }

    public int? AssignedMemberID { get; set; }

    public int? CreatedByMemberID { get; set; }

    public DateTime? FirstResponseAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? LastInteractionAt { get; set; }

    /// <summary>Optional CSAT 1–5 after resolution.</summary>
    public byte? SatisfactionRating { get; set; }

    public bool Archive { get; set; }

    public virtual Member? AssignedMember { get; set; }

    public virtual Member? CreatedByMember { get; set; }

    public virtual Order? RelatedOrder { get; set; }

    public virtual ICollection<SupportInteraction> SupportInteractions { get; set; } = new List<SupportInteraction>();

    public virtual Website Website { get; set; } = null!;

    public virtual WebsiteClient? WebsiteClient { get; set; }
}
