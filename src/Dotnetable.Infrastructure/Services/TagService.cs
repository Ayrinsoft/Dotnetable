using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class TagService : ITagService
{
    private readonly AppDbContext _context;

    public TagService(AppDbContext context) => _context = context;

    public async Task<List<Tag>> GetAllAsync(int? websiteId, CancellationToken ct = default)
    {
        var q = _context.Tags.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(t => t.WebsiteID == wid);
        return await q.OrderBy(t => t.Name).ToListAsync(ct);
    }

    public async Task<Tag?> GetByIdAsync(int tagId, CancellationToken ct = default) =>
        await _context.Tags.FindAsync([tagId], ct);

    public async Task<Tag> CreateAsync(Tag tag, CancellationToken ct = default)
    {
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync(ct);
        return tag;
    }

    public async Task UpdateAsync(Tag tag, CancellationToken ct = default)
    {
        _context.Tags.Update(tag);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int tagId, CancellationToken ct = default)
    {
        var tag = await _context.Tags
            .Include(t => t.TagTranslations)
            .Include(t => t.Pos)
            .FirstOrDefaultAsync(t => t.TagID == tagId, ct);
        if (tag is null) return;

        // Drop the post↔tag links and translations before removing the tag itself.
        tag.Pos.Clear();
        _context.TagTranslations.RemoveRange(tag.TagTranslations);
        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<TagTranslation>> GetTranslationsAsync(int tagId, CancellationToken ct = default) =>
        await _context.TagTranslations.AsNoTracking()
            .Where(t => t.TagID == tagId)
            .ToListAsync(ct);

    public async Task SetTranslationsAsync(int tagId, IReadOnlyDictionary<string, (string Name, string Slug)> byLanguage, CancellationToken ct = default)
    {
        var existing = await _context.TagTranslations
            .Where(t => t.TagID == tagId)
            .ToListAsync(ct);

        foreach (var (languageCode, value) in byLanguage)
        {
            var current = existing.FirstOrDefault(t =>
                string.Equals(t.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(value.Name))
            {
                if (current is not null) _context.TagTranslations.Remove(current);
                continue;
            }

            var slug = string.IsNullOrWhiteSpace(value.Slug) ? value.Name.Trim() : value.Slug.Trim();
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
