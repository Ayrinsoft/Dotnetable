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

    public int? FeaturedImageFileID { get; set; }

    public bool IsCatalogOnly { get; set; }

    public bool HasVariants { get; set; }

    public decimal AvgRating { get; set; }

    public int RatingCount { get; set; }

    public byte Status { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedByMemberId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Brand? Brand { get; set; }

    public virtual Member? CreatedByMember { get; set; }

    public virtual FileRecord? FeaturedImageFile { get; set; }

    public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public virtual ICollection<ProductAttributeValue> ProductAttributeValues { get; set; } = new List<ProductAttributeValue>();

    public virtual ICollection<ProductCategoryMap> ProductCategoryMaps { get; set; } = new List<ProductCategoryMap>();

    public virtual ICollection<ProductCategoryRelation> ProductCategoryRelations { get; set; } = new List<ProductCategoryRelation>();

    public virtual ICollection<ProductContentSection> ProductContentSections { get; set; } = new List<ProductContentSection>();

    public virtual ICollection<ProductMedium> ProductMedia { get; set; } = new List<ProductMedium>();

    public virtual ICollection<ProductQuestion> ProductQuestions { get; set; } = new List<ProductQuestion>();

    public virtual ICollection<ProductRelation> ProductRelationProducts { get; set; } = new List<ProductRelation>();

    public virtual ICollection<ProductRelation> ProductRelationRelatedProducts { get; set; } = new List<ProductRelation>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual ICollection<ProductTranslation> ProductTranslations { get; set; } = new List<ProductTranslation>();

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    public virtual ICollection<ProductWarning> ProductWarnings { get; set; } = new List<ProductWarning>();

    public virtual Website Website { get; set; } = null!;
}
