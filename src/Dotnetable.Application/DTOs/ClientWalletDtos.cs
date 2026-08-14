namespace Dotnetable.Application.DTOs;

/// <summary>
/// Ledger entry kind for <c>ClientWalletTransactions.Type</c> (see Dotnetable.Database\ClientWalletTransactions.sql).
/// Values are fixed by the DB comment — do not renumber.
/// </summary>
public enum ClientWalletTransactionType : byte
{
    RefundCredit = 1,
    PurchaseUse = 2,
    WithdrawalHold = 3,
    WithdrawalReversed = 4,
    AdminAdjustment = 5,
    /// <summary>Admin-recorded incoming money (wallet top-up / deposit).</summary>
    AdminDeposit = 6,
}

/// <summary>
/// Polymorphic pointer kind for <c>ClientWalletTransactions.SourceType</c>/<c>SourceId</c> (unenforced,
/// like JournalEntries.SourceType/SourceId). Values are fixed by the DB comment — do not renumber.
/// </summary>
public enum ClientWalletSourceType : byte
{
    PaymentRefund = 1,
    Payment = 2,
    ClientWalletWithdrawal = 3,
    AdminManual = 4,
}

/// <summary>
/// Status for <c>ClientWalletWithdrawals.Status</c> (see Dotnetable.Database\ClientWalletWithdrawals.sql).
/// Values are fixed by the DB comment — do not renumber.
/// </summary>
public enum ClientWalletWithdrawalStatus : byte
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Paid = 4,
}

/// <summary>Withdrawal statuses that still need admin action (excludes terminal Paid / Rejected / Approved).</summary>
public static class ClientWalletWithdrawalStatusQueues
{
    public static readonly ClientWalletWithdrawalStatus[] Actionable =
    [
        ClientWalletWithdrawalStatus.Pending,
    ];
}

/// <summary>A customer bank account as returned by the API — flattened, no navigation cycles.</summary>
public sealed class ClientBankAccountDto
{
    public int ClientBankAccountID { get; set; }
    public int? BankID { get; set; }
    public string? BankName { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public string? IBAN { get; set; }
    public string? CardNumber { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>Payload for creating or updating a customer bank account.</summary>
public sealed class ClientBankAccountRequest
{
    public int? BankID { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public string? IBAN { get; set; }
    public string? CardNumber { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>One currency wallet balance for the signed-in customer.</summary>
public sealed class WalletBalanceDto
{
    public string CurrencyCode { get; set; } = string.Empty;
    /// <summary>Balance in <see cref="CurrencyCode"/>.</summary>
    public decimal Balance { get; set; }
    /// <summary>Obsolete alias — same as <see cref="Balance"/> (kept for older clients).</summary>
    public decimal BalanceUsd { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefaultCurrency { get; set; }
}

/// <summary>A single wallet ledger row as returned by the API — flattened, no navigation cycles.</summary>
public sealed class WalletTransactionDto
{
    public int ClientWalletTransactionID { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public byte Type { get; set; }
    /// <summary>Signed amount in wallet currency.</summary>
    public decimal Amount { get; set; }
    /// <summary>Obsolete alias for <see cref="Amount"/>.</summary>
    public decimal AmountUsd { get; set; }
    public decimal BalanceAfter { get; set; }
    /// <summary>Obsolete alias for <see cref="BalanceAfter"/>.</summary>
    public decimal BalanceAfterUsd { get; set; }
    public byte? SourceType { get; set; }
    public int? SourceId { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Payload to request a cash-out withdrawal from a specific currency wallet.</summary>
public sealed class WithdrawalRequest
{
    public int ClientBankAccountId { get; set; }
    /// <summary>Amount in <see cref="CurrencyCode"/> (or site default when omitted).</summary>
    public decimal Amount { get; set; }
    /// <summary>Obsolete alias for <see cref="Amount"/>.</summary>
    public decimal AmountUsd { get; set; }
    /// <summary>Wallet currency; null = site default.</summary>
    public string? CurrencyCode { get; set; }
}

/// <summary>A withdrawal request as returned by the API/admin grid — flattened, no navigation cycles.</summary>
public sealed class WithdrawalDto
{
    public int ClientWalletWithdrawalID { get; set; }
    public int WebsiteID { get; set; }
    public int WebsiteClientID { get; set; }
    public string? ClientName { get; set; }
    public int ClientBankAccountID { get; set; }
    public string? BankAccountSummary { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    /// <summary>Obsolete alias for <see cref="Amount"/>.</summary>
    public decimal AmountUsd { get; set; }
    public byte Status { get; set; }
    public int? ReviewedByMemberID { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectReason { get; set; }
    public string? PaymentRefNumber { get; set; }
    public string? Note { get; set; }
    public int? CreatedByMemberID { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime RequestedAt { get; set; }
}

/// <summary>Wallet currency enabled on a website.</summary>
public sealed class WebsiteWalletCurrencyDto
{
    public string CurrencyCode { get; set; } = string.Empty;
    public string? CurrencyName { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
}
