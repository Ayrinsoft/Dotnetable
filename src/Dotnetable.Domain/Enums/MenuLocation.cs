namespace Dotnetable.Domain.Enums;

/// <summary>
/// Where a navigation <see cref="Entities.Menu"/> is rendered on the public website.
/// Stored as a TINYINT in <see cref="Entities.Menu.Location"/>.
/// </summary>
public enum MenuLocation : byte
{
    /// <summary>Primary navigation bar at the top of the page.</summary>
    Header = 1,

    /// <summary>Footer links area.</summary>
    Footer = 2,

    /// <summary>Sidebar / secondary navigation.</summary>
    Sidebar = 3,

    /// <summary>Collapsed navigation used on small screens.</summary>
    Mobile = 4,
}
