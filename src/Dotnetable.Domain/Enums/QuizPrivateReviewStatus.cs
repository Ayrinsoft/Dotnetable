namespace Dotnetable.Domain.Enums;

/// <summary>
/// Optional specialist write-up for one sitting. The author turns the offer on per test
/// and sets the fee. Stored on <c>QuizAttempts.PrivateReviewStatus</c>.
/// </summary>
public enum QuizPrivateReviewStatus : byte
{
    None = 0,
    AwaitingPayment = 1,
    PaidAwaitingAnswer = 2,
    ManualRequested = 3,
    Delivered = 4,
}
