using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class MediaSet
{
    public int MediaSetID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public bool IsShared { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<MediaSetItem> MediaSetItems { get; set; } = new List<MediaSetItem>();

    public virtual ICollection<ProductMedium> ProductMedia { get; set; } = new List<ProductMedium>();

    public virtual Website Website { get; set; } = null!;
}
