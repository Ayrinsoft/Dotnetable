namespace Dotnetable.Domain.Enums;

/// <summary>
/// Kind of a <see cref="Entities.WebsiteContactInfo"/> row. A fixed, closed list so the admin picks
/// from a dropdown instead of free-typing a category — each value drives a default icon and label
/// in the admin picker, which is what makes a list of otherwise-identical title/value rows readable.
/// Stored as a TINYINT in <see cref="Entities.WebsiteContactInfo.ContactType"/>.
/// </summary>
public enum ContactInfoType : byte
{
    /// <summary>Anything that doesn't fit the other kinds — the admin's own title/icon carry it.</summary>
    Other = 0,

    /// <summary>A phone number.</summary>
    Phone = 1,

    /// <summary>An email address.</summary>
    Email = 2,

    /// <summary>A postal/street address.</summary>
    Address = 3,

    /// <summary>A map link / location (e.g. a Google Maps URL) distinct from a plain postal address.</summary>
    Location = 4,

    /// <summary>Working hours / business hours text.</summary>
    WorkingHours = 5,
}
