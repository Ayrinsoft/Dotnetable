using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

public interface IInstagramPublishService
{
    /// <summary>Permalink for each post that was already sent. Missing keys were not sent.</summary>
    Task<IReadOnlyDictionary<int, string?>> GetLinksAsync(IReadOnlyCollection<int> postIds, CancellationToken ct = default);

    /// <summary>Publishes once. Temporary hosted files and the outbox file are removed when the call finishes.</summary>
    Task<string?> PublishAsync(int postId, InstagramPublishRequest request, CancellationToken ct = default);
}
