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

/// <summary>
/// Commercial / physical condition of stock (return QC, registration, used listings).
/// Values 1–2 keep legacy return QC compatibility (1 was Sellable→New, 2 Defective).
/// </summary>
public enum StockItemCondition : byte
{
    /// <summary>Not applicable (non-return lines).</summary>
    None = 0,
    /// <summary>Brand-new / sealed — restock original sellable listing.</summary>
    New = 1,
    /// <summary>Legacy alias for <see cref="New"/> (older return QC “Sellable”).</summary>
    Sellable = 1,
    /// <summary>Defective / damaged scrap — warehouse receive only, not resold as used catalog.</summary>
    Defective = 2,
    /// <summary>Opened and barely used.</summary>
    LikeNew = 3,
    /// <summary>Box opened, contents complete.</summary>
    OpenBox = 4,
    /// <summary>Showroom / display sample (ویترینی).</summary>
    Display = 5,
    /// <summary>Used / pre-owned (کارکرده).</summary>
    Used = 6,
}

/// <summary>Health grade for non-new stock (required when condition is not New). 0 = n/a.</summary>
public enum StockHealthGrade : byte
{
    None = 0,
    /// <summary>Excellent.</summary>
    A = 1,
    /// <summary>Good.</summary>
    B = 2,
    /// <summary>Fair.</summary>
    C = 3,
    /// <summary>Poor / heavy wear.</summary>
    D = 4,
    /// <summary>Critical — usually scrap path.</summary>
    F = 5,
}

/// <summary>Obsolete name — use <see cref="StockItemCondition"/>.</summary>
public enum StockReturnCondition : byte
{
    None = 0,
    Sellable = 1,
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

public enum CustomerReturnStatus : byte
{
    PreRequest = 0,
    Rejected = 1,
    ApprovedAwaitingShipment = 2,
    Shipped = 3,
    Received = 4,
    Completed = 5,
    Cancelled = 6,
}

public enum CustomerReturnReason : byte
{
    Damaged = 1,
    WrongItem = 2,
    NotAsDescribed = 3,
    ChangedMind = 4,
    Defective = 5,
    Other = 9,
}

/// <summary>Who bears return shipping. Stored on the RMA so ledgers can split cost.</summary>
public enum ReturnShippingPayer : byte
{
    Unset = 0,
    /// <summary>Customer pays the carrier (or brings the parcel at their cost).</summary>
    Customer = 1,
    /// <summary>Customer and seller split the shipping cost 50/50.</summary>
    SplitFiftyFifty = 2,
    /// <summary>Seller / vendor pays. On a 1P shop the site is the seller.</summary>
    Seller = 3,
    /// <summary>Customer drops the goods at a store / return center — no carrier invoice.</summary>
    DropOffAtCenter = 4,
    /// <summary>Legacy alias — 1P shop paying is the same as <see cref="Seller"/>.</summary>
    Site = 3,
}

/// <summary>When the return countdown starts.</summary>
public enum ReturnWindowFrom : byte
{
    ShippedAt = 0,
    Delivered = 1,
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
