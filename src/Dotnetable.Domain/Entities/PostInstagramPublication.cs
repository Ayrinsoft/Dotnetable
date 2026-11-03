using System;

namespace Dotnetable.Domain.Entities;

/// <summary>A row means this post has been sent to Instagram at least once. Sending again is allowed.</summary>
public partial class PostInstagramPublication
{
    public int PostID { get; set; }

    public string IgMediaId { get; set; } = null!;

    public string? Permalink { get; set; }

    public DateTime PublishedAt { get; set; }

    public virtual Post Post { get; set; } = null!;
}
