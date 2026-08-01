using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Product attribute definitions (e.g. Color, Size, Material) and their selectable options, with
/// per-language translations. Scoped per website.
/// </summary>
public interface IAttributeDefinitionService
{
    Task<List<AttributeDefinition>> GetAllAsync(int? websiteId, CancellationToken ct = default);
    Task<PagedResult<AttributeDefinition>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);

    /// <summary>A single definition with its options loaded (for the edit form).</summary>
    Task<AttributeDefinition?> GetByIdAsync(int attributeDefinitionId, CancellationToken ct = default);

    Task<AttributeDefinition> CreateAsync(AttributeDefinition definition, CancellationToken ct = default);
    Task UpdateAsync(AttributeDefinition definition, CancellationToken ct = default);
    Task DeleteAsync(int attributeDefinitionId, CancellationToken ct = default);

    // ── Definition translations ─────────────────────────────────────
    Task<List<AttributeDefinitionTranslation>> GetTranslationsAsync(int attributeDefinitionId, CancellationToken ct = default);
    Task SetTranslationsAsync(int attributeDefinitionId, IReadOnlyDictionary<string, (string Name, string? Unit)> byLanguage, CancellationToken ct = default);

    // ── Options (replace-all-children pattern) ──────────────────────
    Task<List<AttributeOption>> GetOptionsAsync(int attributeDefinitionId, CancellationToken ct = default);

    /// <summary>Replaces the full option set for a definition. Options with <c>AttributeOptionID == 0</c> are inserted.</summary>
    Task SetOptionsAsync(int attributeDefinitionId, IReadOnlyList<AttributeOption> options, CancellationToken ct = default);

    /// <summary>
    /// Finds an existing option by value (case-insensitive), optionally matching <paramref name="colorHex"/>,
    /// or creates one. Used when product variants introduce a new color/size on the fly.
    /// </summary>
    Task<AttributeOption> EnsureOptionAsync(
        int attributeDefinitionId,
        string value,
        string? colorHex = null,
        CancellationToken ct = default);

    Task<List<AttributeOptionTranslation>> GetOptionTranslationsAsync(int attributeOptionId, CancellationToken ct = default);
    Task SetOptionTranslationsAsync(int attributeOptionId, IReadOnlyDictionary<string, string> valueByLanguage, CancellationToken ct = default);
}
