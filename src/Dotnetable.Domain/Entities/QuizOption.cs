namespace Dotnetable.Domain.Entities;

public class QuizOption
{
    public int QuizOptionID { get; set; }

    public int QuizQuestionID { get; set; }

    public string Text { get; set; } = null!;

    /// <summary>Answer key. Set when the author writes the question, never after the taker starts.</summary>
    public bool IsCorrect { get; set; }

    public int SortOrder { get; set; }

    public virtual QuizQuestion QuizQuestion { get; set; } = null!;
}
