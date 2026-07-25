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

    public byte AccountType { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<ChartOfAccount> InverseParentAccount { get; set; } = new List<ChartOfAccount>();

    public virtual ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();

    public virtual ChartOfAccount? ParentAccount { get; set; }

    public virtual Website Website { get; set; } = null!;
}
