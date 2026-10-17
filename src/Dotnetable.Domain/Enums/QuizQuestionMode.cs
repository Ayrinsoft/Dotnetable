namespace Dotnetable.Domain.Enums;

/// <summary>How many of a test's questions a taker sees. Stored on <c>Quizzes.QuestionMode</c>.</summary>
public enum QuizQuestionMode : byte
{
    /// <summary>Every active question, in the author's sort order.</summary>
    All = 0,

    /// <summary>A random subset. The size is <c>Quizzes.RandomCount</c>.</summary>
    Random = 1,
}
