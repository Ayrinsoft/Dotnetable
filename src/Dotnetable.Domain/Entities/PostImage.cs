namespace Dotnetable.Domain.Entities;

/// <summary>One photo in a post gallery. The lowest <see cref="SortOrder"/> is the cover and matches <see cref="Post.FeaturedImageFileID"/>.</summary>
public partial class PostImage
{
    public int PostImageID { get; set; }

    public int PostID { get; set; }

    public int FileRecordID { get; set; }

    public int SortOrder { get; set; }

    public virtual FileRecord FileRecord { get; set; } = null!;

    public virtual Post Post { get; set; } = null!;
}
