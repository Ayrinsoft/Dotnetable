using System;

namespace Dotnetable.Domain.Entities;

/// <summary>A row means this product has been sent to Instagram at least once. Sending again is allowed.</summary>
public partial class ProductInstagramPublication
{
    public int ProductID { get; set; }

    public string IgMediaId { get; set; } = null!;

    public string? Permalink { get; set; }

    public DateTime PublishedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
