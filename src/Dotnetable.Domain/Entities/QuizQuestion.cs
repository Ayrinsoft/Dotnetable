namespace Dotnetable.Domain.Entities;

public class QuizQuestion
{
    public int QuizQuestionID { get; set; }

    public int QuizID { get; set; }

    public string Prompt { get; set; } = null!;

    public decimal Points { get; set; }

    /// <summary>False = one option. True = any number of options.</summary>
    public bool AllowMultiple { get; set; }

    /// <summary>A free-text answer, alone or next to the options.</summary>
    public bool AllowText { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();

    public virtual ICollection<QuizQuestionImage> Images { get; set; } = new List<QuizQuestionImage>();
}
