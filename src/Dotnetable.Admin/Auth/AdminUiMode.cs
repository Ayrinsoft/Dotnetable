namespace Dotnetable.Admin.Auth;

/// <summary>Mirrors <c>Member.AdminUIMode</c> — how much of the admin surface a member sees.</summary>
public enum AdminUiMode : byte
{
    /// <summary>Admin surface reduced to what this member's Website.WebsiteType needs.</summary>
    Basic = 0,

    /// <summary>Same admin surface regardless of website type.</summary>
    General = 1,

    /// <summary>Full surface, e.g. extra-language pages/buttons on an otherwise single-language site.</summary>
    Advanced = 2,
}
