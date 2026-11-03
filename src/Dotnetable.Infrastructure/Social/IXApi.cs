namespace Dotnetable.Infrastructure.Social;

public interface IXApi
{
    Task<string> GetUsernameAsync(XCredentials credentials, CancellationToken ct = default);

    Task<string> UploadImageAsync(XCredentials credentials, byte[] jpeg, CancellationToken ct = default);

    Task<string> UploadVideoAsync(XCredentials credentials, byte[] content, string mime, CancellationToken ct = default);

    Task<string> TweetAsync(XCredentials credentials, string? text, IReadOnlyList<string> mediaIds, string? replyTo, CancellationToken ct = default);
}
