using Dotnetable.Application.Content;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ContentVersionService : IContentVersionService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public ContentVersionService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<List<ContentVersion>> GetAllAsync(int websiteId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var rows = await context.ContentVersions.AsNoTracking()
            .Where(v => v.WebsiteID == websiteId)
            .ToListAsync(ct);
        return Order(rows);
    }

    public async Task<List<ContentVersion>> GetPublishedAsync(int websiteId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var rows = await context.ContentVersions.AsNoTracking()
            .Where(v => v.WebsiteID == websiteId && v.IsPublished)
            .ToListAsync(ct);
        return Order(rows);
    }

    public async Task<ContentVersion> SaveAsync(ContentVersion version, CancellationToken ct = default)
    {
        version.Name = ContentEditionRules.NormalizeVersion(version.Name)
            ?? throw new ContentEditionException("Version name is required.");

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var duplicate = await context.ContentVersions.AnyAsync(v =>
            v.WebsiteID == version.WebsiteID && v.ContentVersionID != version.ContentVersionID && v.Name == version.Name, ct);
        if (duplicate)
            throw new ContentEditionException("This website already has that version name.");

        ContentVersion saved;
        if (version.ContentVersionID == 0)
        {
            context.ContentVersions.Add(version);
            saved = version;
        }
        else
        {
            saved = await context.ContentVersions.FirstOrDefaultAsync(v => v.ContentVersionID == version.ContentVersionID, ct)
                ?? throw new ContentEditionException("Content version was not found.");
            if (saved.WebsiteID != version.WebsiteID)
                throw new ContentEditionException("Content version belongs to another website.");
            saved.Name = version.Name;
            saved.SortOrder = version.SortOrder;
            saved.IsLatest = version.IsLatest;
            saved.IsPublished = version.IsPublished;
        }

        if (saved.IsLatest)
        {
            var others = await context.ContentVersions
                .Where(v => v.WebsiteID == saved.WebsiteID && v.IsLatest && v.ContentVersionID != saved.ContentVersionID)
                .ToListAsync(ct);
            foreach (var other in others)
                other.IsLatest = false;
        }

        await context.SaveChangesAsync(ct);
        return saved;
    }

    public async Task DeleteAsync(int contentVersionId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var row = await context.ContentVersions.FirstOrDefaultAsync(v => v.ContentVersionID == contentVersionId, ct);
        if (row is null) return;
        context.ContentVersions.Remove(row);
        await context.SaveChangesAsync(ct);
    }

    private static List<ContentVersion> Order(List<ContentVersion> rows) =>
        rows.OrderBy(v => v.SortOrder).ThenBy(v => v.Name, ContentEditionRules.LabelComparer).ToList();
}
