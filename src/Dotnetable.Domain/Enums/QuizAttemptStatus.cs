namespace Dotnetable.Domain.Enums;

/// <summary>Lifecycle of one sitting. Stored on <c>QuizAttempts.Status</c>.</summary>
public enum QuizAttemptStatus : byte
{
    InProgress = 0,
    Submitted = 1,
    Expired = 2,
}
