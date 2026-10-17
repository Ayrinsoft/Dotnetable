namespace Dotnetable.Domain.Entities;

public class QuizAttemptAnswer
{
    public int QuizAttemptAnswerID { get; set; }

    public int QuizAttemptID { get; set; }

    public int QuizQuestionID { get; set; }

    public string? TextAnswer { get; set; }

    public decimal PointsAwarded { get; set; }

    public virtual QuizAttempt QuizAttempt { get; set; } = null!;

    public virtual QuizQuestion QuizQuestion { get; set; } = null!;

    public virtual ICollection<QuizAttemptAnswerOption> SelectedOptions { get; set; } = new List<QuizAttemptAnswerOption>();
}
