namespace Dotnetable.Domain.Enums;

/// <summary>
/// Specific kind of website (<see cref="Entities.Website.WebsiteType"/>, stored as a TINYINT).
/// Chosen once at creation to pick sensible default <see cref="WebsiteFeatureKey"/> modules
/// (see <see cref="WebsiteTypeExtensions.GetDefaultFeatures"/>) — always editable afterwards.
/// <see cref="Corporate"/> (0) and <see cref="ECommerce"/> (1) keep their original numeric
/// values from the old "Website"/"Store" toggle so existing rows need no data migration.
/// </summary>
public enum WebsiteType : byte
{
    // ── Informational / Brochure ──────────────────────────────────
    /// <summary>Regular corporate/company website.</summary>
    Corporate = 0,
    Personal = 2,
    LandingPage = 3,
    Resume = 4,

    // ── Commerce ───────────────────────────────────────────────────
    /// <summary>Online store with cart, checkout and order management (formerly "Store").</summary>
    ECommerce = 1,
    /// <summary>Product listing with prices but no online checkout.</summary>
    DigitalCatalog = 5,
    Auction = 6,
    Crowdfunding = 7,
    RealEstate = 8,
    Restaurant = 9,

    // ── Content ────────────────────────────────────────────────────
    Gallery = 10,
    Portfolio = 11,
    Blog = 12,
    News = 13,

    // ── Educational / Professional ─────────────────────────────────
    ELearning = 14,
    Membership = 15,
    /// <summary>General purpose reservation site (hotel, ticket, table, ...).</summary>
    Booking = 16,
    /// <summary>Medical appointment booking. Shares the booking engine with <see cref="Booking"/>
    /// and <see cref="BeautyBooking"/> — only labels/terminology differ (doctor, appointment, ...).</summary>
    MedicalBooking = 17,
    /// <summary>Salon/beauty appointment booking. Shares the booking engine with <see cref="Booking"/>
    /// and <see cref="MedicalBooking"/> — only labels/terminology differ (stylist, session, ...).</summary>
    BeautyBooking = 18,
}
