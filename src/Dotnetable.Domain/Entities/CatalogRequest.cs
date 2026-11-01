namespace Dotnetable.Domain.Entities;

/// <summary>
/// A request from a website that is not allowed to change the shared product catalog.
/// Website 1's admins are notified by email and in the admin inbox.
/// </summary>
public class CatalogRequest
{
    public int CatalogRequestID { get; set; }

    public int WebsiteID { get; set; }

    public int RequestedByMemberID { get; set; }

    /// <summary><see cref="Enums.CatalogRequestKind"/>.</summary>
    public byte Kind { get; set; }

    /// <summary><see cref="Enums.CatalogRequestStatus"/>.</summary>
    public byte Status { get; set; }

    public string? ProposedCategoryName { get; set; }

    public int? ParentCategoryID { get; set; }

    public string ProductTitle { get; set; } = null!;

    public string? Details { get; set; }

    public string? AdminNote { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public int? ReviewedByMemberID { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual Member RequestedByMember { get; set; } = null!;

    public virtual Member? ReviewedByMember { get; set; }

    public virtual ProductCategory? ParentCategory { get; set; }
}
