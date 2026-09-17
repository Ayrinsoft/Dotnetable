using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// The customer's "save for later" list. A product with at least one variant on the list is the
/// customer's favorite: adding the first variant records the favorite (and counts it once in
/// <see cref="Product.FavoriteCount"/>), removing the last one withdraws it.
/// </summary>
public class WishlistService : IWishlistService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public WishlistService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<List<WishlistItem>> GetItemsAsync(int clientId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.WishlistItems.AsNoTracking()
            .Include(i => i.ProductVariant).ThenInclude(v => v.Product)
            .Include(i => i.ProductVariant).ThenInclude(v => v.ImageFile)
            .Where(i => i.Wishlist.WebsiteClientID == clientId)
            .OrderByDescending(i => i.AddedAt)
            .ToListAsync(ct);
    }

    public async Task AddItemAsync(int websiteId, int clientId, int variantId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var productId = await _context.ProductVariants
            .Where(v => v.ProductVariantID == variantId && v.Product.WebsiteID == websiteId)
            .Select(v => (int?)v.ProductID)
            .FirstOrDefaultAsync(ct);
        if (productId is null) return;

        await AddVariantAsync(_context, websiteId, clientId, variantId, ct);
        await ReactionStore.AddAsync(_context, websiteId, clientId, ReactionTargetType.Product, productId.Value, ReactionType.Like, ct);
    }

    public async Task RemoveItemAsync(int clientId, int variantId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var item = await _context.WishlistItems
            .Include(i => i.ProductVariant)
            .FirstOrDefaultAsync(i => i.Wishlist.WebsiteClientID == clientId && i.ProductVariantID == variantId, ct);
        if (item is null) return;

        var productId = item.ProductVariant.ProductID;
        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync(ct);

        var stillSaved = await _context.WishlistItems.AnyAsync(i =>
            i.Wishlist.WebsiteClientID == clientId && i.ProductVariant.ProductID == productId, ct);
        if (!stillSaved)
            await ReactionStore.RemoveAsync(_context, clientId, ReactionTargetType.Product, productId, ReactionType.Like, ct);
    }

    /// <summary>Makes sure the product is on the client's wishlist, adding its default (or first active)
    /// variant when none of its variants is there yet.</summary>
    internal static async Task EnsureProductAsync(AppDbContext _context, int websiteId, int clientId, int productId, CancellationToken ct)
    {
        var saved = await _context.WishlistItems.AnyAsync(i =>
            i.Wishlist.WebsiteClientID == clientId && i.ProductVariant.ProductID == productId, ct);
        if (saved) return;

        var variantId = await _context.ProductVariants
            .Where(v => v.ProductID == productId && v.IsActive)
            .OrderByDescending(v => v.IsDefault).ThenBy(v => v.ProductVariantID)
            .Select(v => (int?)v.ProductVariantID)
            .FirstOrDefaultAsync(ct);
        if (variantId is int id)
            await AddVariantAsync(_context, websiteId, clientId, id, ct);
    }

    private static async Task AddVariantAsync(AppDbContext _context, int websiteId, int clientId, int variantId, CancellationToken ct)
    {
        var wishlist = await _context.Wishlists.FirstOrDefaultAsync(w => w.WebsiteClientID == clientId, ct);
        if (wishlist is null)
        {
            wishlist = new Wishlist { WebsiteID = websiteId, WebsiteClientID = clientId, CreatedAt = DateTime.UtcNow };
            _context.Wishlists.Add(wishlist);
            await _context.SaveChangesAsync(ct);
        }

        var exists = await _context.WishlistItems.AnyAsync(i => i.WishlistID == wishlist.WishlistID && i.ProductVariantID == variantId, ct);
        if (exists) return;

        _context.WishlistItems.Add(new WishlistItem { WishlistID = wishlist.WishlistID, ProductVariantID = variantId, AddedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync(ct);
    }
}
