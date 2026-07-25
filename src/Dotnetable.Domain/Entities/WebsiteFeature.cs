using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class WebsiteFeature
{
    public int WebsiteFeatureID { get; set; }

    public int WebsiteID { get; set; }

    public byte FeatureKey { get; set; }

    public bool Enabled { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;
}
