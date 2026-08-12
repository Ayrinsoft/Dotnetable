namespace Dotnetable.Application.DTOs;

public sealed class ChartAccountDto
{
    public int ChartOfAccountID { get; init; }
    public int? ParentAccountID { get; init; }
    public string Code { get; init; } = "";
    public string Name { get; init; } = "";
    public byte AccountType { get; init; }
    public bool IsActive { get; init; }
    public bool IsSystem { get; init; }
    public int SortOrder { get; init; }
    public IReadOnlyList<ChartAccountDto> Children { get; init; } = Array.Empty<ChartAccountDto>();
}

public sealed class JournalLineDto
{
    public int ChartOfAccountID { get; set; }
    public string? AccountCode { get; set; }
    public string? AccountName { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Description { get; set; }
}

public sealed class JournalEntryDto
{
    public int JournalEntryID { get; init; }
    public string EntryNumber { get; init; } = "";
    public DateOnly EntryDate { get; init; }
    public string? Description { get; init; }
    public string? SourceType { get; init; }
    public string? SourceKey { get; init; }
    public string CurrencyCode { get; init; } = "";
    public bool ReportToTax { get; init; }
    public bool IsPosted { get; init; }
    public bool IsReversed { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? PostedAt { get; init; }
    public decimal TotalDebit { get; init; }
    public decimal TotalCredit { get; init; }
    public IReadOnlyList<JournalLineDto> Lines { get; init; } = Array.Empty<JournalLineDto>();
}

public sealed class TrialBalanceRowDto
{
    public int ChartOfAccountID { get; init; }
    public string Code { get; init; } = "";
    public string Name { get; init; } = "";
    public byte AccountType { get; init; }
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public decimal Balance { get; init; }
}

public sealed class ProfitAndLossDto
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public string CurrencyCode { get; init; } = "";
    public IReadOnlyList<TrialBalanceRowDto> Income { get; init; } = Array.Empty<TrialBalanceRowDto>();
    public IReadOnlyList<TrialBalanceRowDto> Expenses { get; init; } = Array.Empty<TrialBalanceRowDto>();
    public decimal TotalIncome { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal NetProfit { get; init; }
}
