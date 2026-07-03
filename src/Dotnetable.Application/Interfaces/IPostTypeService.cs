using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Post types (e.g. Blog, News, Portfolio) that group posts and decide which features
/// (categories, tags, author, comments) apply. Scoped to a single website.
/// </summary>
public interface IPostTypeService
{
    /// <summary>All post types for a website (or every website when <paramref name="websiteId"/> is null — master only).</summary>
    Task<List<PostType>> GetAllAsync(int? websiteId, CancellationToken ct = default);

    Task<PostType?> GetByIdAsync(int postTypeId, CancellationToken ct = default);
    Task<PostType> CreateAsync(PostType postType, CancellationToken ct = default);
    Task UpdateAsync(PostType postType, CancellationToken ct = default);
    Task DeleteAsync(int postTypeId, CancellationToken ct = default);
}
