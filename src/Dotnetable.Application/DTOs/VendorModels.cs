namespace Dotnetable.Application.DTOs;

public sealed class VendorProductListItemDto
{
    public int VendorProductID { get; init; }
    public int VendorID { get; init; }
    public string VendorName { get; init; } = string.Empty;
    /// <summary>0=Display, 1=Member, 2=Site (see <c>VendorType</c>).</summary>
    public byte VendorType { get; init; }
    public int ProductVariantID { get; init; }
    public int ProductID { get; init; }
    public string ProductTitle { get; init; } = string.Empty;
    public string VariantTitle { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    /// <summary>Listing price in site operational currency.</summary>
    public decimal ReferencePrice { get; init; }
    public decimal? OverridePriceLocal { get; init; }
    public decimal ReferencePriceUsd { get; init; }
    public decimal? OverridePrice { get; init; }
    public int StockQuantity { get; init; }
    public int DeliveryDays { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>Pick-list row for adding a vendor listing (searchable product + variant).</summary>
public sealed class VendorVariantPickDto
{
    public int ProductVariantID { get; init; }
    public int ProductID { get; init; }
    public string ProductTitle { get; init; } = string.Empty;
    public string VariantTitle { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public decimal ReferencePrice { get; init; }
    public decimal ReferencePriceUsd { get; init; }
    public bool AlreadyListed { get; init; }

    public string DisplayLabel =>
        string.IsNullOrWhiteSpace(VariantTitle)
            ? $"{ProductTitle}  ·  {Sku}"
            : $"{ProductTitle}  ·  {VariantTitle}  ·  {Sku}";
}

public sealed class VendorCreditBalanceDto
{
    public int VendorID { get; init; }
    public decimal AvailableCredit { get; init; }
    public decimal AvailableCreditUsd { get; init; }
    public decimal? CreditLimit { get; init; }
    public decimal? CreditLimitUsd { get; init; }
    public byte SettlementMode { get; init; }
    public byte VendorType { get; init; }
    public int? LinkedWebsiteID { get; init; }
    public bool CanExposeCatalog { get; init; }
}
