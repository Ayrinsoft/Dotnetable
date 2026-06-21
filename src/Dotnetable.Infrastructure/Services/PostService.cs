using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class PostService : IPostService
{
    private readonly AppDbContext _context;

    public PostService(AppDbContext context) => _context = context;

    public async Task<Post?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.Posts.FindAsync([id], ct);

    public async Task<Post?> GetBySlugAsync(int websiteId, string slug, int languageId, CancellationToken ct = default) =>
        await _context.Posts.FirstOrDefaultAsync(
            p => p.WebsiteId == websiteId && p.Slug == slug && p.LanguageId == languageId, ct);

    public async Task<IEnumerable<Post>> GetByWebsiteAsync(int websiteId, PostType? type = null, PostStatus? status = null, CancellationToken ct = default)
    {
        var q = _context.Posts.Where(p => p.WebsiteId == websiteId);
        if (type.HasValue) q = q.Where(p => p.Type == type.Value);
        if (status.HasValue) q = q.Where(p => p.Status == status.Value);
        return await q.OrderByDescending(p => p.CreatedAt).ToListAsync(ct);
    }

    public async Task<Post> CreateAsync(Post post, CancellationToken ct = default)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync(ct);
        return post;
    }

    public async Task UpdateAsync(Post post, CancellationToken ct = default)
    {
        post.UpdatedAt = DateTime.UtcNow;
        _context.Posts.Update(post);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var post = await _context.Posts.FindAsync([id], ct);
        if (post is null) return;
        _context.Posts.Remove(post);
        await _context.SaveChangesAsync(ct);
    }
}
