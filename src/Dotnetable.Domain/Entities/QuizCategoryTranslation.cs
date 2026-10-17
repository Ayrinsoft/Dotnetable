namespace Dotnetable.Domain.Entities;

public class QuizCategoryTranslation
{
    public int QuizCategoryTranslationID { get; set; }

    public int QuizCategoryID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string? Title { get; set; }

    public string? Description { get; set; }

    public virtual QuizCategory QuizCategory { get; set; } = null!;
}
