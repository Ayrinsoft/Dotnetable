namespace Dotnetable.Domain.Enums;

/// <summary>Values for <see cref="Entities.VendorCreditTransaction.SourceType"/>.</summary>
public enum VendorCreditSourceType : byte
{
    Grant = 1,
    Sale = 2,
    Adjustment = 3,
    Refund = 4,
}
