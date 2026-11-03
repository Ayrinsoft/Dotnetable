using System;

namespace Dotnetable.Domain.Entities;

/// <summary>A row means this post has been sent to X at least once. Sending again is allowed.</summary>
public partial class PostXPublication
{
    public int PostID { get; set; }

    public string? TweetId { get; set; }

    public DateTime PublishedAt { get; set; }

    public virtual Post Post { get; set; } = null!;
}
