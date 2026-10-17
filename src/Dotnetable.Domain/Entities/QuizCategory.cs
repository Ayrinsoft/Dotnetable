namespace Dotnetable.Domain.Entities;

/// <summary>A group of tests the storefront can list from a menu or from the blog.</summary>
public class QuizCategory
{
    public int QuizCategoryID { get; set; }

    public int WebsiteID { get; set; }

    public int? ParentCategoryID { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    /// <summary>When set, the public blog index also lists this category beside posts.</summary>
    public bool ShowInPosts { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual QuizCategory? ParentCategory { get; set; }

    public virtual ICollection<QuizCategory> Children { get; set; } = new List<QuizCategory>();

    public virtual ICollection<QuizCategoryTranslation> Translations { get; set; } = new List<QuizCategoryTranslation>();

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}
