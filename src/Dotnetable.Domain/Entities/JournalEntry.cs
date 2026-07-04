using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class JournalEntry
{
    public int JournalEntrieID { get; set; }

    public int WebsiteID { get; set; }

    public string EntryNumber { get; set; } = null!;

    public DateOnly EntryDate { get; set; }

    public string? Description { get; set; }

    public byte? SourceType { get; set; }

    public int? SourceId { get; set; }

    public bool IsPosted { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();

    public virtual Website Website { get; set; } = null!;
}
