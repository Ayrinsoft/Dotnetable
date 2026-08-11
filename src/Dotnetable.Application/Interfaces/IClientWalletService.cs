using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Customer cash wallets: one ledger per enabled currency per customer.
/// <see cref="ApplyAsync"/> is the only way balances change. Amounts are always in the wallet's own
/// <see cref="ClientWallet.CurrencyCode"/> — never dual-stored as authority (unlike product dual-USD).
/// </summary>
public interface IClientWalletService
{
    /// <summary>
    /// Wallet currencies the website allows. Ensures the site default currency is present and active.
    /// </summary>
    Task<IReadOnlyList<WebsiteWalletCurrency>> GetEnabledWalletCurrenciesAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Enable an additional wallet currency for the website (must exist in Currencies and typically have a rate).</summary>
    Task<(bool Success, string? Error)> EnableWalletCurrencyAsync(int websiteId, string currencyCode, CancellationToken ct = default);

    /// <summary>Deactivate a non-default wallet currency (blocked if any positive balances remain).</summary>
    Task<(bool Success, string? Error)> DisableWalletCurrencyAsync(int websiteId, string currencyCode, CancellationToken ct = default);

    /// <summary>
    /// Returns the customer's wallet for <paramref name="currencyCode"/> (or site default when null),
    /// creating a zero-balance active wallet when missing and the currency is enabled.
    /// </summary>
    Task<ClientWallet> GetOrCreateAsync(int websiteId, int clientId, string? currencyCode = null, CancellationToken ct = default);

    /// <summary>All wallet accounts for a customer on a website.</summary>
    Task<IReadOnlyList<ClientWallet>> ListForClientAsync(int websiteId, int clientId, CancellationToken ct = default);

    /// <summary>Balance in the given currency (0 when no wallet). Null currency = site default.</summary>
    Task<decimal> GetBalanceAsync(int websiteId, int clientId, string? currencyCode = null, CancellationToken ct = default);

    /// <summary>Ledger history for one wallet currency (null = default). Newest first by default.</summary>
    Task<PagedResult<ClientWalletTransaction>> GetHistoryAsync(
        int websiteId, int clientId, GridQuery query, string? currencyCode = null, CancellationToken ct = default);

    /// <summary>
    /// Applies a signed change in the wallet currency and appends a ledger row.
    /// </summary>
    /// <param name="signedAmount">Signed amount in <paramref name="currencyCode"/> (or site default).</param>
    /// <exception cref="InvalidOperationException">Debit would go negative, or currency not enabled.</exception>
    Task<ClientWalletTransaction> ApplyAsync(
        int websiteId,
        int clientId,
        byte type,
        decimal signedAmount,
        byte? sourceType,
        int? sourceId,
        string? note,
        int? memberId,
        string? currencyCode = null,
        CancellationToken ct = default);
}
