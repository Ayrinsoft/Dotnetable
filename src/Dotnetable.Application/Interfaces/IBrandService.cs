using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Product brands with per-language translations. Scoped per website.</summary>
public interface IBrandService
{
    Task<List<Brand>> GetAllAsync(int? websiteId, CancellationToken ct = default);
    Task<PagedResult<Brand>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);
    Task<Brand?> GetByIdAsync(int brandId, CancellationToken ct = default);
    Task<Brand> CreateAsync(Brand brand, CancellationToken ct = default);
    Task UpdateAsync(Brand brand, CancellationToken ct = default);
    Task DeleteAsync(int brandId, CancellationToken ct = default);

    Task<List<BrandTranslation>> GetTranslationsAsync(int brandId, CancellationToken ct = default);
    Task SetTranslationsAsync(int brandId, IReadOnlyDictionary<string, (string Name, string Slug)> byLanguage, CancellationToken ct = default);

    /// <summary>Active brands for a website, localized (for public brand pickers/lists).</summary>
    Task<List<Dotnetable.Application.DTOs.BrandDto>> GetActiveAsync(int websiteId, string? languageCode = null, CancellationToken ct = default);
}
