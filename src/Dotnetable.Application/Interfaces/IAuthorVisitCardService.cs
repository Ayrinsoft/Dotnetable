using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Digital visiting cards of content authors: edited from the admin author screen, read by the public
/// site at <c>/c/{slug}</c>, with a phone book of people who asked to be called back.
/// </summary>
public interface IAuthorVisitCardService
{
    Task<AuthorVisitCardEditorState> GetEditorAsync(int memberId, CancellationToken ct = default);

    /// <summary>Creates or updates the member's card. Returns an error message, or null when saved.</summary>
    Task<string?> SaveAsync(int memberId, AuthorVisitCardEdit card, CancellationToken ct = default);

    Task<VisitCardPageDto?> GetPublicAsync(int websiteId, string slug, CancellationToken ct = default);

    /// <summary>vCard 3.0 text of the card owner, or null when the card is not public.</summary>
    Task<string?> BuildPublicVcfAsync(int websiteId, string slug, CancellationToken ct = default);

    /// <summary>
    /// Stores a visitor's name, phone and/or email and why they should be contacted.
    /// Returns null on success, or an error code: <c>name</c>, <c>contact</c>, <c>mobile</c>, <c>email</c>,
    /// <c>reason</c>, <c>disabled</c>, <c>missing</c>, <c>full</c>.
    /// </summary>
    Task<string?> InsertContactAsync(int websiteId, string slug, VisitCardContactRequest request, CancellationToken ct = default);

    Task<VisitCardPhoneBookPage> ListContactsAsync(int memberId, string? search, bool onlySaved, int page, int pageSize, CancellationToken ct = default);

    Task<bool> UpdateContactAsync(int memberId, int contactId, string? displayName, string? note, CancellationToken ct = default);

    Task<bool> DeleteContactAsync(int memberId, int contactId, CancellationToken ct = default);
}
