using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Product Q&amp;A: customers ask questions, other customers or the selling vendor can answer, with moderation.</summary>
public interface IProductQuestionService
{
    Task<ProductQuestion> AskAsync(int websiteId, int clientId, int productId, string body, CancellationToken ct = default);

    Task<ProductAnswer> AnswerAsync(int questionId, int? clientId, int? vendorId, string body, CancellationToken ct = default);

    Task<PagedResult<ProductQuestion>> GetApprovedAsync(int productId, GridQuery query, CancellationToken ct = default);

    Task<PagedResult<ProductQuestion>> GetPendingAsync(int? websiteId, GridQuery query, CancellationToken ct = default);

    Task<bool> ModerateQuestionAsync(int questionId, bool approve, CancellationToken ct = default);

    Task<bool> ModerateAnswerAsync(int answerId, bool approve, CancellationToken ct = default);

    Task LikeAnswerAsync(int answerId, CancellationToken ct = default);
}
