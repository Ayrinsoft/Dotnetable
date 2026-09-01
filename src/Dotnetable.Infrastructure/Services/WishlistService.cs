using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

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

    public async Task RemoveItemAsync(int clientId, int variantId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(i => i.Wishlist.WebsiteClientID == clientId && i.ProductVariantID == variantId, ct);
        if (item is null) return;

        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync(ct);
    }
}
