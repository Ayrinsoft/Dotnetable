namespace Dotnetable.Domain.Entities;

public class QuizAttemptAnswerOption
{
    public int QuizAttemptAnswerID { get; set; }

    public int QuizOptionID { get; set; }

    public virtual QuizAttemptAnswer QuizAttemptAnswer { get; set; } = null!;

    public virtual QuizOption QuizOption { get; set; } = null!;
}
