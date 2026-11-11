using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

public interface IXAccountService
{
    /// <summary>The first connected channel, or an empty status when none is saved.</summary>
    Task<XAccountStatus> GetAsync(int websiteId, CancellationToken ct = default);

    Task<IReadOnlyList<XAccountStatus>> ListAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Checks the four keys with X, then stores them. A blank field keeps the saved value. AccountId zero adds a channel.</summary>
    Task<XAccountStatus> SaveAsync(int websiteId, XAccountInput input, CancellationToken ct = default);

    Task DisconnectAsync(int websiteId, int accountId, CancellationToken ct = default);
}
