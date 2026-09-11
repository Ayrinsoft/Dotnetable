using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Text;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class TagService : ITagService
{
    private const int SlugMaxLength = 150;

    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public TagService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    /// <summary>All slugs in use by this website's tags (main + translation rows), excluding
    /// <paramref name="excludeTagId"/> — the lookup route matches either, so uniqueness must span
    /// both.</summary>
    private static async Task<HashSet<string>> GetUsedSlugsAsync(
        AppDbContext context, int websiteId, int excludeTagId, CancellationToken ct)
    {
        var main = await context.Tags.AsNoTracking()
            .Where(t => t.WebsiteID == websiteId && t.TagID != excludeTagId)
            .Select(t => t.Slug).ToListAsync(ct);
        var translated = await context.TagTranslations.AsNoTracking()
            .Where(t => t.Tag.WebsiteID == websiteId && t.TagID != excludeTagId)
            .Select(t => t.Slug).ToListAsync(ct);
        return new HashSet<string>(main.Concat(translated), StringComparer.OrdinalIgnoreCase);
    }

    public async Task<List<Tag>> GetAllAsync(int? websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.Tags.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(t => t.WebsiteID == wid);
        return await q.OrderBy(t => t.Name).ToListAsync(ct);
    }

    public async Task<PagedResult<Tag>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.Tags.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(t => t.WebsiteID == wid);

        if (query.GetSearch(nameof(Tag.Name)) is string name)
            q = q.Where(t => t.Name.Contains(name));
        if (query.GetSearch(nameof(Tag.Slug)) is string slug)
            q = q.Where(t => t.Slug.Contains(slug));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Tag.Name))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Tag> { Items = items, TotalCount = total };
    }

    public async Task<Tag?> GetByIdAsync(int tagId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.Tags.FindAsync([tagId], ct);
    }

    public async Task<Tag> CreateAsync(Tag tag, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var used = await GetUsedSlugsAsync(_context, tag.WebsiteID, excludeTagId: 0, ct);
        tag.Slug = SlugGenerator.MakeUnique(
            SlugGenerator.Normalize(string.IsNullOrWhiteSpace(tag.Slug) ? tag.Name : tag.Slug, SlugMaxLength),
            used);

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync(ct);
        return tag;
    }

    public async Task UpdateAsync(Tag tag, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var used = await GetUsedSlugsAsync(_context, tag.WebsiteID, tag.TagID, ct);
        tag.Slug = SlugGenerator.MakeUnique(
            SlugGenerator.Normalize(string.IsNullOrWhiteSpace(tag.Slug) ? tag.Name : tag.Slug, SlugMaxLength),
            used);

        _context.Tags.Update(tag);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Tag> GetOrCreateAsync(int websiteId, string name, CancellationToken ct = default)
    {
        var trimmed = name.Trim();

        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.Tags.FirstOrDefaultAsync(
            t => t.WebsiteID == websiteId && t.Name.ToLower() == trimmed.ToLower(), ct);
        if (existing is not null) return existing;

        var used = await GetUsedSlugsAsync(_context, websiteId, excludeTagId: 0, ct);
        var slug = SlugGenerator.MakeUnique(SlugGenerator.Normalize(trimmed, SlugMaxLength), used);

        var tag = new Tag { WebsiteID = websiteId, Name = trimmed, Slug = slug };
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync(ct);
        return tag;
    }

    public async Task DeleteAsync(int tagId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var tag = await _context.Tags
            .Include(t => t.TagTranslations)
            .Include(t => t.Posts)
            .FirstOrDefaultAsync(t => t.TagID == tagId, ct);
        if (tag is null) return;

        // Drop the post↔tag links and translations before removing the tag itself.
        tag.Posts.Clear();
        _context.TagTranslations.RemoveRange(tag.TagTranslations);
        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<TagTranslation>> GetTranslationsAsync(int tagId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.TagTranslations.AsNoTracking()
            .Where(t => t.TagID == tagId)
            .ToListAsync(ct);
    }

    public async Task SetTranslationsAsync(int tagId, IReadOnlyDictionary<string, (string Name, string Slug)> byLanguage, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var tag = await _context.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.TagID == tagId, ct);
        if (tag is null) return;

        var existing = await _context.TagTranslations
            .Where(t => t.TagID == tagId)
            .ToListAsync(ct);

        var used = await GetUsedSlugsAsync(_context, tag.WebsiteID, tagId, ct);

        foreach (var (languageCode, value) in byLanguage)
        {
            var current = existing.FirstOrDefault(t =>
                string.Equals(t.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(value.Name))
            {
                if (current is not null) _context.TagTranslations.Remove(current);
                continue;
            }

            var slug = SlugGenerator.MakeUnique(
                SlugGenerator.Normalize(string.IsNullOrWhiteSpace(value.Slug) ? value.Name : value.Slug, SlugMaxLength),
                used);
            used.Add(slug);
            if (current is null)
                _context.TagTranslations.Add(new TagTranslation
                {
                    TagID = tagId,
                    LanguageCode = languageCode,
                    Name = value.Name.Trim(),
                    Slug = slug,
                });
            else
            {
                current.Name = value.Name.Trim();
                current.Slug = slug;
            }
        }

        await _context.SaveChangesAsync(ct);
    }
}
