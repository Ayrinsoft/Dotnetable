namespace Dotnetable.Domain.Entities;

/// <summary>Translated address for a <see cref="PostLocation"/>. Coordinates stay on the parent row.</summary>
public partial class PostLocationTranslation
{
    public int PostLocationTranslationID { get; set; }

    public int PostID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string? Address { get; set; }

    public virtual PostLocation PostLocation { get; set; } = null!;
}
