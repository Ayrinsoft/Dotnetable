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

    /// <summary>True when listing stock is unlimited (digital; StockQuantity &lt; 0).</summary>
    static bool IsUnlimited(VendorProduct vp) => vp.StockQuantity < 0;

    /// <summary>
    /// Sellable units for a listing (StockQuantity - QuantityReserved).
    /// Returns <see cref="int.MaxValue"/> when stock is unlimited (-1).
    /// </summary>
    static int Available(VendorProduct vp) =>
        IsUnlimited(vp) ? int.MaxValue : Math.Max(0, vp.StockQuantity - vp.QuantityReserved);

    /// <summary>Reserves listing stock at checkout. Fails when available &lt; qty.</summary>
    Task<bool> ReserveAsync(int vendorProductId, int qty, CancellationToken ct = default);

    /// <summary>Releases a reservation (cancel / unpaid order).</summary>
    Task ReleaseReservationAsync(int vendorProductId, int qty, CancellationToken ct = default);

    /// <summary>
    /// Completes a paid sale: decrements StockQuantity and QuantityReserved, then
    /// rebuilds host <c>InventoryItem.QuantityOnHand</c> as the sum of listing stocks for the variant.
    /// </summary>
    Task CommitSaleAsync(int vendorProductId, int qty, CancellationToken ct = default);

    /// <summary>Restores listing stock after a sellable customer return (not unlimited listings).</summary>
    Task RestockAsync(int vendorProductId, int qty, CancellationToken ct = default);

    /// <summary>
    /// Restock or create a listing for a given commercial condition (New vs OpenBox/Display/Used).
    /// Non-new listings are tagged with condition + health grade for the used-goods channel.
    /// </summary>
    Task RestockWithConditionAsync(
        int websiteId, int vendorId, int productVariantId, int qty,
        byte itemCondition, byte healthGrade, CancellationToken ct = default);

    /// <summary>
    /// Sets <c>InventoryItems.QuantityOnHand</c> for the host website + variant to the sum of
    /// all <see cref="VendorProduct.StockQuantity"/> rows on that host (source of truth = store listings).
    /// Preserves QuantityReserved already held on the inventory row.
    /// </summary>
    Task SyncInventoryOnHandFromListingsAsync(int websiteId, int productVariantId, CancellationToken ct = default);

    /// <summary>
    /// Exports current listing price/stock for bulk edit. Scope: host <paramref name="websiteId"/>,
    /// optionally a single <paramref name="vendorId"/>. When <paramref name="actingMemberId"/> is set,
    /// only that member’s own vendor listings are included.
    /// </summary>
    Task<byte[]> ExportListingsExcelAsync(
        int websiteId, int? vendorId = null, int? actingMemberId = null, CancellationToken ct = default);

    /// <summary>
    /// Applies an Excel file produced by <see cref="ExportListingsExcelAsync"/> (same columns).
    /// Updates only existing listings in scope; does not create new rows. Rows with validation
    /// errors are skipped and listed in the result.
    /// </summary>
    Task<VendorListingImportResult> ImportListingsExcelAsync(
        int websiteId, Stream excel, int? vendorId = null, int? actingMemberId = null, CancellationToken ct = default);
}
