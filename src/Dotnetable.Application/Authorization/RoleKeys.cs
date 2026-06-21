namespace Dotnetable.Application.Authorization;

/// <summary>
/// Canonical permission keys stored in the Role table (Role.RoleKey).
/// The administrator policy created during first-run setup is granted every role listed here.
/// </summary>
public static class RoleKeys
{
    public const string ManageWebsites = "manage.websites";
    public const string ManageMembers = "manage.members";
    public const string ManageContent = "manage.content";
    public const string ManageLocalization = "manage.localization";
    public const string ManageSettings = "manage.settings";

    /// <summary>All role keys seeded for a full-access (administrator) policy.</summary>
    public static readonly IReadOnlyList<string> All =
    [
        ManageWebsites,
        ManageMembers,
        ManageContent,
        ManageLocalization,
        ManageSettings,
    ];
}
