using System;

namespace Dotnetable.Domain.Entities;

/// <summary>Follow-up on a staff task (new facts, progress, comments) without stretching the list.</summary>
public partial class StaffTaskNote
{
    public int StaffTaskNoteID { get; set; }

    public int StaffTaskID { get; set; }

    public int CreatedByMemberID { get; set; }

    public string Body { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual StaffTask StaffTask { get; set; } = null!;

    public virtual Member CreatedByMember { get; set; } = null!;
}
