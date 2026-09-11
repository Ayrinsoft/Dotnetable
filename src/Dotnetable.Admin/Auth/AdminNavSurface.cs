using Dotnetable.Domain.Enums;

namespace Dotnetable.Admin.Auth;

/// <summary>
/// High-level admin drawer / search areas. Visibility is the intersection of:
/// role permissions (AuthorizeView) × <see cref="AdminUiMode"/> ×
/// the logged-in member's website <see cref="WebsiteType"/> (from Website edit form) × vendor-member scope.
/// </summary>
public enum AdminNavArea
{
    /// <summary>Members, customers, policies, login logs.</summary>
    Users,

    /// <summary>Website settings (SEO, scripts, languages, …).</summary>
    Website,

    /// <summary>Contact messages, email accounts/templates.</summary>
    Messages,

    /// <summary>Pages, menus, slideshows, advertisements, themes.</summary>
    ContentCore,

    /// <summary>Posts, blog categories/tags/post-types.</summary>
    ContentBlog,

    /// <summary>Forms, redirects, review/Q&amp;A moderation.</summary>
    ContentExtra,

    Media,

    /// <summary>Products, catalog taxonomy, vendors.</summary>
    Catalog,

    Inventory,

    Orders,

    Finance,

    /// <summary>Coupons, shipping, tax.</summary>
    Promotions,
}

/// <summary>
/// Resolves which admin navigation areas a member should see.
/// Driven by <see cref="Website.WebsiteType"/> of the website on the login claim (same field as Website edit / list).
/// Master website (site 1) and Advanced mode always get the full surface (still role-gated in UI).
/// Content and Media are always on for site admins (Basic/General/Advanced); website type only
/// trims commerce / extra stacks. Member vendors only get seller-relevant areas.
/// </summary>
public static class AdminNavSurface
{
    private static readonly AdminNavArea[] AllAreas = Enum.GetValues<AdminNavArea>();

    public static IReadOnlySet<AdminNavArea> Resolve(
        AdminUiMode mode,
        WebsiteType? websiteType,
        bool isMaster,
        bool isVendorMember)
    {
        if (isMaster)
            return new HashSet<AdminNavArea>(AllAreas);

        if (isVendorMember)
        {
            return new HashSet<AdminNavArea>
            {
                AdminNavArea.Catalog,
                AdminNavArea.Media,
                AdminNavArea.Orders,
                AdminNavArea.Messages,
            };
        }

        // Advanced: every area the role system allows.
        if (mode == AdminUiMode.Advanced)
            return new HashSet<AdminNavArea>(AllAreas);

        var type = websiteType ?? WebsiteType.Corporate;
        var surface = ForWebsiteType(type);

        // Content + Media submenus stay on in Basic/General (still role-gated).
        // Website type only trims commerce / extra stacks, not CMS.
        surface.UnionWith(ContentAndMedia);

        if (mode == AdminUiMode.General)
            surface.UnionWith(GeneralExtras(type));

        return surface;
    }

    private static readonly AdminNavArea[] ContentAndMedia =
    [
        AdminNavArea.ContentCore,
        AdminNavArea.ContentBlog,
        AdminNavArea.ContentExtra,
        AdminNavArea.Media,
    ];

    /// <summary>
    /// Basic surface for the exact type chosen on the Website form
    /// (aligned with <see cref="WebsiteTypeExtensions.GetDefaultFeatures"/> families).
    /// Content and Media are added on top in <see cref="Resolve"/>.
    /// </summary>
    public static HashSet<AdminNavArea> ForWebsiteType(WebsiteType type) => type switch
    {
        // ── Informational / Brochure ──────────────────────────────
        WebsiteType.Corporate or WebsiteType.LandingPage =>
        [
            AdminNavArea.Users, AdminNavArea.Website, AdminNavArea.Messages,
            AdminNavArea.ContentCore, AdminNavArea.ContentExtra, AdminNavArea.Media,
        ],
        WebsiteType.Personal =>
        [
            AdminNavArea.Users, AdminNavArea.Website, AdminNavArea.Messages,
            AdminNavArea.ContentCore, AdminNavArea.ContentBlog, AdminNavArea.ContentExtra, AdminNavArea.Media,
        ],
        WebsiteType.Resume =>
        [
            AdminNavArea.Website, AdminNavArea.Messages,
            AdminNavArea.ContentCore, AdminNavArea.Media,
        ],

        // ── Commerce ──────────────────────────────────────────────
        WebsiteType.ECommerce or WebsiteType.Auction =>
            FullCommerce(),

        WebsiteType.DigitalCatalog or WebsiteType.RealEstate or WebsiteType.Restaurant =>
        [
            // List/prices without full checkout stack in Basic (unlock via Advanced).
            // Finance is included so the daily USD rate that drives price lists can be updated.
            AdminNavArea.Users, AdminNavArea.Website, AdminNavArea.Messages,
            AdminNavArea.ContentCore, AdminNavArea.Media,
            AdminNavArea.Catalog, AdminNavArea.Inventory, AdminNavArea.Finance,
        ],

        WebsiteType.Crowdfunding =>
        [
            AdminNavArea.Users, AdminNavArea.Website, AdminNavArea.Messages,
            AdminNavArea.ContentCore, AdminNavArea.ContentBlog, AdminNavArea.Media,
            AdminNavArea.Catalog, AdminNavArea.Orders, AdminNavArea.Finance,
        ],

        // ── Content showcase ──────────────────────────────────────
        WebsiteType.Blog or WebsiteType.News =>
        [
            AdminNavArea.Users, AdminNavArea.Website, AdminNavArea.Messages,
            AdminNavArea.ContentCore, AdminNavArea.ContentBlog, AdminNavArea.ContentExtra, AdminNavArea.Media,
        ],
        WebsiteType.Gallery or WebsiteType.Portfolio =>
        [
            AdminNavArea.Users, AdminNavArea.Website, AdminNavArea.Messages,
            AdminNavArea.ContentCore, AdminNavArea.ContentExtra, AdminNavArea.Media,
        ],

        // ── Educational / professional ────────────────────────────
        WebsiteType.ELearning or WebsiteType.Membership =>
        [
            AdminNavArea.Users, AdminNavArea.Website, AdminNavArea.Messages,
            AdminNavArea.ContentCore, AdminNavArea.ContentBlog, AdminNavArea.ContentExtra, AdminNavArea.Media,
            // Courses/membership later; shop stays Advanced unless productized.
        ],
        WebsiteType.Booking or WebsiteType.MedicalBooking or WebsiteType.BeautyBooking =>
        [
            AdminNavArea.Users, AdminNavArea.Website, AdminNavArea.Messages,
            AdminNavArea.ContentCore, AdminNavArea.ContentExtra, AdminNavArea.Media,
            AdminNavArea.Orders, // appointments / bookings often surface as orders
        ],

        _ =>
        [
            AdminNavArea.Users, AdminNavArea.Website, AdminNavArea.Messages,
            AdminNavArea.ContentCore, AdminNavArea.ContentExtra, AdminNavArea.Media,
        ],
    };

    public static bool Shows(IReadOnlySet<AdminNavArea> surface, AdminNavArea area) =>
        surface.Contains(area);

    public static bool ShowsAny(IReadOnlySet<AdminNavArea> surface, params AdminNavArea[] areas) =>
        areas.Any(surface.Contains);

    private static HashSet<AdminNavArea> FullCommerce() =>
    [
        AdminNavArea.Users, AdminNavArea.Website, AdminNavArea.Messages,
        AdminNavArea.ContentCore, AdminNavArea.Media,
        AdminNavArea.Catalog, AdminNavArea.Inventory, AdminNavArea.Orders,
        AdminNavArea.Finance, AdminNavArea.Promotions,
    ];

    /// <summary>
    /// General adds a few cross-cutting extras for the same type — never the full unrelated stack
    /// (e.g. a Blog site still has no Catalog until Advanced).
    /// </summary>
    private static HashSet<AdminNavArea> GeneralExtras(WebsiteType type) => type switch
    {
        WebsiteType.DigitalCatalog or WebsiteType.RealEstate or WebsiteType.Restaurant =>
        [
            AdminNavArea.Orders, AdminNavArea.Finance, AdminNavArea.Promotions,
        ],
        WebsiteType.Crowdfunding =>
        [
            AdminNavArea.Promotions, AdminNavArea.Inventory,
        ],
        WebsiteType.ELearning or WebsiteType.Membership =>
        [
            AdminNavArea.Orders, AdminNavArea.Finance,
        ],
        WebsiteType.Booking or WebsiteType.MedicalBooking or WebsiteType.BeautyBooking =>
        [
            AdminNavArea.Finance, AdminNavArea.Promotions,
        ],
        _ => [],
    };
}
