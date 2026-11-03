using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

public interface IXAccountService
{
    Task<XAccountStatus> GetAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Checks the four keys with X, then stores them. A blank field keeps the saved value.</summary>
    Task<XAccountStatus> SaveAsync(int websiteId, XAccountInput input, CancellationToken ct = default);

    Task DisconnectAsync(int websiteId, CancellationToken ct = default);
}
