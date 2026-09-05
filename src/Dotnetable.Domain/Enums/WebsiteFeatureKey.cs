namespace Dotnetable.Domain.Enums;

/// <summary>
/// Togglable module/option that can be enabled or disabled per website
/// (<see cref="Entities.WebsiteFeature"/>). <see cref="WebsiteTypeExtensions.GetDefaultFeatures"/>
/// seeds a starting set from the site's <see cref="WebsiteType"/> at creation time, but every
/// flag stays editable afterwards from the website settings screen.
/// </summary>
public enum WebsiteFeatureKey : byte
{
    ContactForm = 1,
    Newsletter = 2,
    Blog = 3,
    News = 4,
    Gallery = 5,
    Portfolio = 6,
    Resume = 7,
    Ecommerce = 8,
    DigitalCatalog = 9,
    Auction = 10,
    Crowdfunding = 11,
    RealEstate = 12,
    RestaurantMenu = 13,
    Lms = 14,
    Membership = 15,
    /// <summary>Reservation/appointment engine shared by <see cref="WebsiteType.Booking"/>,
    /// <see cref="WebsiteType.MedicalBooking"/> and <see cref="WebsiteType.BeautyBooking"/>.</summary>
    Booking = 16,
    /// <summary>FX-linked published price lists (steel, profiles, sheets, …).</summary>
    PriceLists = 17,
}
