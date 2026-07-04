using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Bank
{
    public int BankID { get; set; }

    public int? WebsiteID { get; set; }

    public string Name { get; set; } = null!;

    public int? LogoFileID { get; set; }

    public bool Active { get; set; }

    public string BankCode { get; set; } = null!;

    public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();

    public virtual ICollection<ClientBankAccount> ClientBankAccounts { get; set; } = new List<ClientBankAccount>();

    public virtual FileRecord? LogoFile { get; set; }

    public virtual Website? Website { get; set; }
}
