using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.Interfaces;

/// <summary>CRUD over a website's outgoing-mail identities (the <c>EmailAccount</c> table).</summary>
public interface IEmailAccountService
{
    /// <summary>All accounts registered for the website, most recently added first.</summary>
    Task<List<EmailAccountInfo>> GetForWebsiteAsync(int websiteId, CancellationToken ct = default);

    Task<EmailAccountInfo?> GetByIdAsync(int emailAccountId, CancellationToken ct = default);

    /// <summary>Creates the account when EmailAccountID is 0, otherwise updates it. Returns its id.</summary>
    Task<int> SaveAsync(EmailAccountInfo account, CancellationToken ct = default);

    Task DeleteAsync(int emailAccountId, CancellationToken ct = default);

    /// <summary>True when the website (or the master website, as fallback) has a usable account.</summary>
    Task<bool> IsConfiguredAsync(int websiteId, CancellationToken ct = default);
}
