namespace Dotnetable.Application.DTOs;

/// <summary>A single cart line, priced live in the requested display currency.</summary>
public sealed class CartItemViewDto
{
    public int CartItemID { get; init; }
    public int ProductVariantID { get; init; }
    public int ProductID { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public int Quantity { get; init; }
    public MoneyDto UnitPrice { get; init; } = new();
    public MoneyDto LineTotal { get; init; } = new();
    public bool IsAvailable { get; init; }
    public int MaxPurchasable { get; init; }
    /// <summary>Store listing id for this line (required for sellable stock).</summary>
    public int? VendorProductID { get; init; }
    public int? VendorID { get; init; }
    /// <summary>Seller display name when the line is a marketplace listing.</summary>
    public string? VendorName { get; init; }
}

/// <summary>The full cart, priced live, with coupon/shipping/tax preview totals in USD.</summary>
public sealed class CartViewDto
{
    public int CartID { get; init; }
    public IReadOnlyList<CartItemViewDto> Items { get; init; } = Array.Empty<CartItemViewDto>();
    public MoneyDto SubTotal { get; init; } = new();
    public string? CouponCode { get; init; }
    public MoneyDto? DiscountAmount { get; init; }
    public string? CouponError { get; init; }
    public decimal TotalWeightKg { get; init; }
}
