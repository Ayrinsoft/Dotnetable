using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class PostCategory
{
    public int PostID { get; set; }

    public int CategoryID { get; set; }

    public bool IsPrimary { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Post Post { get; set; } = null!;
}
