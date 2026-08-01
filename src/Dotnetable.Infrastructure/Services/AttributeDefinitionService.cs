using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
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

    public async Task<PagedResult<AttributeDefinition>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.AttributeDefinitions.AsNoTracking();
        if (websiteId is int wid)
            q = q.Where(a => a.WebsiteID == wid);

        if (query.GetSearch(nameof(AttributeDefinition.Name)) is string name)
            q = q.Where(a => a.Name.Contains(name));
        if (query.GetSearch(nameof(AttributeDefinition.Code)) is string code)
            q = q.Where(a => a.Code.Contains(code));
        if (query.GetSearch(nameof(AttributeDefinition.Active)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(a => a.Active == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(AttributeDefinition.SortOrder))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<AttributeDefinition> { Items = items, TotalCount = total };
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
            .Include(a => a.ProductCategoryAttributes)
            .Include(a => a.ProductAttributeValues).ThenInclude(v => v.ProductAttributeValueTranslations)
            .Include(a => a.VariantAttributeValues)
            .FirstOrDefaultAsync(a => a.AttributeDefinitionID == attributeDefinitionId, ct);
        if (definition is null) return;

        _context.ProductCategoryAttributes.RemoveRange(definition.ProductCategoryAttributes);
        foreach (var pav in definition.ProductAttributeValues)
            _context.ProductAttributeValueTranslations.RemoveRange(pav.ProductAttributeValueTranslations);
        _context.ProductAttributeValues.RemoveRange(definition.ProductAttributeValues);
        _context.VariantAttributeValues.RemoveRange(definition.VariantAttributeValues);

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

    public async Task<AttributeOption> EnsureOptionAsync(
        int attributeDefinitionId,
        string value,
        string? colorHex = null,
        CancellationToken ct = default)
    {
        value = (value ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Option value is required.", nameof(value));

        var normalizedHex = NormalizeColorHex(colorHex);

        var existing = await _context.AttributeOptions
            .Where(o => o.AttributeDefinitionID == attributeDefinitionId)
            .ToListAsync(ct);

        // Prefer exact name + hex match; fall back to name-only (update hex if provided).
        var match = existing.FirstOrDefault(o =>
            string.Equals(o.Value, value, StringComparison.OrdinalIgnoreCase)
            && string.Equals(NormalizeColorHex(o.ColorHex), normalizedHex, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            match = existing.FirstOrDefault(o =>
                string.Equals(o.Value, value, StringComparison.OrdinalIgnoreCase));
            if (match is not null && normalizedHex is not null
                && !string.Equals(NormalizeColorHex(match.ColorHex), normalizedHex, StringComparison.OrdinalIgnoreCase))
            {
                match.ColorHex = normalizedHex;
                await _context.SaveChangesAsync(ct);
            }
        }

        if (match is not null)
            return match;

        var sortOrder = existing.Count == 0 ? 0 : existing.Max(o => o.SortOrder) + 1;
        var created = new AttributeOption
        {
            AttributeDefinitionID = attributeDefinitionId,
            Value = value,
            ColorHex = normalizedHex,
            SortOrder = sortOrder,
        };
        _context.AttributeOptions.Add(created);
        await _context.SaveChangesAsync(ct);
        return created;
    }

    /// <summary>Normalizes to #RRGGBB uppercase, or null when empty/invalid.</summary>
    private static string? NormalizeColorHex(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return null;
        var s = hex.Trim();
        if (s.StartsWith('#')) s = s[1..];
        if (s.Length == 3)
            s = string.Concat(s.Select(c => $"{c}{c}"));
        if (s.Length == 8)
            s = s[..6]; // RRGGBBAA → RGB
        if (s.Length != 6) return null;
        foreach (var c in s)
        {
            var isHex = (c is >= '0' and <= '9') || (c is >= 'a' and <= 'f') || (c is >= 'A' and <= 'F');
            if (!isHex) return null;
        }
        return "#" + s.ToUpperInvariant();
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
