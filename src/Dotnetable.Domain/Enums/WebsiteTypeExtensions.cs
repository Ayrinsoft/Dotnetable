namespace Dotnetable.Domain.Enums;

public static class WebsiteTypeExtensions
{
    /// <summary>Which <see cref="WebsiteCategory"/> group a <see cref="WebsiteType"/> belongs to.</summary>
    public static WebsiteCategory GetCategory(this WebsiteType type) => type switch
    {
        WebsiteType.Corporate or WebsiteType.Personal or WebsiteType.LandingPage or WebsiteType.Resume
            => WebsiteCategory.Informational,

        WebsiteType.ECommerce or WebsiteType.DigitalCatalog or WebsiteType.Auction
            or WebsiteType.Crowdfunding or WebsiteType.RealEstate or WebsiteType.Restaurant
            => WebsiteCategory.Commerce,

        WebsiteType.Gallery or WebsiteType.Portfolio or WebsiteType.Blog or WebsiteType.News
            => WebsiteCategory.Content,

        WebsiteType.ELearning or WebsiteType.Membership or WebsiteType.Booking
            or WebsiteType.MedicalBooking or WebsiteType.BeautyBooking
            => WebsiteCategory.Educational,

        _ => WebsiteCategory.Informational,
    };

    public static WebsiteCategory GetCategory(this byte type) => ((WebsiteType)type).GetCategory();

    /// <summary>Localization key (under "website.type.*") and English fallback label for display, e.g. in type pickers and list chips.</summary>
    public static (string Key, string Fallback) GetDisplayText(this WebsiteType type) => type switch
    {
        WebsiteType.Corporate => ("website.type.corporate", "Corporate Website"),
        WebsiteType.Personal => ("website.type.personal", "Personal Website"),
        WebsiteType.LandingPage => ("website.type.landing_page", "Landing Page"),
        WebsiteType.Resume => ("website.type.resume", "Resume / CV Site"),

        WebsiteType.ECommerce => ("website.type.ecommerce", "Online Store (E-commerce)"),
        WebsiteType.DigitalCatalog => ("website.type.digital_catalog", "Digital Catalog"),
        WebsiteType.Auction => ("website.type.auction", "Auction Site"),
        WebsiteType.Crowdfunding => ("website.type.crowdfunding", "Crowdfunding"),
        WebsiteType.RealEstate => ("website.type.real_estate", "Real Estate Listing"),
        WebsiteType.Restaurant => ("website.type.restaurant", "Restaurant / Online Menu"),

        WebsiteType.Gallery => ("website.type.gallery", "Photo & Video Gallery"),
        WebsiteType.Portfolio => ("website.type.portfolio", "Portfolio"),
        WebsiteType.Blog => ("website.type.blog", "Blog"),
        WebsiteType.News => ("website.type.news", "News / Magazine"),

        WebsiteType.ELearning => ("website.type.elearning", "E-learning / Online Courses"),
        WebsiteType.Membership => ("website.type.membership", "Membership / Subscription Site"),
        WebsiteType.Booking => ("website.type.booking", "Booking / Reservation"),
        WebsiteType.MedicalBooking => ("website.type.medical_booking", "Medical Appointment Booking"),
        WebsiteType.BeautyBooking => ("website.type.beauty_booking", "Beauty / Salon Appointment Booking"),

        _ => ("website.type.corporate", "Corporate Website"),
    };

    public static (string Key, string Fallback) GetDisplayText(this byte type) => ((WebsiteType)type).GetDisplayText();

    /// <summary>
    /// Default set of <see cref="WebsiteFeatureKey"/> modules a new website of this type should
    /// start with. Applied once at creation (see <c>WebsiteService.CreateAsync</c>) — every
    /// feature stays editable afterwards, this is only the initial suggestion.
    /// </summary>
    public static IReadOnlyList<WebsiteFeatureKey> GetDefaultFeatures(this WebsiteType type) => type switch
    {
        WebsiteType.Corporate => [WebsiteFeatureKey.ContactForm],
        WebsiteType.Personal => [WebsiteFeatureKey.ContactForm, WebsiteFeatureKey.Blog],
        WebsiteType.LandingPage => [WebsiteFeatureKey.ContactForm],
        WebsiteType.Resume => [WebsiteFeatureKey.ContactForm, WebsiteFeatureKey.Resume],

        WebsiteType.ECommerce => [WebsiteFeatureKey.Ecommerce, WebsiteFeatureKey.ContactForm, WebsiteFeatureKey.Newsletter],
        WebsiteType.DigitalCatalog => [WebsiteFeatureKey.DigitalCatalog, WebsiteFeatureKey.ContactForm],
        WebsiteType.Auction => [WebsiteFeatureKey.Auction, WebsiteFeatureKey.Ecommerce, WebsiteFeatureKey.ContactForm],
        WebsiteType.Crowdfunding => [WebsiteFeatureKey.Crowdfunding, WebsiteFeatureKey.ContactForm],
        WebsiteType.RealEstate => [WebsiteFeatureKey.RealEstate, WebsiteFeatureKey.ContactForm],
        WebsiteType.Restaurant => [WebsiteFeatureKey.RestaurantMenu, WebsiteFeatureKey.ContactForm],

        WebsiteType.Gallery => [WebsiteFeatureKey.Gallery, WebsiteFeatureKey.ContactForm],
        WebsiteType.Portfolio => [WebsiteFeatureKey.Portfolio, WebsiteFeatureKey.ContactForm],
        WebsiteType.Blog => [WebsiteFeatureKey.Blog, WebsiteFeatureKey.Newsletter, WebsiteFeatureKey.ContactForm],
        WebsiteType.News => [WebsiteFeatureKey.News, WebsiteFeatureKey.Newsletter, WebsiteFeatureKey.ContactForm],

        WebsiteType.ELearning => [WebsiteFeatureKey.Lms, WebsiteFeatureKey.Membership, WebsiteFeatureKey.ContactForm],
        WebsiteType.Membership => [WebsiteFeatureKey.Membership, WebsiteFeatureKey.ContactForm],
        WebsiteType.Booking => [WebsiteFeatureKey.Booking, WebsiteFeatureKey.ContactForm],
        WebsiteType.MedicalBooking => [WebsiteFeatureKey.Booking, WebsiteFeatureKey.ContactForm],
        WebsiteType.BeautyBooking => [WebsiteFeatureKey.Booking, WebsiteFeatureKey.ContactForm],

        _ => [WebsiteFeatureKey.ContactForm],
    };
}
