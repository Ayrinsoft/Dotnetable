using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>One row per website: the watermark image applied to uploaded images and how to place it.</summary>
public partial class WebsiteWatermarkSetting
{
    public int WebsiteWatermarkSettingID { get; set; }

    public int WebsiteID { get; set; }

    public int? WatermarkFileID { get; set; }

    /// <summary><see cref="Enums.WatermarkPosition"/>.</summary>
    public byte Position { get; set; }

    /// <summary>Watermark width as a percentage of the base image width (1-100).</summary>
    public int SizePercent { get; set; }

    /// <summary>Watermark opacity percentage (0-100).</summary>
    public int Opacity { get; set; }

    /// <summary>When true, the watermark is applied to every image upload for this website automatically.</summary>
    public bool Enabled { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual FileRecord? WatermarkFile { get; set; }
}
