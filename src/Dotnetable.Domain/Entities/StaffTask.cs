using System;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// A to-do assigned to an admin member on a website. Related record is polymorphic
/// (order, stock document, customer return, payment, refund).
/// </summary>
public partial class StaffTask
{
    public int StaffTaskID { get; set; }

    public int WebsiteID { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary><see cref="Enums.StaffTaskStatus"/>.</summary>
    public byte Status { get; set; }

    /// <summary><see cref="Enums.StaffTaskPriority"/>.</summary>
    public byte Priority { get; set; }

    public int AssignedMemberID { get; set; }

    public int CreatedByMemberID { get; set; }

    public DateTime? DueAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    /// <summary><see cref="Enums.StaffTaskRelatedKind"/>.</summary>
    public byte RelatedKind { get; set; }

    public int? RelatedEntityID { get; set; }

    /// <summary>Cached display label for the related record (order number, document number, …).</summary>
    public string? RelatedLabel { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual Member AssignedMember { get; set; } = null!;

    public virtual Member CreatedByMember { get; set; } = null!;

    public virtual ICollection<StaffTaskNote> StaffTaskNotes { get; set; } = new List<StaffTaskNote>();
}
