using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Result of an attempt to add a new bank account for a customer.</summary>
public enum BankAccountSaveResult
{
    Success,
    LimitReached,
    NotFound,
}

/// <summary>
/// Manages a website customer's saved bank accounts (<see cref="ClientBankAccount"/>). Every operation
/// is scoped to the owning <see cref="WebsiteClient.WebsiteClientID"/> so a customer (or admin acting
/// on their behalf) can never read or modify another customer's bank accounts. Each customer may hold
/// at most <see cref="AppConstants.MaxClientBankAccounts"/> bank accounts.
/// </summary>
public interface IClientBankAccountService
{
    Task<List<ClientBankAccount>> GetByClientIdAsync(int clientId, CancellationToken ct = default);

    /// <summary>Returns the bank account only when it belongs to <paramref name="clientId"/>.</summary>
    Task<ClientBankAccount?> GetByIdAsync(int bankAccountId, int clientId, CancellationToken ct = default);

    /// <summary>Adds a new bank account. Fails with <see cref="BankAccountSaveResult.LimitReached"/> once
    /// the customer already has <see cref="AppConstants.MaxClientBankAccounts"/> saved bank accounts.</summary>
    Task<BankAccountSaveResult> CreateAsync(ClientBankAccount account, CancellationToken ct = default);

    /// <summary>Updates an existing bank account; fails with <see cref="BankAccountSaveResult.NotFound"/>
    /// when it doesn't exist or doesn't belong to <paramref name="account"/>'s
    /// <see cref="ClientBankAccount.WebsiteClientID"/>.</summary>
    Task<BankAccountSaveResult> UpdateAsync(ClientBankAccount account, CancellationToken ct = default);

    Task<bool> DeleteAsync(int bankAccountId, int clientId, CancellationToken ct = default);

    /// <summary>Marks one bank account as the customer's default and clears the flag on all others.</summary>
    Task<bool> SetDefaultAsync(int bankAccountId, int clientId, CancellationToken ct = default);
}
