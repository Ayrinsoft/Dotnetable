namespace Dotnetable.Domain.Entities;

/// <summary>
/// Maps operational ledger <c>TransactionType</c> (+ optional Flow) to CoA debit/credit accounts
/// for automatic journal projection.
/// </summary>
public partial class LedgerAccountMap
{
    public int LedgerAccountMapID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>Matches <c>FinancialLedgerEntry.TransactionType</c>.</summary>
    public string TransactionType { get; set; } = null!;

    /// <summary>Optional flow filter (1 In / 2 Out / 3 Component). Null = any.</summary>
    public byte? Flow { get; set; }

    public int DebitAccountID { get; set; }

    public int CreditAccountID { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ChartOfAccount DebitAccount { get; set; } = null!;

    public virtual ChartOfAccount CreditAccount { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;
}
