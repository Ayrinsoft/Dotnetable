using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// One entry of an author's online résumé (a job, degree, project, award…). Items are grouped by
/// <see cref="ItemType"/> into résumé sections, and the ones with <see cref="ShowInTimeline"/> also
/// appear on the page's chronological timeline.
/// </summary>
public partial class AuthorResumeItem
{
    public int AuthorResumeItemID { get; set; }

    public int AuthorProfileID { get; set; }

    /// <summary><see cref="Enums.ResumeItemType"/>.</summary>
    public byte ItemType { get; set; }

    public string Title { get; set; } = null!;

    /// <summary>Company, school, client or issuer.</summary>
    public string? Organization { get; set; }

    public string? Location { get; set; }

    public string? Description { get; set; }

    public string? Url { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    /// <summary>Still ongoing ("present"); <see cref="EndDate"/> is ignored when set.</summary>
    public bool IsCurrent { get; set; }

    public bool ShowInTimeline { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual AuthorProfile AuthorProfile { get; set; } = null!;

    public virtual ICollection<AuthorResumeItemTranslation> AuthorResumeItemTranslations { get; set; } = new List<AuthorResumeItemTranslation>();
}
