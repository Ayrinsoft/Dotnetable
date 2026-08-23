namespace Dotnetable.Domain.Enums;

public enum StaffTaskStatus : byte
{
    Open = 0,
    InProgress = 1,
    Done = 2,
    Cancelled = 3,
}

public enum StaffTaskPriority : byte
{
    Normal = 0,
    Low = 1,
    High = 2,
    Urgent = 3,
}

/// <summary>What a staff task is about. Stock kinds all point at <c>StockDocument</c>.</summary>
public enum StaffTaskRelatedKind : byte
{
    None = 0,
    Order = 1,
    StockInbound = 2,
    StockOutbound = 3,
    StockTransfer = 4,
    StockAdjustment = 5,
    StockCount = 6,
    StockReturn = 7,
    CustomerReturn = 8,
    Payment = 9,
    PaymentRefund = 10,
}
