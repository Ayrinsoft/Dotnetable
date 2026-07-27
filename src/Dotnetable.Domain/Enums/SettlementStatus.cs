namespace Dotnetable.Domain.Enums;

/// <summary>Values for <see cref="Entities.Settlement.Status"/>.</summary>
public enum SettlementStatus : byte
{
    Draft = 0,
    Open = 1,
    Approved = 2,
    Paid = 3,
    Cancelled = 4,
}
