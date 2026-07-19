using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class PostTypeService : IPostTypeService
{
    private readonly AppDbContext _context;

    public PostTypeService(AppDbContext context) => _context = context;

    public async Task<List<PostType>> GetAllAsync(int? websiteId, CancellationToken ct = default)
    {
        var q = _context.PostTypes.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(t => t.WebsiteID == wid);
        return await q.OrderBy(t => t.Name).ToListAsync(ct);
    }

    public async Task<PagedResult<PostType>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.PostTypes.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(t => t.WebsiteID == wid);

        if (query.GetSearch(nameof(PostType.Name)) is string name)
            q = q.Where(t => t.Name.Contains(name));
        if (query.GetSearch(nameof(PostType.Slug)) is string slug)
            q = q.Where(t => t.Slug.Contains(slug));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(PostType.Name))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<PostType> { Items = items, TotalCount = total };
    }

    public async Task<PostType?> GetByIdAsync(int postTypeId, CancellationToken ct = default) =>
        await _context.PostTypes.FindAsync([postTypeId], ct);

    public async Task<PostType> CreateAsync(PostType postType, CancellationToken ct = default)
    {
        _context.PostTypes.Add(postType);
        await _context.SaveChangesAsync(ct);
        return postType;
    }

    public async Task UpdateAsync(PostType postType, CancellationToken ct = default)
    {
        _context.PostTypes.Update(postType);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int postTypeId, CancellationToken ct = default)
    {
        var entity = await _context.PostTypes.FindAsync([postTypeId], ct);
        if (entity is null) return;
        _context.PostTypes.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
