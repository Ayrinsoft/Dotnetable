using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class WebsiteScript
{
    public int WebsiteScriptID { get; set; }

    public int WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public string RawContent { get; set; } = null!;

    public byte ScriptPosition { get; set; }

    public byte ScriptLoadCondition { get; set; }

    public DateTime LogTime { get; set; }

    public bool Active { get; set; }

    public byte? Priority { get; set; }

    public virtual Website Website { get; set; } = null!;
}
