using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

public interface IInstagramAccountService
{
    /// <summary>The first connected channel, or an empty status when none is saved.</summary>
    Task<InstagramAccountStatus> GetAsync(int websiteId, CancellationToken ct = default);

    Task<IReadOnlyList<InstagramAccountStatus>> ListAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Checks the token with Instagram, then stores it. A blank token keeps the saved one. AccountId zero adds a channel.</summary>
    Task<InstagramAccountStatus> SaveAsync(int websiteId, InstagramAccountInput input, CancellationToken ct = default);

    Task DisconnectAsync(int websiteId, int accountId, CancellationToken ct = default);
}
