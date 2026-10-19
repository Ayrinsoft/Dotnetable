namespace Dotnetable.Domain.Enums;

/// <summary>
/// What kind of place a post location points at. Stored as <c>tinyint</c>.
/// Add new values at the end. Do not renumber existing ones.
/// </summary>
public enum PostLocationKind : byte
{
    Unspecified = 0,
    Office = 1,
    Store = 2,
    Branch = 3,
    Showroom = 4,
    Warehouse = 5,
    Venue = 6,
    Other = 7,
    /// <summary>Historic or cultural place (تاریخی و فرهنگی).</summary>
    HistoricSite = 8,
    /// <summary>Leisure or recreation spot (تفریحی).</summary>
    Recreation = 9,
    /// <summary>Rural stay / ecolodge (اقامتگاه بوم‌گردی).</summary>
    Ecolodge = 10,
    /// <summary>Backpacking or trekking route (کوله‌گردی).</summary>
    Backpacking = 11,
    /// <summary>Restaurant (رستوران).</summary>
    Restaurant = 12,
    /// <summary>Shrine or other religious place (زیارتی و مذهبی).</summary>
    Pilgrimage = 13,
    /// <summary>Nature destination (طبیعت‌گردی).</summary>
    Nature = 14,
    /// <summary>Camp or outdoor adventure (کمپ و ماجراجویی).</summary>
    CampAdventure = 15,
    /// <summary>Traditional bazaar (بازار سنتی).</summary>
    Bazaar = 16,
}
