using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class WebsiteIP
{
    public int WebsiteIPID { get; set; }

    public int WebsiteID { get; set; }

    public string StartIP { get; set; } = null!;

    public string? EndIP { get; set; }

    public int? CidrPrefix { get; set; }

    public string Label { get; set; } = null!;

    public bool Active { get; set; }

    public virtual Website Website { get; set; } = null!;
}
