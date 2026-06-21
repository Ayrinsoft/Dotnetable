using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface IPostService
{
    Task<Post?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Post?> GetBySlugAsync(int websiteId, string slug, int languageId, CancellationToken ct = default);
    Task<IEnumerable<Post>> GetByWebsiteAsync(int websiteId, PostType? type = null, PostStatus? status = null, CancellationToken ct = default);
    Task<Post> CreateAsync(Post post, CancellationToken ct = default);
    Task UpdateAsync(Post post, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
