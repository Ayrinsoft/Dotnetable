using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

public interface ISetupService
{
    /// <summary>True once the master website (and its administrator) has been created.</summary>
    Task<bool> IsSetupCompletedAsync(CancellationToken ct = default);

    /// <summary>
    /// Creates the master website (WebsiteID 1), the administrator policy with full access,
    /// and the first administrator member. Throws if setup has already been completed.
    /// </summary>
    Task CompleteSetupAsync(SetupRequest request, CancellationToken ct = default);
}
