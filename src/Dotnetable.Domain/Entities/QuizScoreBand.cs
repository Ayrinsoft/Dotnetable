namespace Dotnetable.Domain.Entities;

/// <summary>The general result shown for a percent range, for example 0–40 and 41–100.</summary>
public class QuizScoreBand
{
    public int QuizScoreBandID { get; set; }

    public int QuizID { get; set; }

    public decimal MinPercent { get; set; }

    public decimal MaxPercent { get; set; }

    public string Title { get; set; } = null!;

    public string Body { get; set; } = null!;

    public int SortOrder { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual ICollection<QuizScoreBandTranslation> Translations { get; set; } = new List<QuizScoreBandTranslation>();
}
