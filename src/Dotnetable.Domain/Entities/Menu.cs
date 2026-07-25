using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Menu
{
    public int MenuID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public byte Location { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public virtual Website Website { get; set; } = null!;
}
