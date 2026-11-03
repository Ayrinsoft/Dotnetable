using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

public interface ISocialPublishService
{
    /// <summary>Posts that have been sent to at least one network. Missing keys were never sent.</summary>
    Task<IReadOnlyDictionary<int, SocialSentFlags>> GetSentAsync(IReadOnlyCollection<int> postIds, CancellationToken ct = default);

    /// <summary>Sends the checked networks. One failure does not cancel the others. A previous send does not block a new one.</summary>
    Task<SocialPublishResult> PublishAsync(int postId, InstagramPublishRequest request, CancellationToken ct = default);

    /// <summary>Products that have been sent to at least one network. Missing keys were never sent.</summary>
    Task<IReadOnlyDictionary<int, SocialSentFlags>> GetProductSentAsync(IReadOnlyCollection<int> productIds, CancellationToken ct = default);

    /// <summary>Sends a product with the same network and per-file choices as a post. The short description is the summary.</summary>
    Task<SocialPublishResult> PublishProductAsync(int productId, InstagramPublishRequest request, CancellationToken ct = default);
}
