using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Post tags with per-language translations, scoped to a single website.</summary>
public interface ITagService
{
    Task<List<Tag>> GetAllAsync(int? websiteId, CancellationToken ct = default);
    Task<Tag?> GetByIdAsync(int tagId, CancellationToken ct = default);
    Task<Tag> CreateAsync(Tag tag, CancellationToken ct = default);
    Task UpdateAsync(Tag tag, CancellationToken ct = default);
    Task DeleteAsync(int tagId, CancellationToken ct = default);

    // ── Translations ────────────────────────────────────────────────
    Task<List<TagTranslation>> GetTranslationsAsync(int tagId, CancellationToken ct = default);

    /// <summary>Replaces the tag's translations with the supplied language→(name, slug) map
    /// (blank names remove that language's translation).</summary>
    Task SetTranslationsAsync(int tagId, IReadOnlyDictionary<string, (string Name, string Slug)> byLanguage, CancellationToken ct = default);
}
