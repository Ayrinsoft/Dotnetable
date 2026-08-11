using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Manages customer cash-out requests (<see cref="ClientWalletWithdrawal"/>). Requesting a withdrawal
/// immediately holds the funds via <see cref="IClientWalletService.ApplyAsync"/>
/// (<see cref="ClientWalletTransactionType.WithdrawalHold"/>); rejecting one reverses the hold
/// (<see cref="ClientWalletTransactionType.WithdrawalReversed"/>).
/// </summary>
public interface IClientWalletWithdrawalService
{
    /// <summary>
    /// Requests a cash-out. Validates that <paramref name="clientBankAccountId"/> belongs to the
    /// customer and that the wallet has sufficient balance, then holds the amount and inserts a
    /// <see cref="ClientWalletWithdrawalStatus.Pending"/> row.
    /// </summary>
    /// <exception cref="InvalidOperationException">Bank account not found/not owned by the customer.</exception>
    /// <param name="amount">Amount in <paramref name="currencyCode"/> (or site default when null).</param>
    Task<ClientWalletWithdrawal> RequestAsync(
        int websiteId, int clientId, int clientBankAccountId, decimal amount,
        string? currencyCode = null, CancellationToken ct = default);

    /// <summary>Server-side paged/sorted/searched withdrawal queue for the admin grid.
    /// <paramref name="websiteId"/> null = all websites (master only).</summary>
    Task<PagedResult<ClientWalletWithdrawal>> GetPagedAsync(int? websiteId, byte? status, GridQuery query, CancellationToken ct = default);

    /// <summary>Counts of withdrawals per <see cref="ClientWalletWithdrawal"/>.Status for the optional website scope.</summary>
    Task<IReadOnlyDictionary<byte, int>> GetStatusCountsAsync(int? websiteId, CancellationToken ct = default);

    /// <summary>Own withdrawal history for a customer (client-facing).</summary>
    Task<PagedResult<ClientWalletWithdrawal>> GetByClientIdAsync(int clientId, GridQuery query, CancellationToken ct = default);

    /// <summary>Marks a pending withdrawal as paid, recording who reviewed it and the payment reference.</summary>
    Task<bool> ApproveAsync(int withdrawalId, int memberId, string? paymentRefNumber, CancellationToken ct = default);

    /// <summary>Rejects a pending withdrawal and reverses the held funds back into the wallet.</summary>
    Task<bool> RejectAsync(int withdrawalId, int memberId, string reason, CancellationToken ct = default);
}
