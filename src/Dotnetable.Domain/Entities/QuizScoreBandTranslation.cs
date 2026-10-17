namespace Dotnetable.Domain.Entities;

public class QuizScoreBandTranslation
{
    public int QuizScoreBandTranslationID { get; set; }

    public int QuizScoreBandID { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string? Title { get; set; }

    public string? Body { get; set; }

    public virtual QuizScoreBand QuizScoreBand { get; set; } = null!;
}
