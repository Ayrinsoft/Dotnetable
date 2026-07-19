using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>The website's own bank accounts, used to receive manual/offline customer bank transfers.</summary>
public interface IBankAccountService
{
    Task<List<BankAccount>> GetByWebsiteAsync(int websiteId, CancellationToken ct = default);
    Task<PagedResult<BankAccount>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default);
    Task<BankAccount?> GetByIdAsync(int bankAccountId, CancellationToken ct = default);
    Task<BankAccount> CreateAsync(BankAccount account, CancellationToken ct = default);
    Task<bool> UpdateAsync(BankAccount account, CancellationToken ct = default);
    Task<bool> DeleteAsync(int bankAccountId, int websiteId, CancellationToken ct = default);

    /// <summary>Active accounts a customer may transfer to at checkout.</summary>
    Task<List<BankAccount>> GetOfflinePaymentAccountsAsync(int websiteId, CancellationToken ct = default);
}
