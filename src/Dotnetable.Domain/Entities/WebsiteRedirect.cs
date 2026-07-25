using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class WebsiteRedirect
{
    public int WebsiteRedirectID { get; set; }

    public int WebsiteID { get; set; }

    public string SourcePath { get; set; } = null!;

    public string TargetPath { get; set; } = null!;

    public int StatusCode { get; set; }

    public bool IsRegex { get; set; }

    public int HitCount { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
}
