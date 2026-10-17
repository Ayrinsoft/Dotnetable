namespace Dotnetable.Domain.Entities;

public class QuizTranslation
{
    public int QuizTranslationID { get; set; }

    public int QuizID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string? Title { get; set; }

    public string? Summary { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;
}
