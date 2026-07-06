using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class WebsiteTheme
{
    public int WebsiteThemeID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    /// <summary>Design tokens as JSON (colors, fonts, radius, dark-mode default …). The React
    /// front-end applies these as CSS custom properties at runtime, so a serverless deployment can
    /// be re-themed from the admin without a rebuild.</summary>
    public string SettingsJson { get; set; } = null!;

    /// <summary>The theme the public front-end currently uses; at most one per website.</summary>
    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
}
