using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class WebsiteWatermarkSetting
{
    public int WebsiteWatermarkSettingID { get; set; }

    public int WebsiteID { get; set; }

    public int? WatermarkFileID { get; set; }

    public byte Position { get; set; } = (byte)9;

    public int SizePercent { get; set; } = 20;

    public int Opacity { get; set; } = 80;

    public bool Enabled { get; set; }

    public virtual FileRecord? WatermarkFile { get; set; }

    public virtual Website Website { get; set; } = null!;
}
