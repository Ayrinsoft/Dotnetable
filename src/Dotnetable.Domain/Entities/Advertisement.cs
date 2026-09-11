using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Advertisement
{
    public int AdvertisementID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary><see cref="Enums.AdvertisementLocation"/> — where the link is rendered.</summary>
    public byte Location { get; set; }

    /// <summary>Anchor text in the website's default language.</summary>
    public string Keyword { get; set; } = null!;

    /// <summary>Target URL in the website's default language (http(s) or a site-relative path).</summary>
    public string Url { get; set; } = null!;

    public bool OpenInNewTab { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<AdvertisementTranslation> AdvertisementTranslations { get; set; } = new List<AdvertisementTranslation>();

    public virtual Website Website { get; set; } = null!;
}
