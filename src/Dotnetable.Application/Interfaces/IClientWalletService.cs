using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Manages a website customer's cash wallet (<see cref="ClientWallet"/>) and its append-only ledger
/// (<see cref="ClientWalletTransaction"/>). <see cref="ApplyAsync"/> is the ONLY way a wallet balance
/// may change — every other domain (Payment refunds, checkout wallet-pay, admin adjustments,
/// withdrawals) must go through it so the ledger always reconciles with <see cref="ClientWallet.BalanceUsd"/>.
/// </summary>
public interface IClientWalletService
{
    /// <summary>Returns the customer's wallet, creating a zero-balance active one if it doesn't exist yet.</summary>
    Task<ClientWallet> GetOrCreateAsync(int websiteId, int clientId, CancellationToken ct = default);

    /// <summary>Current balance in USD (0 when the customer has no wallet yet).</summary>
    Task<decimal> GetBalanceAsync(int clientId, CancellationToken ct = default);

    /// <summary>Server-side paged/sorted ledger history for one customer, newest first by default.</summary>
    Task<PagedResult<ClientWalletTransaction>> GetHistoryAsync(int clientId, GridQuery query, CancellationToken ct = default);

    /// <summary>
    /// Applies a signed change to the customer's wallet and appends the corresponding ledger row.
    /// This is the only method that may ever change <see cref="ClientWallet.BalanceUsd"/>.
    /// </summary>
    /// <param name="websiteId">Owning website, used only when the wallet must be created.</param>
    /// <param name="clientId">The customer whose wallet is affected.</param>
    /// <param name="type">A <see cref="ClientWalletTransactionType"/> value.</param>
    /// <param name="signedAmountUsd">Positive to credit, negative to debit.</param>
    /// <param name="sourceType">A <see cref="ClientWalletSourceType"/> value, or null.</param>
    /// <param name="sourceId">Polymorphic id paired with <paramref name="sourceType"/>, or null.</param>
    /// <param name="note">Optional free-text note stored on the ledger row.</param>
    /// <param name="memberId">Admin member id when the change was triggered by an admin action, or null.</param>
    /// <exception cref="InvalidOperationException">The debit would take the balance negative.</exception>
    Task<ClientWalletTransaction> ApplyAsync(
        int websiteId,
        int clientId,
        byte type,
        decimal signedAmountUsd,
        byte? sourceType,
        int? sourceId,
        string? note,
        int? memberId,
        CancellationToken ct = default);
}
