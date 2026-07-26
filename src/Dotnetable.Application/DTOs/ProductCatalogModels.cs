namespace Dotnetable.Application.DTOs;

// ── Public read projections (consumed by the website through the API) ──────────

/// <summary>A product category node (may carry nested children) localized to the requested language.</summary>
public sealed class ProductCategoryDto
{
    public int ProductCategoryID { get; init; }
    public int? ParentCategoryID { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public int SortOrder { get; init; }
    public IReadOnlyList<ProductCategoryDto> Children { get; init; } = Array.Empty<ProductCategoryDto>();
}

/// <summary>A brand projected for public/read use.</summary>
public sealed class BrandDto
{
    public int BrandID { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
}

/// <summary>A vendor/marketplace seller projected for public/read use.</summary>
public sealed class VendorDto
{
    public int VendorID { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public decimal Rating { get; init; }
}

/// <summary>One resolved attribute value on a product or variant, localized.</summary>
public sealed class ProductAttributeValueDto
{
    public int AttributeDefinitionID { get; init; }
    public string AttributeName { get; init; } = string.Empty;
    public string? Unit { get; init; }
    public int? AttributeOptionID { get; init; }
    public string? OptionValue { get; init; }
    public string? ColorHex { get; init; }
    public string? CustomValue { get; init; }
    public decimal? NumericValue { get; init; }
    public bool IsFeatured { get; init; }
}

/// <summary>A single purchasable variant of a product, localized/priced for display.</summary>
public sealed class ProductVariantDto
{
    public int ProductVariantID { get; init; }
    public string Sku { get; init; } = string.Empty;
    /// <summary>Human-readable variant label shown in pickers and cart (e.g. "Black · XL").</summary>
    public string Title { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public string? ImageUrl { get; init; }
    public MoneyDto Price { get; init; } = new();
    public MoneyDto? CompareAtPrice { get; init; }
    public decimal? Weight { get; init; }
    public string? Barcode { get; init; }
    public bool IsActive { get; init; }
    /// <summary>Available units (for max qty / cart). Prefer <see cref="DisplayStockQuantity"/> for UI labels.</summary>
    public int StockQuantity { get; init; }
    /// <summary>True when <see cref="StockQuantity"/> &gt; 0.</summary>
    public bool IsInStock { get; init; }
    /// <summary>Exact count for UI only when 1..8; null means out of stock or “in stock” without a number (≥9).</summary>
    public int? DisplayStockQuantity { get; init; }
    public int? VendorProductID { get; init; }
    public int? VendorID { get; init; }
    public IReadOnlyList<ProductAttributeValueDto> Attributes { get; init; } = Array.Empty<ProductAttributeValueDto>();
}

/// <summary>One marketplace seller offering a product variant (only in-stock sellers are returned).</summary>
public sealed class ProductSellerOfferDto
{
    public int VendorID { get; init; }
    public string VendorName { get; init; } = string.Empty;
    public string VendorSlug { get; init; } = string.Empty;
    public byte VendorType { get; init; }
    public int VendorProductID { get; init; }
    public int ProductVariantID { get; init; }
    public string VariantTitle { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public MoneyDto Price { get; init; } = new();
    public int StockQuantity { get; init; }
    public bool IsInStock { get; init; }
    public int? DisplayStockQuantity { get; init; }
    public int DeliveryDays { get; init; }
}

/// <summary>A safety/compliance warning shown on the product detail page.</summary>
public sealed class ProductWarningDto
{
    public int ProductWarningID { get; init; }
    public string Severity { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
}

/// <summary>Resolved warranty line for a product (catalog definition and/or custom text).</summary>
public sealed class ProductWarrantyDto
{
    public int ProductWarrantyID { get; init; }
    public int? WarrantyID { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ProviderName { get; init; }
    public int? DurationMonths { get; init; }
}

/// <summary>One recorded price point for a product variant (admin history / charts).</summary>
public sealed class ProductVariantPriceHistoryDto
{
    public long ProductVariantPriceHistoryID { get; init; }
    public int ProductVariantID { get; init; }
    public decimal ReferencePrice { get; init; }
    public decimal? CompareAtPrice { get; init; }
    public decimal ReferencePriceUsd { get; init; }
    public decimal? CompareAtPriceUsd { get; init; }
    public DateTime RecordedAt { get; init; }
    public int? ChangedByMemberId { get; init; }
}

/// <summary>Admin/list projection of a reusable warranty definition.</summary>
public sealed class WarrantyListItemDto
{
    public int WarrantyID { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? ProviderName { get; init; }
    public int? DurationMonths { get; init; }
    public bool IsActive { get; set; }
    public int SortOrder { get; init; }
}

/// <summary>A lightweight reference to another product (used for related/cross-sell lists).</summary>
public sealed class ProductRefDto
{
    public int ProductID { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public MoneyDto? MinPrice { get; init; }
    public byte RelationType { get; init; }
}

/// <summary>A product in a list (card) context — localized, priced in the requested display currency.</summary>
public class ProductSummaryDto
{
    public int ProductID { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? ShortDescription { get; init; }
    public string? FeaturedImageUrl { get; init; }
    public string? BrandName { get; init; }
    public string? DefaultSku { get; init; }
    public MoneyDto MinPrice { get; init; } = new();
    public decimal AvgRating { get; init; }
    public int RatingCount { get; init; }
    public bool HasVariants { get; init; }

    /// <summary>True when any sellable channel (warehouse or seller listing) has stock &gt; 0.</summary>
    public bool IsInStock { get; init; }
    /// <summary>Best available units across channels (for cart max). Prefer <see cref="DisplayStockQuantity"/> for labels.</summary>
    public int StockQuantity { get; init; }
    /// <summary>Exact count for UI only when 1..8; null means out of stock or “in stock” without a number (≥9).</summary>
    public int? DisplayStockQuantity { get; init; }

    /// <summary>When the listing comes from a site-linked or member vendor on the host storefront.</summary>
    public int? VendorID { get; init; }
    public string? VendorName { get; init; }
    public int? VendorProductID { get; init; }
}

/// <summary>A single published product with full detail (variants, attributes, content, warnings, related).</summary>
public sealed class ProductDetailDto : ProductSummaryDto
{
    public IReadOnlyList<ProductCategoryDto> Categories { get; init; } = Array.Empty<ProductCategoryDto>();
    public IReadOnlyList<ProductVariantDto> Variants { get; init; } = Array.Empty<ProductVariantDto>();
    /// <summary>In-stock marketplace sellers for this product (out-of-stock sellers are omitted).</summary>
    public IReadOnlyList<ProductSellerOfferDto> Sellers { get; init; } = Array.Empty<ProductSellerOfferDto>();
    public IReadOnlyList<ProductAttributeValueDto> Attributes { get; init; } = Array.Empty<ProductAttributeValueDto>();
    /// <summary>Rich HTML description (CKEditor), with optional inline images.</summary>
    public string? Content { get; init; }
    /// <summary>Rich HTML expert/technical review (CKEditor), same shape as <see cref="Content"/>.</summary>
    public string? ExpertReview { get; init; }
    public IReadOnlyList<ProductWarningDto> Warnings { get; init; } = Array.Empty<ProductWarningDto>();
    public IReadOnlyList<ProductWarrantyDto> Warranties { get; init; } = Array.Empty<ProductWarrantyDto>();
    public IReadOnlyList<ProductRefDto> RelatedProducts { get; init; } = Array.Empty<ProductRefDto>();
    public IReadOnlyList<string> GalleryImageUrls { get; init; } = Array.Empty<string>();
}

// ── Admin projections ───────────────────────────────────────────────────────────

/// <summary>A lightweight row for the admin products grid.</summary>
public sealed class ProductListItemDto
{
    public int ProductID { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? BrandName { get; init; }
    public byte Status { get; init; }
    public bool IsActive { get; init; }
    public bool HasVariants { get; init; }
    /// <summary>Minimum catalog price in site operational currency.</summary>
    public decimal? MinPrice { get; init; }
    /// <summary>Minimum catalog price in USD (dual / bridge).</summary>
    public decimal? MinPriceUsd { get; init; }
    public string? FeaturedImageUrl { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>Optional filters applied to the admin product listing.</summary>
public sealed class ProductFilter
{
    public int? BrandID { get; set; }
    public int? ProductCategoryID { get; set; }
    public byte? Status { get; set; }
    public bool? IsActive { get; set; }
    /// <summary>When set, only products created by this member (vendor scope).</summary>
    public int? CreatedByMemberId { get; set; }
}
