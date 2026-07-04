using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Marketplace vendors (sellers) with per-language translations. Scoped per website.</summary>
public interface IVendorService
{
    Task<List<Vendor>> GetAllAsync(int? websiteId, CancellationToken ct = default);
    Task<Vendor?> GetByIdAsync(int vendorId, CancellationToken ct = default);
    Task<Vendor> CreateAsync(Vendor vendor, CancellationToken ct = default);
    Task UpdateAsync(Vendor vendor, CancellationToken ct = default);
    Task DeleteAsync(int vendorId, CancellationToken ct = default);

    Task<List<VendorTranslation>> GetTranslationsAsync(int vendorId, CancellationToken ct = default);
    Task SetTranslationsAsync(int vendorId, IReadOnlyDictionary<string, string> nameByLanguage, CancellationToken ct = default);

    /// <summary>Active vendors for a website, localized.</summary>
    Task<List<Dotnetable.Application.DTOs.VendorDto>> GetActiveAsync(int websiteId, string? languageCode = null, CancellationToken ct = default);
}
