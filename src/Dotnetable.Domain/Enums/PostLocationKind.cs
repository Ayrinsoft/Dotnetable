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
}
