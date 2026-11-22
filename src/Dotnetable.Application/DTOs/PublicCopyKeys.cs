namespace Dotnetable.Application.DTOs;

/// <summary>
/// Storefront sentences a site admin edits on Website → Site profile.
/// The travel site reads the same keys from <c>api/localization/fa</c>.
/// </summary>
public static class PublicCopyKeys
{
    public const string Language = "fa";

    public const string FooterBlurb = "footer.blurb";
    public const string FooterLegal = "footer.admin";
    public const string ContactKicker = "contact.kicker";
    public const string ContactTitle = "contact.title";
    public const string ContactLead = "contact.lead";
    public const string HomeKicker = "home.kicker";
    public const string HomeTitle = "home.title";
    public const string HomeLead = "home.lead";
}

/// <summary>Fields a site admin may change on their own website. Domain, API key and active flag stay with the platform admin.</summary>
public sealed class WebsitePublicProfile
{
    public int WebsiteId { get; set; }
    public string TradeName { get; set; } = "";
    public string BrandName { get; set; } = "";
    public string Manager { get; set; } = "";
    public string Mobile { get; set; } = "";
    public string Email { get; set; } = "";
    public int? LogoFileId { get; set; }
    public int? FavIconFileId { get; set; }
    public int? BackgroundImageFileId { get; set; }
    public int? DarkBackgroundImageFileId { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? BackgroundColor { get; set; }
    public string? DarkBackgroundColor { get; set; }
    public bool TaxEnabled { get; set; }
    public bool PricesIncludeTax { get; set; }
    public bool TaxOnShipping { get; set; }
    public bool ReportOfflineOrdersToTax { get; set; }
    public int? TaxCountryId { get; set; }
    public string? SellerLegalName { get; set; }
    public string? SellerTaxId { get; set; }
    public string? SellerEconomicCode { get; set; }
    public string? SellerVatNumber { get; set; }
    public string? SellerRegistrationNumber { get; set; }
}
