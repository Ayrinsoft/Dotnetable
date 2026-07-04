using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ClientWallet
{
    public int ClientWalletID { get; set; }

    public int WebsiteID { get; set; }

    public int WebsiteClientID { get; set; }

    public decimal BalanceUsd { get; set; }

    public bool IsActive { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ClientWalletTransaction> ClientWalletTransactions { get; set; } = new List<ClientWalletTransaction>();

    public virtual ICollection<ClientWalletWithdrawal> ClientWalletWithdrawals { get; set; } = new List<ClientWalletWithdrawal>();

    public virtual Website Website { get; set; } = null!;

    public virtual WebsiteClient WebsiteClient { get; set; } = null!;
}
