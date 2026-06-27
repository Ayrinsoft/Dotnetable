namespace Dotnetable.Application.Authorization;

/// <summary>
/// Canonical permission keys stored in the Role table (Role.RoleKey).
/// Keys follow an "area.action" convention so the admin UI can render them as a tree
/// (the part before the dot is the group, the part after is the action).
/// Authorization is role-based: every page/action requires one specific key, and a member
/// is allowed only when the key is present in the roles granted through their policy
/// (master-website members bypass the check entirely).
/// </summary>
public static class RoleKeys
{
    // ── Members (admin) ─────────────────────────────────────────────
    public const string MembersView = "members.view";
    public const string MembersInsert = "members.insert";
    public const string MembersEdit = "members.edit";
    public const string MembersDelete = "members.delete";

    // ── Access levels / policies (admin) ────────────────────────────
    public const string PoliciesView = "policies.view";
    public const string PoliciesInsert = "policies.insert";
    public const string PoliciesEdit = "policies.edit";
    public const string PoliciesDelete = "policies.delete";

    // ── Website settings: IP, scripts, SEO, social, email (admin) ───
    public const string WebsiteView = "website.view";
    public const string WebsiteEdit = "website.edit";

    // ── Contact messages (admin) ────────────────────────────────────
    public const string MessagesView = "messages.view";
    public const string MessagesDelete = "messages.delete";

    // ── Localization / translations (admin) ─────────────────────────
    public const string LocalizationView = "localization.view";
    public const string LocalizationEdit = "localization.edit";

    // ── Media library (admin) ───────────────────────────────────────
    public const string MediaView = "media.view";
    public const string MediaUpload = "media.upload";
    public const string MediaDelete = "media.delete";

    // ── Login logs (admin) ──────────────────────────────────────────
    public const string LoginLogsView = "loginlogs.view";

    // ── Client (website customers) — never used in the admin panel ──
    public const string ClientAccess = "client.access";
    public const string ClientPurchase = "client.purchase";
    public const string ClientReview = "client.review";
    public const string ClientProfile = "client.profile";
}
