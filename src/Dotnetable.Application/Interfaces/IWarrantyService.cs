using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Reusable warranty catalog per website (company / official plans). Products pick these or use free text.</summary>
public interface IWarrantyService
{
    Task<List<Warranty>> GetAllAsync(int? websiteId, bool activeOnly = false, CancellationToken ct = default);
    Task<PagedResult<Warranty>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);
    Task<Warranty?> GetByIdAsync(int warrantyId, CancellationToken ct = default);
    Task<Warranty> CreateAsync(Warranty warranty, CancellationToken ct = default);
    Task UpdateAsync(Warranty warranty, CancellationToken ct = default);
    Task DeleteAsync(int warrantyId, CancellationToken ct = default);

    Task<List<WarrantyTranslation>> GetTranslationsAsync(int warrantyId, CancellationToken ct = default);
    Task SetTranslationsAsync(int warrantyId, IReadOnlyDictionary<string, (string Title, string? Description, string? ProviderName)> byLanguage, CancellationToken ct = default);
}
