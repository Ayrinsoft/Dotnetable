using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class WebsiteSeoSetting
{
    public int WebsiteSeoSettingID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>
    /// Page | {SiteName}
    /// </summary>
    public string? DefaultMetaTitle { get; set; }

    public string? TitleSeparator { get; set; }

    public string? DefaultMetaDescription { get; set; }

    public bool SitemapEnabled { get; set; }

    public bool RobotsEnabled { get; set; }

    public string CustomRobotsTxt { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;
}
