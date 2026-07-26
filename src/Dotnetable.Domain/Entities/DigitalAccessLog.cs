using System;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// Audit log of each time a customer opens/downloads a digital entitlement.
/// </summary>
public partial class DigitalAccessLog
{
    public long DigitalAccessLogID { get; set; }

    public int OrderDigitalAssetID { get; set; }

    public int WebsiteClientID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary><see cref="Enums.DigitalAccessType"/>.</summary>
    public byte AccessType { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime AccessedAt { get; set; }

    public virtual OrderDigitalAsset OrderDigitalAsset { get; set; } = null!;

    public virtual WebsiteClient WebsiteClient { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;
}
