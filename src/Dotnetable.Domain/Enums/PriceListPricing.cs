namespace Dotnetable.Domain.Enums;

/// <summary>
/// How custom price-list rows are priced. Catalog-sourced lists always use the product's
/// stored catalog price — this flag is ignored for <see cref="PriceListSource.CatalogProducts"/>.
/// USD follow is opt-in, never the default.
/// </summary>
public enum PriceListPricing : byte
{
    /// <summary>Prices are in the website operational currency and do not move with FX.</summary>
    SiteCurrency = 0,

    /// <summary>Row base prices are USD; display = USD × current website rate.</summary>
    LinkedToUsd = 1,
}
