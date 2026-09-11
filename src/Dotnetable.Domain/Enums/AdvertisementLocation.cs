namespace Dotnetable.Domain.Enums;

/// <summary>
/// Where a text advertisement is rendered on the public website.
/// Stored as a TINYINT in <see cref="Entities.Advertisement.Location"/>.
/// </summary>
public enum AdvertisementLocation : byte
{
    /// <summary>Strip below the site header / navigation bar.</summary>
    Header = 1,

    /// <summary>Footer links / sponsored strip.</summary>
    Footer = 2,

    /// <summary>Shop (and similar) left-hand sidebar.</summary>
    Sidebar = 3,

    /// <summary>Homepage, typically under the hero / slideshow.</summary>
    Home = 4,

    /// <summary>Product detail page.</summary>
    Product = 5,

    /// <summary>Blog listing and post pages.</summary>
    Blog = 6,

    /// <summary>CMS pages.</summary>
    Page = 7,
}
