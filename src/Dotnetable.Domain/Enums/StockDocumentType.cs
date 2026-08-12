namespace Dotnetable.Domain.Enums;

public enum StockDocumentType : byte
{
    Inbound = 1,
    Outbound = 2,
    Transfer = 3,
    Adjustment = 4,
    Count = 5,
    /// <summary>Customer/vendor return after goods left the warehouse (linked to order / refund).</summary>
    Return = 6,
}

/// <summary>QC outcome on a return document line (restock vs scrap).</summary>
public enum StockReturnCondition : byte
{
    /// <summary>Not applicable (non-return lines).</summary>
    None = 0,
    /// <summary>Found OK — restock to sellable warehouse / listings.</summary>
    Sellable = 1,
    /// <summary>Defective / damaged — receive to warehouse for scrap tracking, do not restore sellable listings.</summary>
    Defective = 2,
}

public enum StockDocumentStatus : byte
{
    Draft = 0,
    Submitted = 1,
    /// <summary>Approved / ready to pick (worker queue).</summary>
    Approved = 2,
    Rejected = 3,
    Posted = 4,
    Cancelled = 5,
}

public enum PayrollRunStatus : byte
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Paid = 3,
    Cancelled = 4,
}

public enum EmployeeStatus : byte
{
    Active = 1,
    OnLeave = 2,
    Terminated = 3,
}
