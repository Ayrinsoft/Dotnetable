using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class AttributeDefinitionService : IAttributeDefinitionService
{
    private readonly AppDbContext _context;

    public AttributeDefinitionService(AppDbContext context) => _context = context;

    public async Task<List<AttributeDefinition>> GetAllAsync(int? websiteId, CancellationToken ct = default)
    {
        var q = _context.AttributeDefinitions.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(a => a.WebsiteID == wid);
        return await q.OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToListAsync(ct);
    }

    public async Task<AttributeDefinition?> GetByIdAsync(int attributeDefinitionId, CancellationToken ct = default) =>
        await _context.AttributeDefinitions.AsNoTracking()
            .Include(a => a.AttributeOptions.OrderBy(o => o.SortOrder))
            .FirstOrDefaultAsync(a => a.AttributeDefinitionID == attributeDefinitionId, ct);

    public async Task<AttributeDefinition> CreateAsync(AttributeDefinition definition, CancellationToken ct = default)
    {
        _context.AttributeDefinitions.Add(definition);
        await _context.SaveChangesAsync(ct);
        return definition;
    }

    public async Task UpdateAsync(AttributeDefinition definition, CancellationToken ct = default)
    {
        _context.AttributeDefinitions.Update(definition);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int attributeDefinitionId, CancellationToken ct = default)
    {
        var definition = await _context.AttributeDefinitions
            .Include(a => a.AttributeDefinitionTranslations)
            .Include(a => a.AttributeOptions).ThenInclude(o => o.AttributeOptionTranslations)
            .FirstOrDefaultAsync(a => a.AttributeDefinitionID == attributeDefinitionId, ct);
        if (definition is null) return;

        foreach (var option in definition.AttributeOptions)
            _context.AttributeOptionTranslations.RemoveRange(option.AttributeOptionTranslations);
        _context.AttributeOptions.RemoveRange(definition.AttributeOptions);
        _context.AttributeDefinitionTranslations.RemoveRange(definition.AttributeDefinitionTranslations);
        _context.AttributeDefinitions.Remove(definition);
        await _context.SaveChangesAsync(ct);
    }

    // ── Definition translations ─────────────────────────────────────

    public async Task<List<AttributeDefinitionTranslation>> GetTranslationsAsync(int attributeDefinitionId, CancellationToken ct = default) =>
        await _context.AttributeDefinitionTranslations.AsNoTracking()
            .Where(t => t.AttributeDefinitionID == attributeDefinitionId)
            .ToListAsync(ct);

    public async Task SetTranslationsAsync(int attributeDefinitionId, IReadOnlyDictionary<string, (string Name, string? Unit)> byLanguage, CancellationToken ct = default)
    {
        var existing = await _context.AttributeDefinitionTranslations
            .Where(t => t.AttributeDefinitionID == attributeDefinitionId)
            .ToListAsync(ct);

        foreach (var (languageCode, value) in byLanguage)
        {
            var current = existing.FirstOrDefault(t =>
                string.Equals(t.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(value.Name))
            {
                if (current is not null) _context.AttributeDefinitionTranslations.Remove(current);
                continue;
            }

            if (current is null)
                _context.AttributeDefinitionTranslations.Add(new AttributeDefinitionTranslation
                {
                    AttributeDefinitionID = attributeDefinitionId,
                    LanguageCode = languageCode,
                    Name = value.Name.Trim(),
                    Unit = value.Unit,
                });
            else
            {
                current.Name = value.Name.Trim();
                current.Unit = value.Unit;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    // ── Options ──────────────────────────────────────────────────────

    public async Task<List<AttributeOption>> GetOptionsAsync(int attributeDefinitionId, CancellationToken ct = default) =>
        await _context.AttributeOptions.AsNoTracking()
            .Where(o => o.AttributeDefinitionID == attributeDefinitionId)
            .OrderBy(o => o.SortOrder)
            .ToListAsync(ct);

    public async Task SetOptionsAsync(int attributeDefinitionId, IReadOnlyList<AttributeOption> options, CancellationToken ct = default)
    {
        var existing = await _context.AttributeOptions
            .Where(o => o.AttributeDefinitionID == attributeDefinitionId)
            .ToListAsync(ct);

        var wantedIds = options.Where(o => o.AttributeOptionID != 0).Select(o => o.AttributeOptionID).ToHashSet();
        var toRemove = existing.Where(o => !wantedIds.Contains(o.AttributeOptionID)).ToList();
        if (toRemove.Count > 0)
        {
            var removeIds = toRemove.Select(o => o.AttributeOptionID).ToList();
            var translations = await _context.AttributeOptionTranslations
                .Where(t => removeIds.Contains(t.AttributeOptionID)).ToListAsync(ct);
            _context.AttributeOptionTranslations.RemoveRange(translations);
            _context.AttributeOptions.RemoveRange(toRemove);
        }

        var sortOrder = 0;
        foreach (var option in options)
        {
            if (option.AttributeOptionID == 0)
                _context.AttributeOptions.Add(new AttributeOption
                {
                    AttributeDefinitionID = attributeDefinitionId,
                    Value = option.Value,
                    ColorHex = option.ColorHex,
                    SortOrder = sortOrder++,
                });
            else
            {
                var current = existing.FirstOrDefault(o => o.AttributeOptionID == option.AttributeOptionID);
                if (current is not null)
                {
                    current.Value = option.Value;
                    current.ColorHex = option.ColorHex;
                    current.SortOrder = sortOrder++;
                }
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<AttributeOptionTranslation>> GetOptionTranslationsAsync(int attributeOptionId, CancellationToken ct = default) =>
        await _context.AttributeOptionTranslations.AsNoTracking()
            .Where(t => t.AttributeOptionID == attributeOptionId)
            .ToListAsync(ct);

    public async Task SetOptionTranslationsAsync(int attributeOptionId, IReadOnlyDictionary<string, string> valueByLanguage, CancellationToken ct = default)
    {
        var existing = await _context.AttributeOptionTranslations
            .Where(t => t.AttributeOptionID == attributeOptionId)
            .ToListAsync(ct);

        foreach (var (languageCode, value) in valueByLanguage)
        {
            var current = existing.FirstOrDefault(t =>
                string.Equals(t.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(value))
            {
                if (current is not null) _context.AttributeOptionTranslations.Remove(current);
                continue;
            }

            if (current is null)
                _context.AttributeOptionTranslations.Add(new AttributeOptionTranslation
                {
                    AttributeOptionID = attributeOptionId,
                    LanguageCode = languageCode,
                    Value = value.Trim(),
                });
            else
                current.Value = value.Trim();
        }

        await _context.SaveChangesAsync(ct);
    }
}
