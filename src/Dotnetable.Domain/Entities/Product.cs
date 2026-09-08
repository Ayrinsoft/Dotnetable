using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class Product
{
    public int ProductID { get; set; }

    public int WebsiteID { get; set; }

    public int? BrandID { get; set; }

    public string Slug { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? ShortDescription { get; set; }

    public string? Content { get; set; }

    public string? ExpertReview { get; set; }

    public int? FeaturedImageFileID { get; set; }

    /// <summary><see cref="Enums.ProductType"/> — physical vs digital fulfillment.</summary>
    public byte ProductType { get; set; }

    /// <summary>
    /// When true, checkout requires a shipping method and address.
    /// Digital products typically set this false.
    /// </summary>
    public bool RequiresShipping { get; set; } = true;

    /// <summary>
    /// External download link for <see cref="Enums.ProductType.DigitalDownload"/>.
    /// Only the URL is stored — the binary is never hosted or snapshotted in this system.
    /// </summary>
    public string? DigitalDownloadUrl { get; set; }

    /// <summary>Service / access URL path for <see cref="Enums.ProductType.DigitalService"/>.</summary>
    public string? DigitalServiceUrl { get; set; }

    /// <summary>
    /// Static delivery text for digital code products (license key template, instructions),
    /// or notes shown after purchase for download/service kinds.
    /// </summary>
    public string? DigitalDeliveryNote { get; set; }

    public bool IsCatalogOnly { get; set; }

    public bool HasVariants { get; set; }

    public decimal AvgRating { get; set; }

    public int RatingCount { get; set; }

    public byte Status { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedByMemberID { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Brand? Brand { get; set; }

    public virtual Member? CreatedByMember { get; set; }

    public virtual FileRecord? FeaturedImageFile { get; set; }

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public virtual ICollection<OrderDigitalAsset> OrderDigitalAssets { get; set; } = new List<OrderDigitalAsset>();

    public virtual ICollection<ProductAttributeValue> ProductAttributeValues { get; set; } = new List<ProductAttributeValue>();

    public virtual ICollection<MarketplaceChannelProduct> MarketplaceChannelProducts { get; set; } = new List<MarketplaceChannelProduct>();

    public virtual ICollection<ProductCategoryMap> ProductCategoryMaps { get; set; } = new List<ProductCategoryMap>();

    public virtual ICollection<ProductCategoryRelation> ProductCategoryRelations { get; set; } = new List<ProductCategoryRelation>();

    public virtual ICollection<ProductMedium> ProductMedia { get; set; } = new List<ProductMedium>();

    public virtual ICollection<ProductQuestion> ProductQuestions { get; set; } = new List<ProductQuestion>();

    public virtual ICollection<ProductRelation> ProductRelationProducts { get; set; } = new List<ProductRelation>();

    public virtual ICollection<ProductRelation> ProductRelationRelatedProducts { get; set; } = new List<ProductRelation>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual ICollection<ProductTranslation> ProductTranslations { get; set; } = new List<ProductTranslation>();

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    public virtual ICollection<ProductWarning> ProductWarnings { get; set; } = new List<ProductWarning>();

    public virtual ICollection<ProductWarranty> ProductWarranties { get; set; } = new List<ProductWarranty>();

    public virtual Website Website { get; set; } = null!;
}
