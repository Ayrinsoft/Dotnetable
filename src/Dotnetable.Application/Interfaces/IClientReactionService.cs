using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Likes (favorites for products) and bookmarks of signed-in customers on products, posts and pages.
/// A like is counted once per customer: the counter only moves when a row is actually inserted or
/// deleted. A product favorite is kept in step with the wishlist — favoriting adds the product's
/// default variant to the wishlist, un-favoriting removes the product from it.
/// </summary>
public interface IClientReactionService
{
    /// <summary>Current counter and the caller's own state. Null when the target is not a
    /// published item of <paramref name="websiteId"/>.</summary>
    Task<ReactionStateDto?> GetStateAsync(int websiteId, ReactionTargetType targetType, int targetId, int? clientId, CancellationToken ct = default);

    /// <summary>Turns a like or bookmark on or off (idempotent) and returns the new state.
    /// Null when the target is not a published item of <paramref name="websiteId"/>.</summary>
    Task<ReactionStateDto?> SetAsync(int websiteId, int clientId, ReactionTargetType targetType, int targetId,
        ReactionType reaction, bool on, CancellationToken ct = default);

    /// <summary>The customer's bookmarks (or likes), newest first, optionally of one target type.
    /// Items that are no longer published are left out.</summary>
    Task<PagedResult<ClientReactionItemDto>> GetListAsync(int websiteId, int clientId, ReactionType reaction,
        ReactionTargetType? targetType, int page, int pageSize, string? languageCode = null, CancellationToken ct = default);
}
