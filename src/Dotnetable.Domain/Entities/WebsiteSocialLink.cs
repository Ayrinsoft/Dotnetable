using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class WebsiteSocialLink
{
    public int WebsiteSocialLinkID { get; set; }

    public int WebsiteID { get; set; }

    public byte SocialType { get; set; }

    public string? SocialName { get; set; }

    public string? SocialIcon { get; set; }

    public string UrlAddress { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;
}
