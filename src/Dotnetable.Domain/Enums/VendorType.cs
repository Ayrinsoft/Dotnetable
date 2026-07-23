namespace Dotnetable.Domain.Enums;

/// <summary>
/// How a marketplace vendor is connected to the host website.
/// 0 = Display-only title, 1 = Member who logs into admin, 2 = Another website selling via virtual credit.
/// </summary>
public enum VendorType : byte
{
    Display = 0,
    Member = 1,
    Site = 2,
}
