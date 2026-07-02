namespace Dotnetable.Domain.Enums;

/// <summary>
/// Loyalty / value tier of a website customer (<see cref="Entities.WebsiteClient"/>).
/// Stored as a TINYINT; new self-registered customers start at <see cref="Normal"/>.
/// Higher tiers are assigned by administrators to mark more valuable customers.
/// </summary>
public enum ClientLevel : byte
{
    Normal = 0,
    Bronze = 1,
    Silver = 2,
    Gold = 3,
    VIP = 4,
}
