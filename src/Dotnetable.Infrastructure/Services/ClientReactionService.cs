using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ClientReactionService : IClientReactionService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public ClientReactionService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<ReactionStateDto?> GetStateAsync(int websiteId, ReactionTargetType targetType, int targetId, int? clientId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        if (!await ReactionStore.IsPublishedAsync(_context, websiteId, targetType, targetId, ct)) return null;
        return await ReactionStore.StateAsync(_context, targetType, targetId, clientId, ct);
    }

    public async Task<ReactionStateDto?> SetAsync(int websiteId, int clientId, ReactionTargetType targetType, int targetId,
        ReactionType reaction, bool on, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        if (!await ReactionStore.IsPublishedAsync(_context, websiteId, targetType, targetId, ct)) return null;

        // A product favorite *is* the "save for later" list: keep the wishlist in step with it.
        var isFavorite = targetType == ReactionTargetType.Product && reaction == ReactionType.Like;
        if (on)
        {
            if (isFavorite)
                await WishlistService.EnsureProductAsync(_context, websiteId, clientId, targetId, ct);
            await ReactionStore.AddAsync(_context, websiteId, clientId, targetType, targetId, reaction, ct);
        }
        else
        {
            if (isFavorite)
                await _context.WishlistItems
                    .Where(i => i.Wishlist.WebsiteClientID == clientId && i.ProductVariant.ProductID == targetId)
                    .ExecuteDeleteAsync(ct);
            await ReactionStore.RemoveAsync(_context, clientId, targetType, targetId, reaction, ct);
        }

        return await ReactionStore.StateAsync(_context, targetType, targetId, clientId, ct);
    }

    public async Task<PagedResult<ClientReactionItemDto>> GetListAsync(int websiteId, int clientId, ReactionType reaction,
        ReactionTargetType? targetType, int page, int pageSize, string? languageCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var type = (byte?)targetType;
        var rows = await _context.ClientReactions.AsNoTracking()
            .Where(r => r.WebsiteID == websiteId && r.WebsiteClientID == clientId && r.ReactionType == (byte)reaction
                        && (type == null || r.TargetType == type))
            .OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.ClientReactionID)
            .Select(r => new { r.TargetType, r.TargetID, r.CreatedAt })
            .ToListAsync(ct);

        IdsOf(ReactionTargetType.Product, out var productIds);
        IdsOf(ReactionTargetType.Post, out var postIds);
        IdsOf(ReactionTargetType.Page, out var pageIds);

        var now = DateTime.UtcNow;
        var products = productIds.Count == 0 ? new() : await _context.Products.AsNoTracking()
            .Where(p => productIds.Contains(p.ProductID) && p.WebsiteID == websiteId && p.IsActive && p.Status == ProductService.PublishedStatus)
            .Include(p => p.FeaturedImageFile).Include(p => p.ProductTranslations)
            .ToDictionaryAsync(p => p.ProductID, ct);
        var posts = postIds.Count == 0 ? new() : await _context.Posts.AsNoTracking()
            .Where(p => postIds.Contains(p.PostID) && p.WebsiteID == websiteId && p.IsActive && p.Status == PostService.PublishedStatus
                        && (p.PublishedAt == null || p.PublishedAt <= now))
            .Include(p => p.FeaturedImageFile).Include(p => p.PostTranslations)
            .ToDictionaryAsync(p => p.PostID, ct);
        var pages = pageIds.Count == 0 ? new() : await _context.Pages.AsNoTracking()
            .Where(p => pageIds.Contains(p.PageID) && p.WebsiteID == websiteId && p.IsActive && p.Status == ReactionStore.PagePublishedStatus)
            .Include(p => p.PageTranslations)
            .ToDictionaryAsync(p => p.PageID, ct);

        var items = new List<ClientReactionItemDto>();
        foreach (var r in rows)
        {
            ClientReactionItemDto? item = (ReactionTargetType)r.TargetType switch
            {
                ReactionTargetType.Product when products.TryGetValue(r.TargetID, out var p) => FromProduct(p),
                ReactionTargetType.Post when posts.TryGetValue(r.TargetID, out var p) => FromPost(p),
                ReactionTargetType.Page when pages.TryGetValue(r.TargetID, out var p) => FromPage(p),
                _ => null,
            };
            if (item is not null) items.Add(item);

            ClientReactionItemDto FromProduct(Product p)
            {
                var t = Translation(p.ProductTranslations, x => x.LanguageCode);
                return Item(ReactionTargetType.Product, p.ProductID, Pick(t?.Title, p.Title), Pick(t?.Slug, p.Slug),
                    p.FeaturedImageFile?.ThumbnailCDN ?? p.FeaturedImageFile?.CNDUrl, Pick(t?.ShortDescription, p.ShortDescription));
            }
            ClientReactionItemDto FromPost(Post p)
            {
                var t = Translation(p.PostTranslations, x => x.LanguageCode);
                return Item(ReactionTargetType.Post, p.PostID, Pick(t?.Title, p.Title), Pick(t?.Slug, p.Slug),
                    p.FeaturedImageFile?.ThumbnailCDN ?? p.FeaturedImageFile?.CNDUrl, Pick(t?.Excerpt, p.Excerpt));
            }
            ClientReactionItemDto FromPage(Page p)
            {
                var t = Translation(p.PageTranslations, x => x.LanguageCode);
                return Item(ReactionTargetType.Page, p.PageID, Pick(t?.Title, p.Title), Pick(t?.Slug, p.Slug), null, null);
            }
            ClientReactionItemDto Item(ReactionTargetType tt, int id, string? title, string? slug, string? image, string? excerpt) => new()
            {
                TargetType = ReactionTargets.Name(tt), TargetId = id, Title = title ?? "", Slug = slug ?? "",
                ImageUrl = image, Excerpt = excerpt, CreatedAt = r.CreatedAt,
            };
        }

        var take = pageSize < 1 ? 20 : pageSize;
        var skip = (page < 1 ? 0 : page - 1) * take;
        return new PagedResult<ClientReactionItemDto> { Items = items.Skip(skip).Take(take).ToList(), TotalCount = items.Count };

        void IdsOf(ReactionTargetType t, out List<int> ids) =>
            ids = rows.Where(r => r.TargetType == (byte)t).Select(r => r.TargetID).Distinct().ToList();

        T? Translation<T>(IEnumerable<T> all, Func<T, string> code) where T : class =>
            string.IsNullOrWhiteSpace(languageCode) ? null
                : all.FirstOrDefault(x => string.Equals(code(x).Trim(), languageCode, StringComparison.OrdinalIgnoreCase));
    }

    private static string? Pick(string? translated, string? fallback) =>
        string.IsNullOrWhiteSpace(translated) ? fallback : translated;
}

/// <summary>
/// Reaction rows and their counters. Shared by <see cref="ClientReactionService"/> and
/// <see cref="WishlistService"/>; every method takes the caller's context.
///
/// <para>A like moves its counter only when a row was really inserted or deleted, and the counter is
/// changed with a single atomic <c>UPDATE … SET n = n ± 1</c>. The unique index on
/// (client, target, reaction) makes a double-click or a racing second request a no-op instead of a
/// second count — do not replace this with read-modify-write.</para>
/// </summary>
internal static class ReactionStore
{
    public const byte PagePublishedStatus = 1;

    public static async Task<bool> IsPublishedAsync(AppDbContext _context, int websiteId, ReactionTargetType type, int id, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        return type switch
        {
            ReactionTargetType.Product => await _context.Products.AnyAsync(p =>
                p.ProductID == id && p.WebsiteID == websiteId && p.IsActive && p.Status == ProductService.PublishedStatus, ct),
            ReactionTargetType.Post => await _context.Posts.AnyAsync(p =>
                p.PostID == id && p.WebsiteID == websiteId && p.IsActive && p.Status == PostService.PublishedStatus
                && (p.PublishedAt == null || p.PublishedAt <= now), ct),
            ReactionTargetType.Page => await _context.Pages.AnyAsync(p =>
                p.PageID == id && p.WebsiteID == websiteId && p.IsActive && p.Status == PagePublishedStatus, ct),
            _ => false,
        };
    }

    public static async Task<ReactionStateDto> StateAsync(AppDbContext _context, ReactionTargetType type, int id, int? clientId, CancellationToken ct)
    {
        var count = type switch
        {
            ReactionTargetType.Product => await _context.Products.Where(p => p.ProductID == id).Select(p => p.FavoriteCount).FirstOrDefaultAsync(ct),
            ReactionTargetType.Post => await _context.Posts.Where(p => p.PostID == id).Select(p => p.LikeCount).FirstOrDefaultAsync(ct),
            ReactionTargetType.Page => await _context.Pages.Where(p => p.PageID == id).Select(p => p.LikeCount).FirstOrDefaultAsync(ct),
            _ => 0,
        };

        var mine = clientId is int cid
            ? await _context.ClientReactions.AsNoTracking()
                .Where(r => r.WebsiteClientID == cid && r.TargetType == (byte)type && r.TargetID == id)
                .Select(r => r.ReactionType).ToListAsync(ct)
            : new List<byte>();

        return new ReactionStateDto
        {
            TargetType = ReactionTargets.Name(type),
            TargetId = id,
            LikeCount = Math.Max(0, count),
            Liked = mine.Contains((byte)ReactionType.Like),
            Bookmarked = mine.Contains((byte)ReactionType.Bookmark),
        };
    }

    /// <summary>Inserts the reaction unless it exists; bumps the counter only when a row was inserted.</summary>
    public static async Task<bool> AddAsync(AppDbContext _context, int websiteId, int clientId, ReactionTargetType type, int id,
        ReactionType reaction, CancellationToken ct)
    {
        if (await _context.ClientReactions.AnyAsync(r =>
                r.WebsiteClientID == clientId && r.TargetType == (byte)type && r.TargetID == id && r.ReactionType == (byte)reaction, ct))
            return false;

        var row = new ClientReaction
        {
            WebsiteID = websiteId, WebsiteClientID = clientId, TargetType = (byte)type, TargetID = id,
            ReactionType = (byte)reaction, CreatedAt = DateTime.UtcNow,
        };
        _context.ClientReactions.Add(row);
        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // A concurrent request inserted the same row first: the unique index rejected ours.
            _context.Entry(row).State = EntityState.Detached;
            return false;
        }

        if (reaction == ReactionType.Like)
            await AdjustCounterAsync(_context, type, id, +1, ct);
        return true;
    }

    /// <summary>Deletes the reaction; lowers the counter only when a row was deleted.</summary>
    public static async Task<bool> RemoveAsync(AppDbContext _context, int clientId, ReactionTargetType type, int id,
        ReactionType reaction, CancellationToken ct)
    {
        var deleted = await _context.ClientReactions
            .Where(r => r.WebsiteClientID == clientId && r.TargetType == (byte)type && r.TargetID == id && r.ReactionType == (byte)reaction)
            .ExecuteDeleteAsync(ct);
        if (deleted == 0) return false;

        if (reaction == ReactionType.Like)
            await AdjustCounterAsync(_context, type, id, -deleted, ct);
        return true;
    }

    /// <summary>Loads every reaction on a target so a delete path can remove them in its own SaveChanges.</summary>
    public static Task<List<ClientReaction>> ForTargetAsync(AppDbContext _context, ReactionTargetType type, int id, CancellationToken ct) =>
        _context.ClientReactions.Where(r => r.TargetType == (byte)type && r.TargetID == id).ToListAsync(ct);

    private static Task<int> AdjustCounterAsync(AppDbContext _context, ReactionTargetType type, int id, int delta, CancellationToken ct) =>
        type switch
        {
            // The "> 0" guard on a decrement keeps a counter that drifted (e.g. hand-edited data) from going negative.
            ReactionTargetType.Product => _context.Products
                .Where(p => p.ProductID == id && (delta > 0 || p.FavoriteCount > 0))
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.FavoriteCount, p => p.FavoriteCount + delta), ct),
            ReactionTargetType.Post => _context.Posts
                .Where(p => p.PostID == id && (delta > 0 || p.LikeCount > 0))
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.LikeCount, p => p.LikeCount + delta), ct),
            ReactionTargetType.Page => _context.Pages
                .Where(p => p.PageID == id && (delta > 0 || p.LikeCount > 0))
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.LikeCount, p => p.LikeCount + delta), ct),
            _ => Task.FromResult(0),
        };
}
