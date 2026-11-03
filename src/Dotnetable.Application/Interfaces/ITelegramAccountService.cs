using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

public interface ITelegramAccountService
{
    Task<TelegramAccountStatus> GetAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Checks the bot and the chat, then stores them. A blank token keeps the saved one.</summary>
    Task<TelegramAccountStatus> SaveAsync(int websiteId, TelegramAccountInput input, CancellationToken ct = default);

    Task DisconnectAsync(int websiteId, CancellationToken ct = default);
}
