using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ProductReviewService : IProductReviewService
{
    private readonly AppDbContext _context;
    private readonly IOrderService _orders;

    public ProductReviewService(AppDbContext context, IOrderService orders)
    {
        _context = context;
        _orders = orders;
    }

    public async Task<ProductReview> SubmitAsync(
        int websiteId, int clientId, int productId, int? variantId, byte rating,
        string? title, string body, string? prosJson, string? consJson, CancellationToken ct = default)
    {
        var isVerified = await _orders.ClientHasPaidOrderForProductAsync(clientId, productId, ct);

        var review = new ProductReview
        {
            WebsiteID = websiteId,
            ProductID = productId,
            ProductVariantID = variantId,
            WebsiteClientID = clientId,
            Rating = rating,
            Title = title,
            Body = body,
            ProsJson = prosJson,
            ConsJson = consJson,
            IsVerifiedPurchase = isVerified,
            Status = (byte)ModerationStatus.Pending,
            Approved = false,
            CreatedAt = DateTime.UtcNow,
        };
        _context.ProductReviews.Add(review);
        await _context.SaveChangesAsync(ct);
        return review;
    }

    public async Task<PagedResult<ProductReview>> GetApprovedAsync(int productId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.ProductReviews.AsNoTracking()
            .Include(r => r.WebsiteClient)
            .Where(r => r.ProductID == productId && r.Approved);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(r => r.CreatedAt).Skip(query.Skip).Take(query.Take).ToListAsync(ct);
        return new PagedResult<ProductReview> { Items = items, TotalCount = total };
    }

    public async Task<PagedResult<ProductReview>> GetPendingAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.ProductReviews.AsNoTracking()
            .Include(r => r.Product).Include(r => r.WebsiteClient)
            .Where(r => r.Status == (byte)ModerationStatus.Pending);
        if (websiteId is int wid) q = q.Where(r => r.WebsiteID == wid);

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(r => r.CreatedAt).Skip(query.Skip).Take(query.Take).ToListAsync(ct);
        return new PagedResult<ProductReview> { Items = items, TotalCount = total };
    }

    public async Task<bool> ModerateAsync(int reviewId, bool approve, CancellationToken ct = default)
    {
        var review = await _context.ProductReviews.FirstOrDefaultAsync(r => r.ProductReviewID == reviewId, ct);
        if (review is null) return false;

        review.Approved = approve;
        review.Status = (byte)(approve ? ModerationStatus.Approved : ModerationStatus.Rejected);
        await _context.SaveChangesAsync(ct);

        await RecomputeRatingAsync(review.ProductID, ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int reviewId, CancellationToken ct = default)
    {
        var review = await _context.ProductReviews.FirstOrDefaultAsync(r => r.ProductReviewID == reviewId, ct);
        if (review is null) return false;

        var productId = review.ProductID;
        _context.ProductReviews.Remove(review);
        await _context.SaveChangesAsync(ct);

        await RecomputeRatingAsync(productId, ct);
        return true;
    }

    public async Task LikeAsync(int reviewId, CancellationToken ct = default) =>
        await _context.ProductReviews.Where(r => r.ProductReviewID == reviewId)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.LikeCount, r => r.LikeCount + 1), ct);

    public async Task DislikeAsync(int reviewId, CancellationToken ct = default) =>
        await _context.ProductReviews.Where(r => r.ProductReviewID == reviewId)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.DislikeCount, r => r.DislikeCount + 1), ct);

    private async Task RecomputeRatingAsync(int productId, CancellationToken ct)
    {
        var approved = await _context.ProductReviews.AsNoTracking()
            .Where(r => r.ProductID == productId && r.Approved)
            .Select(r => (int)r.Rating)
            .ToListAsync(ct);

        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == productId, ct);
        if (product is null) return;

        product.RatingCount = approved.Count;
        product.AvgRating = approved.Count == 0 ? 0 : Math.Round((decimal)approved.Average(), 2);
        await _context.SaveChangesAsync(ct);
    }
}
