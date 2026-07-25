using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class WebsiteTheme
{
    public int WebsiteThemeID { get; set; }

    public int WebsiteID { get; set; }

    public string Slug { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Version { get; set; }

    public string? Author { get; set; }

    public string? Description { get; set; }

    public bool HasScreenshot { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
}
