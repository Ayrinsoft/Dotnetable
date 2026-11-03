namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Tells IndexNow (Bing and the other engines on the protocol) that public URLs changed.
/// Implementations must not fail the content save when the remote call fails.
/// </summary>
public interface IIndexNowNotifier
{
    Task NotifyPageAsync(int pageId, CancellationToken ct = default);

    Task NotifyPostAsync(int postId, CancellationToken ct = default);

    Task NotifyProductAsync(int productId, CancellationToken ct = default);

    /// <summary>Submits site-relative paths such as <c>/</c> or <c>/page/about</c>.</summary>
    Task NotifyPathsAsync(int websiteId, IReadOnlyCollection<string> relativePaths, CancellationToken ct = default);
}
