using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class JournalEntryLine
{
    public int JournalEntryLineID { get; set; }

    public int JournalEntryID { get; set; }

    public int ChartOfAccountID { get; set; }

    public decimal Debit { get; set; }

    public decimal Credit { get; set; }

    public string? Description { get; set; }

    public virtual ChartOfAccount ChartOfAccount { get; set; } = null!;

    public virtual JournalEntry JournalEntry { get; set; } = null!;
}
