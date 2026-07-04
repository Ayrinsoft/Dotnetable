using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Values shared by <see cref="ProductReview"/>.Status and <see cref="ProductQuestion"/>/<see cref="ProductAnswer"/>.Status.</summary>
public enum ModerationStatus : byte
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
}

/// <summary>
/// Customer product reviews with star ratings, moderation, and like/dislike voting. Approving/deleting
/// a review keeps <see cref="Product.AvgRating"/>/<see cref="Product.RatingCount"/> in sync.
/// </summary>
public interface IProductReviewService
{
    Task<ProductReview> SubmitAsync(
        int websiteId, int clientId, int productId, int? variantId, byte rating,
        string? title, string body, string? prosJson, string? consJson, CancellationToken ct = default);

    Task<PagedResult<ProductReview>> GetApprovedAsync(int productId, GridQuery query, CancellationToken ct = default);

    Task<PagedResult<ProductReview>> GetPendingAsync(int? websiteId, GridQuery query, CancellationToken ct = default);

    Task<bool> ModerateAsync(int reviewId, bool approve, CancellationToken ct = default);

    Task<bool> DeleteAsync(int reviewId, CancellationToken ct = default);

    Task LikeAsync(int reviewId, CancellationToken ct = default);

    Task DislikeAsync(int reviewId, CancellationToken ct = default);
}
