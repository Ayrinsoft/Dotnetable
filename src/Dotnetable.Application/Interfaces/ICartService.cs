using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Shopping cart for guests (keyed by a random <see cref="Cart.SessionKey"/>, usually stored in a
/// cookie) and signed-in customers (keyed by <see cref="Cart.WebsiteClientID"/>). Prices are never
/// stored on <see cref="CartItem"/> — every read recomputes them live from <see cref="ProductVariant.ReferencePriceUsd"/>
/// (or <see cref="ProductVariant.OverridePrice"/>) via <see cref="ICurrencyConversionService"/>, so a
/// price change is reflected immediately. Prices are only ever snapshotted onto <c>OrderItem</c> at checkout.
/// </summary>
public interface ICartService
{
    /// <summary>Finds or creates the cart for a signed-in client, or a guest cart keyed by session.</summary>
    Task<Cart> GetOrCreateAsync(int websiteId, int? clientId, string? sessionKey, CancellationToken ct = default);

    /// <summary>The cart priced for display, including a coupon preview when one is applied.</summary>
    Task<CartViewDto> GetCartViewAsync(int websiteId, int cartId, string? currencyCode = null, CancellationToken ct = default);

    /// <summary>Adds a variant to the cart, merging quantity into an existing line for the same variant+vendor.</summary>
    Task AddItemAsync(int websiteId, int cartId, int variantId, int quantity, int? vendorProductId = null, CancellationToken ct = default);

    Task UpdateQuantityAsync(int cartId, int cartItemId, int quantity, CancellationToken ct = default);

    Task RemoveItemAsync(int cartId, int cartItemId, CancellationToken ct = default);

    Task ClearAsync(int cartId, CancellationToken ct = default);

    /// <summary>Moves every line from a guest cart into (or creates) the client's cart on login, summing
    /// quantities on collision, then deletes the guest cart.</summary>
    Task MergeGuestCartAsync(int websiteId, string sessionKey, int clientId, CancellationToken ct = default);

    Task<(bool Success, string? Error)> ApplyCouponAsync(int websiteId, int cartId, string code, int? clientId, CancellationToken ct = default);

    Task RemoveCouponAsync(int cartId, CancellationToken ct = default);
}
