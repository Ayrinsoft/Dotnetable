using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ProductReview
{
    public int ProductReviewID { get; set; }

    public int WebsiteID { get; set; }

    public int ProductID { get; set; }

    public int? ProductVariantID { get; set; }

    public int WebsiteClientID { get; set; }

    public byte Rating { get; set; }

    public string? Title { get; set; }

    public string Body { get; set; } = null!;

    public string? ProsJson { get; set; }

    public string? ConsJson { get; set; }

    public bool IsVerifiedPurchase { get; set; }

    public byte Status { get; set; } = (byte)1;

    public int LikeCount { get; set; }

    public int DislikeCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool Approved { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ProductVariant? ProductVariant { get; set; }

    public virtual Website Website { get; set; } = null!;

    public virtual WebsiteClient WebsiteClient { get; set; } = null!;
}
