using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Post tags with per-language translations, scoped to a single website.</summary>
public interface ITagService
{
    Task<List<Tag>> GetAllAsync(int? websiteId, CancellationToken ct = default);

    /// <summary>Paged / sorted / filtered tags for the admin grid.</summary>
    Task<PagedResult<Tag>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);

    Task<Tag?> GetByIdAsync(int tagId, CancellationToken ct = default);
    Task<Tag> CreateAsync(Tag tag, CancellationToken ct = default);
    Task UpdateAsync(Tag tag, CancellationToken ct = default);
    Task DeleteAsync(int tagId, CancellationToken ct = default);

    /// <summary>Returns the website's existing tag matching <paramref name="name"/> (case-insensitive,
    /// trimmed) or creates one — lets an editor (e.g. Post edit) add a tag inline by name without a
    /// duplicate being created if it already exists.</summary>
    Task<Tag> GetOrCreateAsync(int websiteId, string name, CancellationToken ct = default);

    // ── Translations ────────────────────────────────────────────────
    Task<List<TagTranslation>> GetTranslationsAsync(int tagId, CancellationToken ct = default);

    /// <summary>Replaces the tag's translations with the supplied language→(name, slug) map
    /// (blank names remove that language's translation).</summary>
    Task SetTranslationsAsync(int tagId, IReadOnlyDictionary<string, (string Name, string Slug)> byLanguage, CancellationToken ct = default);
}
