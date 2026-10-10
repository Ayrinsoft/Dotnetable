namespace Dotnetable.Domain.Enums;

/// <summary>What a visitor is reporting, from a broken page to content that should not be public.</summary>
public enum SiteReportKind : byte
{
    Bug = 1,
    BrokenPage = 2,
    WrongContent = 3,
    InappropriateContent = 4,
    Other = 5,
}

/// <summary>Review state of a <see cref="Entities.SiteReport"/> in the admin panel.</summary>
public enum SiteReportStatus : byte
{
    New = 0,
    InReview = 1,
    Resolved = 2,
    Dismissed = 3,
}
