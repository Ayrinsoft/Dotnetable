using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly AppDbContext _context;
    private readonly IInventoryService _inventory;
    private readonly ICurrencyConversionService _currency;
    private readonly ICouponService _coupons;
    private readonly IVendorProductService _vendorProducts;

    public CartService(
        AppDbContext context, IInventoryService inventory, ICurrencyConversionService currency, ICouponService coupons,
        IVendorProductService vendorProducts)
    {
        _context = context;
        _inventory = inventory;
        _currency = currency;
        _coupons = coupons;
        _vendorProducts = vendorProducts;
    }

    public async Task<Cart> GetOrCreateAsync(int websiteId, int? clientId, string? sessionKey, CancellationToken ct = default)
    {
        Cart? cart = clientId is int cid
            ? await _context.Carts.FirstOrDefaultAsync(c => c.WebsiteID == websiteId && c.WebsiteClientID == cid, ct)
            : !string.IsNullOrEmpty(sessionKey)
                ? await _context.Carts.FirstOrDefaultAsync(c => c.WebsiteID == websiteId && c.SessionKey == sessionKey && c.WebsiteClientID == null, ct)
                : null;

        if (cart is not null) return cart;

        cart = new Cart
        {
            WebsiteID = websiteId,
            WebsiteClientID = clientId,
            SessionKey = clientId is null ? sessionKey ?? Guid.NewGuid().ToString("N") : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync(ct);
        return cart;
    }

    public async Task<CartViewDto> GetCartViewAsync(int websiteId, int cartId, string? currencyCode = null, CancellationToken ct = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Coupon)
            .Include(c => c.CartItems).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.Product)
            .Include(c => c.CartItems).ThenInclude(i => i.ProductVariant).ThenInclude(v => v.ImageFile)
            .Include(c => c.CartItems).ThenInclude(i => i.VendorProduct)
            .FirstOrDefaultAsync(c => c.CartID == cartId, ct);
        if (cart is null) return new CartViewDto { CartID = cartId };

        var localVariantIds = cart.CartItems.Where(i => i.VendorProductID is null).Select(i => i.ProductVariantID).ToList();
        var availability = localVariantIds.Count > 0
            ? await _inventory.GetAvailabilityBulkAsync(websiteId, localVariantIds, ct)
            : new Dictionary<int, StockAvailability>();

        var items = new List<CartItemViewDto>();
        decimal subtotalUsd = 0;
        decimal totalWeight = 0;
        foreach (var item in cart.CartItems)
        {
            var variant = item.ProductVariant;
            // Vendor listings may override in USD. ProductVariant.OverridePrice stores the
            // display-currency snapshot (not USD); commerce always uses ReferencePriceUsd.
            var unitPriceUsd = item.VendorProduct is { } vp
                ? (vp.OverridePrice ?? vp.ReferencePriceUsd)
                : variant.ReferencePriceUsd;
            var lineTotalUsd = unitPriceUsd * item.Quantity;
            subtotalUsd += lineTotalUsd;
            totalWeight += (variant.Weight ?? 0) * item.Quantity;

            int maxPurchasable;
            bool isAvailable;
            if (item.VendorProduct is { } listing)
            {
                maxPurchasable = listing.StockQuantity;
                isAvailable = listing.IsActive && listing.StockQuantity >= item.Quantity;
            }
            else
            {
                var avail = availability.GetValueOrDefault(item.ProductVariantID);
                maxPurchasable = avail.Available;
                isAvailable = avail.Available >= item.Quantity;
            }

            items.Add(new CartItemViewDto
            {
                CartItemID = item.CartItemID,
                ProductVariantID = item.ProductVariantID,
                ProductID = variant.ProductID,
                Title = variant.Product.Title,
                Sku = variant.Sku,
                ImageUrl = variant.ImageFile?.ThumbnailCDN ?? variant.ImageFile?.CNDUrl,
                Quantity = item.Quantity,
                UnitPrice = await _currency.ToDisplayAsync(websiteId, unitPriceUsd, currencyCode, ct),
                LineTotal = await _currency.ToDisplayAsync(websiteId, lineTotalUsd, currencyCode, ct),
                IsAvailable = isAvailable,
                MaxPurchasable = maxPurchasable,
            });
        }

        MoneyDto? discount = null;
        string? couponError = null;
        if (cart.CouponID is int couponId && cart.Coupon is not null)
        {
            var (valid, error, discountUsd) = await _coupons.ValidateAndComputeAsync(
                websiteId, cart.Coupon.Code, cart.WebsiteClientID, subtotalUsd, ct);
            if (valid)
                discount = await _currency.ToDisplayAsync(websiteId, discountUsd, currencyCode, ct);
            else
                couponError = error;
        }

        return new CartViewDto
        {
            CartID = cart.CartID,
            Items = items,
            SubTotal = await _currency.ToDisplayAsync(websiteId, subtotalUsd, currencyCode, ct),
            CouponCode = cart.Coupon?.Code,
            DiscountAmount = discount,
            CouponError = couponError,
            TotalWeightKg = totalWeight,
        };
    }

    public async Task AddItemAsync(int websiteId, int cartId, int variantId, int quantity, int? vendorProductId = null, int? vendorId = null, CancellationToken ct = default)
    {
        if (quantity < 1) quantity = 1;

        if (vendorProductId is null && vendorId is int vid)
        {
            var listing = await _vendorProducts.EnsureListingForVariantAsync(websiteId, vid, variantId, ct);
            vendorProductId = listing?.VendorProductID;
        }

        var existing = await _context.CartItems.FirstOrDefaultAsync(
            i => i.CartID == cartId && i.ProductVariantID == variantId && i.VendorProductID == vendorProductId, ct);

        if (existing is not null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            _context.CartItems.Add(new CartItem
            {
                CartID = cartId,
                ProductVariantID = variantId,
                VendorProductID = vendorProductId,
                Quantity = quantity,
                AddedAt = DateTime.UtcNow,
            });
        }

        await TouchAsync(cartId, ct);
    }

    public async Task UpdateQuantityAsync(int cartId, int cartItemId, int quantity, CancellationToken ct = default)
    {
        var item = await _context.CartItems.FirstOrDefaultAsync(i => i.CartItemID == cartItemId && i.CartID == cartId, ct);
        if (item is null) return;

        if (quantity < 1)
            _context.CartItems.Remove(item);
        else
            item.Quantity = quantity;

        await TouchAsync(cartId, ct);
    }

    public async Task RemoveItemAsync(int cartId, int cartItemId, CancellationToken ct = default)
    {
        var item = await _context.CartItems.FirstOrDefaultAsync(i => i.CartItemID == cartItemId && i.CartID == cartId, ct);
        if (item is null) return;

        _context.CartItems.Remove(item);
        await TouchAsync(cartId, ct);
    }

    public async Task ClearAsync(int cartId, CancellationToken ct = default)
    {
        var items = await _context.CartItems.Where(i => i.CartID == cartId).ToListAsync(ct);
        _context.CartItems.RemoveRange(items);

        var cart = await _context.Carts.FirstOrDefaultAsync(c => c.CartID == cartId, ct);
        if (cart is not null) cart.CouponID = null;

        await TouchAsync(cartId, ct);
    }

    public async Task MergeGuestCartAsync(int websiteId, string sessionKey, int clientId, CancellationToken ct = default)
    {
        var guestCart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.WebsiteID == websiteId && c.SessionKey == sessionKey && c.WebsiteClientID == null, ct);
        if (guestCart is null || guestCart.CartItems.Count == 0)
        {
            if (guestCart is not null) { _context.Carts.Remove(guestCart); await _context.SaveChangesAsync(ct); }
            return;
        }

        var clientCart = await GetOrCreateAsync(websiteId, clientId, null, ct);

        foreach (var guestItem in guestCart.CartItems)
        {
            var existing = await _context.CartItems.FirstOrDefaultAsync(
                i => i.CartID == clientCart.CartID && i.ProductVariantID == guestItem.ProductVariantID && i.VendorProductID == guestItem.VendorProductID, ct);
            if (existing is not null)
                existing.Quantity += guestItem.Quantity;
            else
                _context.CartItems.Add(new CartItem
                {
                    CartID = clientCart.CartID,
                    ProductVariantID = guestItem.ProductVariantID,
                    VendorProductID = guestItem.VendorProductID,
                    Quantity = guestItem.Quantity,
                    AddedAt = DateTime.UtcNow,
                });
        }

        _context.CartItems.RemoveRange(guestCart.CartItems);
        _context.Carts.Remove(guestCart);
        await TouchAsync(clientCart.CartID, ct);
    }

    public async Task<(bool Success, string? Error)> ApplyCouponAsync(int websiteId, int cartId, string code, int? clientId, CancellationToken ct = default)
    {
        var cart = await _context.Carts.Include(c => c.CartItems).ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(c => c.CartID == cartId, ct);
        if (cart is null) return (false, "Cart not found.");

        var subtotalUsd = cart.CartItems.Sum(i =>
        {
            var usd = i.VendorProduct is { } vp
                ? (vp.OverridePrice ?? vp.ReferencePriceUsd)
                : i.ProductVariant.ReferencePriceUsd;
            return usd * i.Quantity;
        });
        var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.WebsiteID == websiteId && c.Code == code, ct);
        if (coupon is null) return (false, "Coupon not found.");

        var (valid, error, _) = await _coupons.ValidateAndComputeAsync(websiteId, code, clientId, subtotalUsd, ct);
        if (!valid) return (false, error);

        cart.CouponID = coupon.CouponID;
        await TouchAsync(cartId, ct);
        return (true, null);
    }

    public async Task RemoveCouponAsync(int cartId, CancellationToken ct = default)
    {
        var cart = await _context.Carts.FirstOrDefaultAsync(c => c.CartID == cartId, ct);
        if (cart is null) return;
        cart.CouponID = null;
        await TouchAsync(cartId, ct);
    }

    private async Task TouchAsync(int cartId, CancellationToken ct)
    {
        var cart = await _context.Carts.FirstOrDefaultAsync(c => c.CartID == cartId, ct);
        if (cart is not null) cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }
}
