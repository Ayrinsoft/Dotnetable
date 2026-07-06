using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// Whether a given <see cref="Enums.WebsiteFeatureKey"/> module is enabled for a website.
/// Seeded from <see cref="Enums.WebsiteTypeExtensions.GetDefaultFeatures"/> when the site is
/// created, then freely toggled afterwards from the website settings screen.
/// </summary>
public partial class WebsiteFeature
{
    public int WebsiteFeatureID { get; set; }

    public int WebsiteID { get; set; }

    public byte FeatureKey { get; set; }

    public bool Enabled { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
}
