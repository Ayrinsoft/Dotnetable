using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

public partial class ProductVariant
{
    public int ProductVariantID { get; set; }

    public int WebsiteID { get; set; }

    public int ProductID { get; set; }

    public string Sku { get; set; } = null!;

    public string Title { get; set; } = null!;

    public bool IsDefault { get; set; }

    public int? ImageFileID { get; set; }

    public decimal ReferencePriceUsd { get; set; }

    public decimal? CompareAtPriceUsd { get; set; }

    public decimal? OverridePrice { get; set; }

    public decimal? Weight { get; set; }

    public string? Barcode { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual FileRecord? ImageFile { get; set; }

    public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual ICollection<ProductVariantPriceHistory> ProductVariantPriceHistories { get; set; } = new List<ProductVariantPriceHistory>();

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public virtual ICollection<VariantAttributeValue> VariantAttributeValues { get; set; } = new List<VariantAttributeValue>();

    public virtual ICollection<VendorProduct> VendorProducts { get; set; } = new List<VendorProduct>();

    public virtual Website Website { get; set; } = null!;

    public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
}
