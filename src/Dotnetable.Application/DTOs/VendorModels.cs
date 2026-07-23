namespace Dotnetable.Application.DTOs;

public sealed class VendorProductListItemDto
{
    public int VendorProductID { get; init; }
    public int VendorID { get; init; }
    public int ProductVariantID { get; init; }
    public int ProductID { get; init; }
    public string ProductTitle { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public decimal ReferencePriceUsd { get; init; }
    public decimal? OverridePrice { get; init; }
    public int StockQuantity { get; init; }
    public int DeliveryDays { get; init; }
    public bool IsActive { get; init; }
}

public sealed class VendorCreditBalanceDto
{
    public int VendorID { get; init; }
    public decimal AvailableCreditUsd { get; init; }
    public decimal? CreditLimitUsd { get; init; }
    public byte SettlementMode { get; init; }
    public byte VendorType { get; init; }
    public int? LinkedWebsiteID { get; init; }
    public bool CanExposeCatalog { get; init; }
}
