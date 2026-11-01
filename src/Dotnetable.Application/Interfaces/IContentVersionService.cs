using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// The website's list of content release labels (help center, policy, release notes).
/// Not API versioning and not commerce products.
/// </summary>
public interface IContentVersionService
{
    Task<List<ContentVersion>> GetAllAsync(int websiteId, CancellationToken ct = default);

    /// <summary>Published labels, ordered by <see cref="ContentVersion.SortOrder"/> then release name.</summary>
    Task<List<ContentVersion>> GetPublishedAsync(int websiteId, CancellationToken ct = default);

    Task<ContentVersion> SaveAsync(ContentVersion version, CancellationToken ct = default);
    Task DeleteAsync(int contentVersionId, CancellationToken ct = default);
}
