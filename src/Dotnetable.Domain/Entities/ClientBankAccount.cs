using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ClientBankAccount
{
    public int ClientBankAccountID { get; set; }

    public int WebsiteID { get; set; }

    public int WebsiteClientID { get; set; }

    public int? BankID { get; set; }

    public string OwnerName { get; set; } = null!;

    public string? AccountNumber { get; set; }

    public string? IBAN { get; set; }

    public string? CardNumber { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Bank? Bank { get; set; }

    public virtual ICollection<ClientWalletWithdrawal> ClientWalletWithdrawals { get; set; } = new List<ClientWalletWithdrawal>();

    public virtual Website Website { get; set; } = null!;

    public virtual WebsiteClient WebsiteClient { get; set; } = null!;
}
