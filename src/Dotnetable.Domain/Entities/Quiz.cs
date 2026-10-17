namespace Dotnetable.Domain.Entities;

/// <summary>One timed test. The author stores the answer key with each question.</summary>
public class Quiz
{
    public int QuizID { get; set; }

    public int WebsiteID { get; set; }

    public int QuizCategoryID { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Summary { get; set; }

    /// <summary>Whole sitting, shown as a countdown to the taker.</summary>
    public int DurationMinutes { get; set; }

    public byte QuestionMode { get; set; }

    /// <summary>How many questions to draw when <see cref="QuestionMode"/> is random.</summary>
    public int? RandomCount { get; set; }

    public bool IsPublished { get; set; }

    /// <summary>When set, the taker may ask for a specialist's private write-up after the score.</summary>
    public bool OfferPrivateReview { get; set; }

    public decimal PrivateReviewFee { get; set; }

    public string? CurrencyCode { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual QuizCategory QuizCategory { get; set; } = null!;

    public virtual ICollection<QuizTranslation> Translations { get; set; } = new List<QuizTranslation>();

    public virtual ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();

    public virtual ICollection<QuizScoreBand> ScoreBands { get; set; } = new List<QuizScoreBand>();

    public virtual ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
}
