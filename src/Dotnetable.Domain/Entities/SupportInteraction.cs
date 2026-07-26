using System;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// Append-only timeline entry on a support session: calls, notes, status changes, assignments.
/// </summary>
public partial class SupportInteraction
{
    public int SupportInteractionID { get; set; }

    public int SupportSessionID { get; set; }

    /// <summary>See <c>SupportInteractionType</c>.</summary>
    public byte InteractionType { get; set; }

    public string? Body { get; set; }

    /// <summary>Call duration in seconds when the type is a call.</summary>
    public int? DurationSeconds { get; set; }

    /// <summary>See <c>SupportCallOutcome</c>; only for call interactions.</summary>
    public byte? CallOutcome { get; set; }

    public byte? FromStatus { get; set; }

    public byte? ToStatus { get; set; }

    public int? RelatedOrderID { get; set; }

    public int? CreatedByMemberID { get; set; }

    /// <summary>When true, only staff see this entry (internal note).</summary>
    public bool IsInternal { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Member? CreatedByMember { get; set; }

    public virtual Order? RelatedOrder { get; set; }

    public virtual SupportSession SupportSession { get; set; } = null!;
}
