using Dotnetable.Domain.Enums;

namespace Dotnetable.Admin.Auth;

/// <summary>
/// High-level admin drawer / search areas. Visibility is the intersection of:
/// role permissions (AuthorizeView) × <see cref="AdminUiMode"/> × website type (non-master) × vendor-member scope.
/// </summary>
public enum AdminNavArea
{
    /// <summary>Members, customers, policies, login logs.</summary>
    Users,

    /// <summary>Website settings (SEO, scripts, languages, …).</summary>
    Website,

    /// <summary>Contact messages, email accounts/templates.</summary>
    Messages,

    /// <summary>Pages, menus, slideshows, themes (always useful on brochure/CMS sites).</summary>
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
/// Master website (site 1) and Advanced mode always get the full surface (still role-gated in UI).
/// Basic/General trim the surface to what the site type needs so a pure CMS blog is not flooded with shop menus.
/// Member vendors only get the seller-relevant areas regardless of mode.
/// </summary>
public static class AdminNavSurface
{
    private static readonly AdminNavArea[] AllAreas = Enum.GetValues<AdminNavArea>();

    public static IReadOnlySet<AdminNavArea> Resolve(
        AdminUiMode mode,
        WebsiteCategory? category,
        bool isMaster,
        bool isVendorMember)
    {
        if (isMaster)
            return new HashSet<AdminNavArea>(AllAreas);

        // Marketplace seller account: only their day-to-day tools (products, listings, media, orders).
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

        // Advanced: every area the role system allows (UI still checks policies).
        if (mode == AdminUiMode.Advanced)
            return new HashSet<AdminNavArea>(AllAreas);

        // Basic / General: type-aware surface. General is a bit broader within the same family.
        var cat = category ?? WebsiteCategory.Informational;
        var basic = cat switch
        {
            WebsiteCategory.Commerce => CommerceBasic(),
            WebsiteCategory.Content => ContentBasic(),
            WebsiteCategory.Educational => EducationalBasic(),
            _ => InformationalBasic(),
        };

        if (mode == AdminUiMode.General)
            basic.UnionWith(GeneralExtras(cat));

        return basic;
    }

    public static bool Shows(IReadOnlySet<AdminNavArea> surface, AdminNavArea area) =>
        surface.Contains(area);

    public static bool ShowsAny(IReadOnlySet<AdminNavArea> surface, params AdminNavArea[] areas) =>
        areas.Any(surface.Contains);

    private static HashSet<AdminNavArea> InformationalBasic() =>
    [
        AdminNavArea.Users,
        AdminNavArea.Website,
        AdminNavArea.Messages,
        AdminNavArea.ContentCore,
        AdminNavArea.ContentExtra,
        AdminNavArea.Media,
    ];

    private static HashSet<AdminNavArea> ContentBasic() =>
    [
        AdminNavArea.Users,
        AdminNavArea.Website,
        AdminNavArea.Messages,
        AdminNavArea.ContentCore,
        AdminNavArea.ContentBlog,
        AdminNavArea.ContentExtra,
        AdminNavArea.Media,
    ];

    private static HashSet<AdminNavArea> EducationalBasic() =>
    [
        AdminNavArea.Users,
        AdminNavArea.Website,
        AdminNavArea.Messages,
        AdminNavArea.ContentCore,
        AdminNavArea.ContentBlog,
        AdminNavArea.ContentExtra,
        AdminNavArea.Media,
        // Membership / booking sites often still need customers + light commerce later via Advanced.
    ];

    private static HashSet<AdminNavArea> CommerceBasic() =>
    [
        AdminNavArea.Users,
        AdminNavArea.Website,
        AdminNavArea.Messages,
        AdminNavArea.ContentCore, // pages/menus for the storefront shell
        AdminNavArea.Media,
        AdminNavArea.Catalog,
        AdminNavArea.Inventory,
        AdminNavArea.Orders,
        AdminNavArea.Finance,
        AdminNavArea.Promotions,
    ];

    /// <summary>
    /// Extra areas unlocked in General (still type-aware — never dumps the full shop onto a CMS site).
    /// </summary>
    private static HashSet<AdminNavArea> GeneralExtras(WebsiteCategory cat) => cat switch
    {
        WebsiteCategory.Commerce =>
        [
            AdminNavArea.ContentBlog, // store blog / marketing posts
            AdminNavArea.ContentExtra,
        ],
        WebsiteCategory.Content or WebsiteCategory.Informational or WebsiteCategory.Educational =>
        [
            // Still no Catalog/Orders — switch to Advanced for shop modules on a CMS site.
        ],
        _ => [],
    };
}
