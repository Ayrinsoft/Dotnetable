namespace Dotnetable.Infrastructure.Social;

public sealed record TelegramOutbound(bool IsVideo, byte[] Bytes, string FileName, string? MimeType = null);

public interface ITelegramBot
{
    Task<string> GetUsernameAsync(string botToken, CancellationToken ct = default);

    Task EnsureChatAsync(string botToken, string chatId, CancellationToken ct = default);

    Task<string?> SendTextAsync(string botToken, string chatId, string text, CancellationToken ct = default, string? previewUrl = null);

    Task<string?> SendPhotoAsync(string botToken, string chatId, TelegramOutbound photo, string? caption, CancellationToken ct = default);

    Task<string?> SendVideoAsync(string botToken, string chatId, TelegramOutbound video, string? caption, CancellationToken ct = default);

    Task<string?> SendAlbumAsync(string botToken, string chatId, IReadOnlyList<TelegramOutbound> items, string? caption, CancellationToken ct = default);
}
