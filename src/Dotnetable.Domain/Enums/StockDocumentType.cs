namespace Dotnetable.Domain.Enums;

public enum StockDocumentType : byte
{
    Inbound = 1,
    Outbound = 2,
    Transfer = 3,
    Adjustment = 4,
    Count = 5,
}

public enum StockDocumentStatus : byte
{
    Draft = 0,
    Submitted = 1,
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
