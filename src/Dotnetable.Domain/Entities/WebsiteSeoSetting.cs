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

    /// <summary>
    /// When true, saving a published page, post, or product submits its public URL to IndexNow
    /// (Bing and the other engines on that protocol).
    /// </summary>
    public bool IndexNowEnabled { get; set; }

    /// <summary>
    /// Public ownership key, 8–128 letters, digits, or dashes. The storefront serves it at
    /// <c>/{key}.txt</c>. Not a secret: search engines fetch that file to verify the host.
    /// </summary>
    public string? IndexNowKey { get; set; }

    public string CustomRobotsTxt { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;
}
