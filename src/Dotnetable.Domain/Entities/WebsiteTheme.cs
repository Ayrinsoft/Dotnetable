using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class WebsiteTheme
{
    public int WebsiteThemeID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public string SettingsJson { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
}
