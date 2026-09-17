using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Author bios and online résumés of admin members: managed from the admin panel (each member edits
/// their own; site owners can edit anyone on their website) and read by the public site through the
/// API. The profile's website is always the member's own website — callers cannot move it.
/// </summary>
public interface IAuthorProfileService
{
    // ── Admin management ────────────────────────────────────────────

    /// <summary>Every admin member of a website (all websites when null) with their author-profile state.</summary>
    Task<List<AuthorListRow>> GetAuthorsAsync(int? websiteId, CancellationToken ct = default);

    /// <summary>The member's profile with translations and résumé items (+ their translations) loaded,
    /// or null when the member has not set one up yet.</summary>
    Task<AuthorProfile?> GetByMemberAsync(int memberId, CancellationToken ct = default);

    /// <summary>Creates or updates the member's profile (<see cref="AuthorProfile.MemberID"/> selects it)
    /// and replaces its translations. The slug is normalised and made unique within the website; a blank
    /// slug is derived from the member's name. Unsafe (non-http) URLs are dropped.</summary>
    Task<AuthorProfile> SaveAsync(AuthorProfile profile, IReadOnlyList<AuthorProfileTranslation> translations, CancellationToken ct = default);

    /// <summary>Adds or updates one résumé item of the member's profile and replaces its translations.
    /// Throws when the member has no profile yet, or the item belongs to someone else.</summary>
    Task<AuthorResumeItem> SaveResumeItemAsync(int memberId, AuthorResumeItem item, IReadOnlyList<AuthorResumeItemTranslation> translations, CancellationToken ct = default);

    /// <summary>Deletes one of the member's résumé items. No-op when it isn't theirs.</summary>
    Task DeleteResumeItemAsync(int memberId, int itemId, CancellationToken ct = default);

    // ── Public read ─────────────────────────────────────────────────

    /// <summary>The public author page for a slug, or null when there is no such author, the member is
    /// inactive or the author has not enabled their résumé page.</summary>
    Task<AuthorPageDto?> GetPublicAsync(int websiteId, string slug, string? languageCode = null, CancellationToken ct = default);
}
