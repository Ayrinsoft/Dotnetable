namespace Dotnetable.Infrastructure.Instagram;

public interface IInstagramGraph
{
    Task<string> GetUsernameAsync(string igUserId, string accessToken, CancellationToken ct = default);

    Task<string> CreateImageAsync(string igUserId, string accessToken, string imageUrl, string? caption, bool carouselItem, CancellationToken ct = default);

    Task<string> CreateVideoAsync(string igUserId, string accessToken, string videoUrl, string? caption, bool carouselItem, string? coverUrl, CancellationToken ct = default);

    Task<string> CreateCarouselAsync(string igUserId, string accessToken, IReadOnlyList<string> children, string caption, CancellationToken ct = default);

    Task WaitUntilReadyAsync(string containerId, string accessToken, bool video, CancellationToken ct = default);

    Task<string> PublishAsync(string igUserId, string accessToken, string creationId, CancellationToken ct = default);

    Task<string?> GetPermalinkAsync(string mediaId, string accessToken, CancellationToken ct = default);
}
