using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

public interface IInstagramAccountService
{
    Task<InstagramAccountStatus> GetAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Checks the token with Instagram, then stores it. A blank token keeps the saved one.</summary>
    Task<InstagramAccountStatus> SaveAsync(int websiteId, InstagramAccountInput input, CancellationToken ct = default);

    Task DisconnectAsync(int websiteId, CancellationToken ct = default);
}
