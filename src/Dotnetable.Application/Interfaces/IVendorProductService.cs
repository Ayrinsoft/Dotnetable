using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Per-vendor listings of product variants (price/stock/delivery overrides) on a host website.
/// For site-linked vendors, only variants whose product is owned by the linked website may be listed
/// (no re-sharing of products the linked site itself imports from others).
/// </summary>
public interface IVendorProductService
{
    Task<PagedResult<VendorProductListItemDto>> GetPagedAsync(int vendorId, GridQuery query, CancellationToken ct = default);

    /// <summary>
    /// All marketplace listings for every variant of a product (each seller’s own stock/price).
    /// Used on the product edit form so stock can be set per vendor without site-linking.
    /// </summary>
    Task<List<VendorProductListItemDto>> GetByProductIdAsync(int productId, CancellationToken ct = default);

    Task<VendorProduct?> GetByIdAsync(int vendorProductId, CancellationToken ct = default);
    Task<VendorProduct?> FindAsync(int vendorId, int productVariantId, CancellationToken ct = default);

    /// <summary>
    /// Search product variants the vendor is allowed to list (ownership rules), for the add-listing picker.
    /// </summary>
    Task<List<VendorVariantPickDto>> SearchEligibleVariantsAsync(
        int vendorId, string? search, int take = 25, bool includeAlreadyListed = false, CancellationToken ct = default);

    /// <summary>
    /// Creates or updates a vendor listing. Enforces ownership rules for site-linked vendors
    /// and member ownership for member-managed vendors when <paramref name="actingMemberId"/> is set.
    /// </summary>
    Task<(bool Success, string? Error, VendorProduct? Item)> UpsertAsync(
        VendorProduct model, int? actingMemberId = null, CancellationToken ct = default);

    Task<(bool Success, string? Error)> DeleteAsync(int vendorProductId, int? actingMemberId = null, CancellationToken ct = default);

    /// <summary>
    /// Ensures a VendorProduct row exists for a site-linked (or member) sale of a source-owned variant
    /// and returns it. Used when a customer adds a linked-site product from the host catalog.
    /// </summary>
    Task<VendorProduct?> EnsureListingForVariantAsync(
        int hostWebsiteId, int vendorId, int productVariantId, CancellationToken ct = default);

    /// <summary>Active listings for a vendor (admin or storefront helper).</summary>
    Task<List<VendorProduct>> GetActiveByVendorAsync(int vendorId, CancellationToken ct = default);
}
