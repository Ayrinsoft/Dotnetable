using System;

namespace Dotnetable.Domain.Entities;

/// <summary>A row means this product has been sent to Telegram at least once. Sending again is allowed.</summary>
public partial class ProductTelegramPublication
{
    public int ProductID { get; set; }

    public string? MessageId { get; set; }

    public DateTime PublishedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
