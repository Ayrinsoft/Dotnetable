namespace Dotnetable.Application.DTOs;

/// <summary>Public identity/branding of a website, consumed by the Web/React front-ends so the
/// layout (brand, logo, contact info, social links, SEO defaults) is admin-managed, not hardcoded.</summary>
public sealed class SiteInfoDto
{
    public string BrandName { get; init; } = string.Empty;
    public string TradeName { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string? FavIconUrl { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string DefaultLanguageCode { get; init; } = "en";
    public string? DefaultMetaTitle { get; init; }
    public string? DefaultMetaDescription { get; init; }
    public string TitleSeparator { get; init; } = "·";
    public List<SocialLinkDto> SocialLinks { get; init; } = new();
    public List<ContactInfoDto> ContactInfos { get; init; } = new();
}

public sealed class SocialLinkDto
{
    public string? Name { get; init; }
    /// <summary>Icon css class (e.g. "bi bi-instagram") as configured in the admin.</summary>
    public string? Icon { get; init; }
    public string Url { get; init; } = string.Empty;
}

/// <summary>An admin-defined contact detail (phone, email, address, working hours, ...), grouped by
/// an admin-chosen category (e.g. "Sales Office", "Factory") for display under Contact Us.</summary>
public sealed class ContactInfoDto
{
    public string? GroupTitle { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    /// <summary>Icon css class (e.g. "bi bi-geo-alt") as configured in the admin.</summary>
    public string? Icon { get; init; }
}
