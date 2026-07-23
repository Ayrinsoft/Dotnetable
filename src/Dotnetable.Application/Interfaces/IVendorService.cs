using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Marketplace vendors (sellers) with three modes: display-only title, member-managed catalog,
/// or another website linked via virtual credit. Scoped per host website.
/// </summary>
public interface IVendorService
{
    Task<List<Vendor>> GetAllAsync(int? websiteId, CancellationToken ct = default);
    Task<PagedResult<Vendor>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);
    Task<Vendor?> GetByIdAsync(int vendorId, CancellationToken ct = default);
    Task<Vendor?> GetByMemberIdAsync(int memberId, CancellationToken ct = default);
    Task<Vendor> CreateAsync(Vendor vendor, CancellationToken ct = default);
    Task UpdateAsync(Vendor vendor, CancellationToken ct = default);
    Task DeleteAsync(int vendorId, CancellationToken ct = default);

    Task<List<VendorTranslation>> GetTranslationsAsync(int vendorId, CancellationToken ct = default);
    Task SetTranslationsAsync(int vendorId, IReadOnlyDictionary<string, string> nameByLanguage, CancellationToken ct = default);

    /// <summary>Active vendors for a website, localized.</summary>
    Task<List<VendorDto>> GetActiveAsync(int websiteId, string? languageCode = null, CancellationToken ct = default);

    /// <summary>
    /// Site-linked vendors on <paramref name="hostWebsiteId"/> that currently expose catalog
    /// (active + credit available or immediate settlement). Only own products of LinkedWebsite are eligible.
    /// </summary>
    Task<List<Vendor>> GetActiveSiteLinksAsync(int hostWebsiteId, CancellationToken ct = default);

    /// <summary>Whether a site-linked vendor currently may show/sell catalog on the host.</summary>
    bool CanExposeCatalog(Vendor vendor);
}
