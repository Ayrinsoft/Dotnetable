using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ClientWallet
{
    public int ClientWalletID { get; set; }

    public int WebsiteID { get; set; }

    public int WebsiteClientID { get; set; }

    /// <summary>
    /// Currency of this wallet ledger. One customer may hold multiple wallets (one per enabled currency).
    /// Balance is always stored only in this currency — never dual-stored as the authority.
    /// </summary>
    public string CurrencyCode { get; set; } = null!;

    /// <summary>Wallet balance in <see cref="CurrencyCode"/> (ledger authority).</summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Optional USD mirror for reporting only when the site dual-stores product prices.
    /// Not used as wallet authority; may be zero for pure single-currency wallets.
    /// </summary>
    public decimal BalanceUsd { get; set; }

    public bool IsActive { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Currency CurrencyCodeNavigation { get; set; } = null!;

    public virtual ICollection<ClientWalletTransaction> ClientWalletTransactions { get; set; } = new List<ClientWalletTransaction>();

    public virtual ICollection<ClientWalletWithdrawal> ClientWalletWithdrawals { get; set; } = new List<ClientWalletWithdrawal>();

    public virtual Website Website { get; set; } = null!;

    public virtual WebsiteClient WebsiteClient { get; set; } = null!;
}
