namespace Dotnetable.Domain.Entities;

/// <summary>The questions this sitting actually shows. Frozen at start so a random draw does not change.</summary>
public class QuizAttemptQuestion
{
    public int QuizAttemptID { get; set; }

    public int QuizQuestionID { get; set; }

    public int SortOrder { get; set; }

    public virtual QuizAttempt QuizAttempt { get; set; } = null!;

    public virtual QuizQuestion QuizQuestion { get; set; } = null!;
}
