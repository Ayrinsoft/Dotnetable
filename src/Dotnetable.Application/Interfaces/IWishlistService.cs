using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>A signed-in customer's saved-for-later product variants (one wishlist per <see cref="WebsiteClient"/>).</summary>
public interface IWishlistService
{
    Task<List<WishlistItem>> GetItemsAsync(int clientId, CancellationToken ct = default);

    /// <summary>Idempotent — no-op if the variant is already saved.</summary>
    Task AddItemAsync(int websiteId, int clientId, int variantId, CancellationToken ct = default);

    Task RemoveItemAsync(int clientId, int variantId, CancellationToken ct = default);
}
