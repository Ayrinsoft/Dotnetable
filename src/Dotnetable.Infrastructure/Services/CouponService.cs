using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class CouponService : ICouponService
{
    private readonly AppDbContext _context;

    public CouponService(AppDbContext context) => _context = context;

    public async Task<PagedResult<Coupon>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
        var q = _context.Coupons.AsNoTracking().Where(c => c.WebsiteID == websiteId);

        if (query.GetSearch(nameof(Coupon.Code)) is string code)
            q = q.Where(c => c.Code.Contains(code));
        if (query.GetSearch(nameof(Coupon.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(c => c.IsActive == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Coupon.CreatedAt) + " DESC")
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Coupon> { Items = items, TotalCount = total };
    }

    public async Task<Coupon?> GetByIdAsync(int couponId, CancellationToken ct = default) =>
        await _context.Coupons.FindAsync([couponId], ct);

    public async Task<Coupon> CreateAsync(Coupon coupon, CancellationToken ct = default)
    {
        coupon.CreatedAt = DateTime.UtcNow;
        coupon.TimesUsed = 0;
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync(ct);
        return coupon;
    }

    public async Task<bool> UpdateAsync(Coupon coupon, CancellationToken ct = default)
    {
        var existing = await _context.Coupons.FirstOrDefaultAsync(c => c.CouponID == coupon.CouponID, ct);
        if (existing is null) return false;

        existing.Code = coupon.Code;
        existing.DiscountType = coupon.DiscountType;
        existing.DiscountValue = coupon.DiscountValue;
        existing.MinOrderAmountUsd = coupon.MinOrderAmountUsd;
        existing.MaxDiscountAmountUsd = coupon.MaxDiscountAmountUsd;
        existing.UsageLimitTotal = coupon.UsageLimitTotal;
        existing.UsageLimitPerClient = coupon.UsageLimitPerClient;
        existing.StartsAt = coupon.StartsAt;
        existing.EndsAt = coupon.EndsAt;
        existing.IsActive = coupon.IsActive;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int couponId, CancellationToken ct = default)
    {
        var existing = await _context.Coupons.FindAsync([couponId], ct);
        if (existing is null) return false;
        _context.Coupons.Remove(existing);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<(bool Valid, string? Error, decimal DiscountUsd)> ValidateAndComputeAsync(
        int websiteId, string code, int? clientId, decimal cartSubtotalUsd, CancellationToken ct = default)
    {
        var coupon = await _context.Coupons.AsNoTracking()
            .FirstOrDefaultAsync(c => c.WebsiteID == websiteId && c.Code == code, ct);

        if (coupon is null) return (false, "Coupon not found.", 0);
        if (!coupon.IsActive) return (false, "Coupon is not active.", 0);

        var now = DateTime.UtcNow;
        if (coupon.StartsAt is DateTime startsAt && now < startsAt) return (false, "Coupon is not yet valid.", 0);
        if (coupon.EndsAt is DateTime endsAt && now > endsAt) return (false, "Coupon has expired.", 0);

        if (cartSubtotalUsd < coupon.MinOrderAmountUsd)
            return (false, $"Order must be at least {coupon.MinOrderAmountUsd:0.00} USD to use this coupon.", 0);

        if (coupon.UsageLimitTotal is int limitTotal && coupon.TimesUsed >= limitTotal)
            return (false, "Coupon usage limit has been reached.", 0);

        if (clientId is int cid && coupon.UsageLimitPerClient is int limitPerClient)
        {
            var usedByClient = await _context.CouponRedemptions.AsNoTracking()
                .CountAsync(r => r.CouponID == coupon.CouponID && r.WebsiteClientID == cid, ct);
            if (usedByClient >= limitPerClient)
                return (false, "You have already used this coupon the maximum number of times.", 0);
        }

        var discount = coupon.DiscountType == DiscountTypes.FixedAmountUsd
            ? Math.Min(coupon.DiscountValue, cartSubtotalUsd)
            : cartSubtotalUsd * (coupon.DiscountValue / 100m);

        if (coupon.DiscountType == DiscountTypes.Percent && coupon.MaxDiscountAmountUsd is decimal maxDiscount)
            discount = Math.Min(discount, maxDiscount);

        return (true, null, discount);
    }

    public async Task RedeemAsync(int couponId, int orderId, int? clientId, decimal discountAmountUsd, CancellationToken ct = default)
    {
        // Note: CouponRedemption.WebsiteClientID is a required FK in the current schema, so a guest
        // (clientId == null) redemption cannot be attributed to a WebsiteClient row. Callers building
        // guest checkout should ensure a clientId is available by the time RedeemAsync runs (e.g. a
        // guest WebsiteClient shell row), or this schema gap should be revisited in a later phase.
        _context.CouponRedemptions.Add(new CouponRedemption
        {
            CouponID = couponId,
            OrderID = orderId,
            WebsiteClientID = clientId ?? 0,
            DiscountAmountUsd = discountAmountUsd,
            RedeemedAt = DateTime.UtcNow,
        });

        await _context.Coupons
            .Where(c => c.CouponID == couponId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.TimesUsed, c => c.TimesUsed + 1), ct);

        await _context.SaveChangesAsync(ct);
    }
}
