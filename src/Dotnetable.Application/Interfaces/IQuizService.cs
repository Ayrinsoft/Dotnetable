using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

public interface IQuizService
{
    Task<List<QuizCategoryListItemDto>> GetCategoriesAsync(int? websiteId, CancellationToken ct = default);
    Task<QuizCategoryEditorDto?> GetCategoryAsync(int categoryId, int? websiteId, CancellationToken ct = default);
    Task<(bool Ok, string? Error, int Id)> SaveCategoryAsync(QuizCategoryEditorDto model, CancellationToken ct = default);
    Task<string?> DeleteCategoryAsync(int categoryId, int? websiteId, CancellationToken ct = default);

    Task<List<QuizListItemDto>> GetQuizzesAsync(int? websiteId, CancellationToken ct = default);
    Task<QuizEditorDto?> GetQuizAsync(int quizId, int? websiteId, CancellationToken ct = default);
    Task<(bool Ok, string? Error, int Id)> SaveQuizAsync(QuizEditorDto model, CancellationToken ct = default);
    Task<string?> DeleteQuizAsync(int quizId, int? websiteId, CancellationToken ct = default);

    Task<List<QuizAttemptAdminDto>> GetAttemptsAsync(int quizId, int? websiteId, CancellationToken ct = default);
    Task<string?> MarkManualPaymentAsync(int attemptId, int? websiteId, CancellationToken ct = default);
    Task<string?> SendPrivateAnswerAsync(int attemptId, int? websiteId, string answer, CancellationToken ct = default);

    Task<IReadOnlyList<QuizPublicCategoryDto>> GetPublicCategoriesAsync(int websiteId, string? lang, bool postsOnly, CancellationToken ct = default);
    Task<QuizPublicCategoryDto?> GetPublicCategoryAsync(int websiteId, int categoryId, string? lang, CancellationToken ct = default);
    Task<QuizPublicDetailDto?> GetPublicQuizAsync(int websiteId, int quizId, string? lang, CancellationToken ct = default);

    Task<QuizActionResult> StartAttemptAsync(int websiteId, int quizId, int? clientId, string? lang, CancellationToken ct = default);
    Task<QuizAttemptViewDto?> GetAttemptAsync(int websiteId, Guid token, CancellationToken ct = default);
    Task<QuizActionResult> SubmitAttemptAsync(int websiteId, Guid token, QuizSubmitRequest request, CancellationToken ct = default);
    Task<QuizActionResult> RequestPrivateReviewAsync(int websiteId, Guid token, int? clientId, QuizPrivateReviewRequest request, CancellationToken ct = default);
    Task<QuizActionResult> CompletePrivatePaymentAsync(int websiteId, Guid token, GatewayCallback callback, CancellationToken ct = default);
}
