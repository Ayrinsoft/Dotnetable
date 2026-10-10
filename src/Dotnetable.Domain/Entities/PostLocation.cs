namespace Dotnetable.Domain.Entities;

/// <summary>One map point for a post. <see cref="PostID"/> is both the primary key and the foreign key (1:1).</summary>
public partial class PostLocation
{
    public int PostID { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    /// <summary><see cref="Enums.PostLocationKind"/>. New kinds are appended; existing numbers stay.</summary>
    public byte Kind { get; set; }

    public string? Address { get; set; }

    public int? CityID { get; set; }

    public virtual City? City { get; set; }

    public virtual Post Post { get; set; } = null!;

    public virtual ICollection<PostLocationTranslation> PostLocationTranslations { get; set; } = new List<PostLocationTranslation>();
}
