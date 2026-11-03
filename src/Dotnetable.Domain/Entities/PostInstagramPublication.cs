using System;

namespace Dotnetable.Domain.Entities;

/// <summary>One Instagram publish for a post. A row means the post was already sent and must not be sent again.</summary>
public partial class PostInstagramPublication
{
    public int PostID { get; set; }

    public string IgMediaId { get; set; } = null!;

    public string? Permalink { get; set; }

    public DateTime PublishedAt { get; set; }

    public virtual Post Post { get; set; } = null!;
}
