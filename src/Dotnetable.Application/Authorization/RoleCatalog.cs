namespace Dotnetable.Application.Authorization;

/// <summary>Distinguishes admin-panel permissions from website-customer (client) permissions.</summary>
public enum RoleCategory : byte
{
    /// <summary>Permission used inside the admin panel.</summary>
    Admin = 0,

    /// <summary>Permission for website customers (purchase, review, profile, …) — never shown in admin.</summary>
    Client = 1,
}

/// <summary>A single seedable permission.</summary>
public sealed record RoleDefinition(string Key, string Description, RoleCategory Category)
{
    /// <summary>The "area" segment of the key (text before the first dot) used to group keys into a tree.</summary>
    public string Group => Key.Split('.', 2)[0];
}

/// <summary>
/// The full set of permissions the system ships with. Seeded on first run and used to register
/// one authorization policy per admin key. Adding a new permission here (plus a migration-free seed
/// top-up) is all that is needed to introduce a new page/action guard.
/// </summary>
public static class RoleCatalog
{
    public static readonly IReadOnlyList<RoleDefinition> All =
    [
        // Members
        new(RoleKeys.MembersView, "View members", RoleCategory.Admin),
        new(RoleKeys.MembersInsert, "Create members", RoleCategory.Admin),
        new(RoleKeys.MembersEdit, "Edit members", RoleCategory.Admin),
        new(RoleKeys.MembersDelete, "Delete members", RoleCategory.Admin),

        // Access levels (policies)
        new(RoleKeys.PoliciesView, "View access levels", RoleCategory.Admin),
        new(RoleKeys.PoliciesInsert, "Create access levels", RoleCategory.Admin),
        new(RoleKeys.PoliciesEdit, "Edit access levels", RoleCategory.Admin),
        new(RoleKeys.PoliciesDelete, "Delete access levels", RoleCategory.Admin),

        // Website settings
        new(RoleKeys.WebsiteView, "View website settings", RoleCategory.Admin),
        new(RoleKeys.WebsiteEdit, "Edit website settings", RoleCategory.Admin),

        // Messages
        new(RoleKeys.MessagesView, "View contact messages", RoleCategory.Admin),
        new(RoleKeys.MessagesDelete, "Delete contact messages", RoleCategory.Admin),

        // Localization
        new(RoleKeys.LocalizationView, "View translations", RoleCategory.Admin),
        new(RoleKeys.LocalizationEdit, "Edit translations", RoleCategory.Admin),

        // Media
        new(RoleKeys.MediaView, "View media library", RoleCategory.Admin),
        new(RoleKeys.MediaUpload, "Upload media & manage storage", RoleCategory.Admin),
        new(RoleKeys.MediaDelete, "Delete media", RoleCategory.Admin),

        // Login logs
        new(RoleKeys.LoginLogsView, "View login logs", RoleCategory.Admin),

        // Client (website customers)
        new(RoleKeys.ClientAccess, "Sign in and general site access", RoleCategory.Client),
        new(RoleKeys.ClientPurchase, "Place orders / purchase", RoleCategory.Client),
        new(RoleKeys.ClientReview, "Post product reviews / comments", RoleCategory.Client),
        new(RoleKeys.ClientProfile, "Manage own profile", RoleCategory.Client),
    ];

    /// <summary>Admin permission keys — one authorization policy is registered per key.</summary>
    public static readonly IReadOnlyList<string> AdminKeys =
        All.Where(r => r.Category == RoleCategory.Admin).Select(r => r.Key).ToList();

    /// <summary>Client permission keys (website customers).</summary>
    public static readonly IReadOnlyList<string> ClientKeys =
        All.Where(r => r.Category == RoleCategory.Client).Select(r => r.Key).ToList();
}
