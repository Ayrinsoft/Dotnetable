using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Reads and writes the product agent JSON (file import and <c>POST /api/product-agent</c>).
/// Prices are in the website's operational currency. Images, seller stock, and related products are not part of the document.
/// </summary>
public interface IProductAgentService
{
    /// <summary>
    /// Resolves brands, categories, and variant options. Does not write the product.
    /// Pass <paramref name="existingProductId"/> so variants keep their ids when the SKU matches.
    /// </summary>
    Task<ProductAgentPrepared> PrepareAsync(int websiteId, string json, int? existingProductId, CancellationToken ct = default);

    /// <summary>
    /// Creates or updates a product. Match order: <c>product_code</c> on this website, then <c>slug</c>.
    /// When <paramref name="restrictToCreatedByMemberId"/> is set, updates are refused unless that member created the product,
    /// and new products are stamped with that member.
    /// </summary>
    Task<ProductAgentApplyResult> ApplyAsync(
        int websiteId, string json, int? actingMemberId, int? restrictToCreatedByMemberId, CancellationToken ct = default);

    Task<string> ExportJsonAsync(int productId, CancellationToken ct = default);

    /// <summary>Product on this website matching a slug or a product code, including drafts. Null when none.</summary>
    Task<int?> FindProductIdAsync(int websiteId, string slugOrCode, CancellationToken ct = default);
}
