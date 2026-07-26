using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// Permanent digital entitlement for a paid order line.
/// Stores link/code snapshots only — never a file binary or FileRecord reference.
/// </summary>
public partial class OrderDigitalAsset
{
    public int OrderDigitalAssetID { get; set; }

    public int WebsiteID { get; set; }

    public int WebsiteClientID { get; set; }

    public int OrderID { get; set; }

    public int OrderItemID { get; set; }

    public int ProductID { get; set; }

    /// <summary><see cref="Enums.ProductType"/> at grant time.</summary>
    public byte ProductType { get; set; }

    public string TitleSnapshot { get; set; } = null!;

    /// <summary>Download URL snapshotted at grant (external link only).</summary>
    public string? DigitalDownloadUrl { get; set; }

    /// <summary>Service / access URL snapshotted at grant time.</summary>
    public string? DigitalServiceUrl { get; set; }

    /// <summary>License code text / delivery instructions snapshotted at grant time.</summary>
    public string? DigitalDeliveryNote { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime GrantedAt { get; set; }

    public virtual ICollection<DigitalAccessLog> DigitalAccessLogs { get; set; } = new List<DigitalAccessLog>();

    public virtual Order Order { get; set; } = null!;

    public virtual OrderItem OrderItem { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;

    public virtual WebsiteClient WebsiteClient { get; set; } = null!;
}
