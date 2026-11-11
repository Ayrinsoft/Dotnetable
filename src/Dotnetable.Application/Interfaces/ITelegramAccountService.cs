using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

public interface ITelegramAccountService
{
    /// <summary>The first connected channel, or an empty status when none is saved.</summary>
    Task<TelegramAccountStatus> GetAsync(int websiteId, CancellationToken ct = default);

    Task<IReadOnlyList<TelegramAccountStatus>> ListAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Checks the bot and the chat, then stores them. A blank token keeps the saved one. AccountId zero adds a channel.</summary>
    Task<TelegramAccountStatus> SaveAsync(int websiteId, TelegramAccountInput input, CancellationToken ct = default);

    Task DisconnectAsync(int websiteId, int accountId, CancellationToken ct = default);
}
