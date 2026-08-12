using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ChartOfAccount
{
    public int ChartOfAccountID { get; set; }

    public int WebsiteID { get; set; }

    public int? ParentAccountID { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    /// <summary><see cref="Enums.GlAccountType"/>.</summary>
    public byte AccountType { get; set; }

    public bool IsActive { get; set; }

    /// <summary>Seeded system accounts should not be deleted.</summary>
    public bool IsSystem { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<ChartOfAccount> InverseParentAccount { get; set; } = new List<ChartOfAccount>();

    public virtual ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();

    public virtual ICollection<LedgerAccountMap> LedgerAccountMapCredits { get; set; } = new List<LedgerAccountMap>();

    public virtual ICollection<LedgerAccountMap> LedgerAccountMapDebits { get; set; } = new List<LedgerAccountMap>();

    public virtual ChartOfAccount? ParentAccount { get; set; }

    public virtual Website Website { get; set; } = null!;
}
