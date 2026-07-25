using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Form
{
    public int FormID { get; set; }

    public int WebsiteID { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public byte FormType { get; set; }

    public string? SubmitButtonText { get; set; }

    public string? SuccessMessage { get; set; }

    public bool RequireLogin { get; set; }

    public bool AllowMultipleSubmissions { get; set; }

    public bool ShowResults { get; set; }

    public string? NotifyEmail { get; set; }

    public DateTime? StartAt { get; set; }

    public DateTime? EndAt { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<FormField> FormFields { get; set; } = new List<FormField>();

    public virtual ICollection<FormResponse> FormResponses { get; set; } = new List<FormResponse>();

    public virtual Website Website { get; set; } = null!;
}
