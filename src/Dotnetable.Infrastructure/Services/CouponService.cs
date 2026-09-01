using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class CouponService : ICouponService
{
    // Contexts come from DbLease per operation: it joins an ambient transaction when one is in
    // flight and otherwise opens a short-lived context, so nothing is shared across a circuit.
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ICurrencyConversionService _currency;

    public CouponService(IDbContextFactory<AppDbContext> contextFactory, ICurrencyConversionService currency)
    {
        _contextFactory = contextFactory;
        _currency = currency;
    }

    public async Task<PagedResult<Coupon>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default)
    {
        await using var _lease = await DbLease.OpenAsync(_contextFactory, ct);
        var _context = _lease.Context;

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

    public async Task<Coupon?> GetByIdAsync(int couponId, CancellationToken ct = default)
    {
        await using var _lease = await DbLease.OpenAsync(_contextFactory, ct);
        var _context = _lease.Context;

        return await _context.Coupons.FindAsync([couponId], ct);
    }

    public async Task<Coupon> CreateAsync(Coupon coupon, CancellationToken ct = default)
    {
        await using var _lease = await DbLease.OpenAsync(_contextFactory, ct);
        var _context = _lease.Context;

        await NormalizeCouponAmountsAsync(coupon, ct);
        coupon.CreatedAt = DateTime.UtcNow;
        coupon.TimesUsed = 0;
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync(ct);
        return coupon;
    }

    public async Task<bool> UpdateAsync(Coupon coupon, CancellationToken ct = default)
    {
        await using var _lease = await DbLease.OpenAsync(_contextFactory, ct);
        var _context = _lease.Context;

        var existing = await _context.Coupons.FirstOrDefaultAsync(c => c.CouponID == coupon.CouponID, ct);
        if (existing is null) return false;

        await NormalizeCouponAmountsAsync(coupon, ct);

        existing.Code = coupon.Code;
        existing.DiscountType = coupon.DiscountType;
        existing.DiscountValue = coupon.DiscountValue;
        existing.MinOrderAmount = coupon.MinOrderAmount;
        existing.MinOrderAmountUsd = coupon.MinOrderAmountUsd;
        existing.MaxDiscountAmount = coupon.MaxDiscountAmount;
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
        await using var _lease = await DbLease.OpenAsync(_contextFactory, ct);
        var _context = _lease.Context;

        var existing = await _context.Coupons.FindAsync([couponId], ct);
        if (existing is null) return false;
        _context.Coupons.Remove(existing);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<(bool Valid, string? Error, decimal DiscountUsd)> ValidateAndComputeAsync(
        int websiteId, string code, int? clientId, decimal cartSubtotalUsd, CancellationToken ct = default)
    {
        await using var _lease = await DbLease.OpenAsync(_contextFactory, ct);
        var _context = _lease.Context;

        var coupon = await _context.Coupons.AsNoTracking()
            .FirstOrDefaultAsync(c => c.WebsiteID == websiteId && c.Code == code, ct);

        if (coupon is null) return (false, "Coupon not found.", 0);
        if (!coupon.IsActive) return (false, "Coupon is not active.", 0);

        var now = DateTime.UtcNow;
        if (coupon.StartsAt is DateTime startsAt && now < startsAt) return (false, "Coupon is not yet valid.", 0);
        if (coupon.EndsAt is DateTime endsAt && now > endsAt) return (false, "Coupon has expired.", 0);

        // 0 = no minimum. When set, cart subtotal must be strictly greater than the threshold.
        var minOrderLocal = coupon.MinOrderAmount > 0 ? coupon.MinOrderAmount : 0m;
        var minOrderUsd = 0m;
        if (minOrderLocal > 0)
        {
            try
            {
                minOrderUsd = await _currency.ToUsdAsync(websiteId, minOrderLocal, null, ct);
            }
            catch (InvalidOperationException)
            {
                minOrderUsd = coupon.MinOrderAmountUsd > 0 ? coupon.MinOrderAmountUsd : minOrderLocal;
            }
        }
        else if (coupon.MinOrderAmountUsd > 0 && coupon.MinOrderAmount <= 0)
        {
            // Legacy rows that only populated the USD dual column.
            minOrderUsd = coupon.MinOrderAmountUsd;
        }

        if (minOrderUsd > 0 && cartSubtotalUsd <= minOrderUsd)
            return (false, "Order total must be greater than the coupon minimum order amount.", 0);

        if (coupon.UsageLimitTotal is int limitTotal && coupon.TimesUsed >= limitTotal)
            return (false, "Coupon usage limit has been reached.", 0);

        if (clientId is int cid && coupon.UsageLimitPerClient is int limitPerClient)
        {
            var usedByClient = await _context.CouponRedemptions.AsNoTracking()
                .CountAsync(r => r.CouponID == coupon.CouponID && r.WebsiteClientID == cid, ct);
            if (usedByClient >= limitPerClient)
                return (false, "You have already used this coupon the maximum number of times.", 0);
        }

        decimal discountUsd;
        if (coupon.DiscountType == DiscountTypes.FixedAmountUsd)
        {
            // DiscountValue is site-currency fixed amount (legacy constant name still FixedAmountUsd).
            var fixedUsd = coupon.DiscountValue > 0
                ? await _currency.ToUsdAsync(websiteId, coupon.DiscountValue, null, ct)
                : 0;
            discountUsd = Math.Min(fixedUsd, cartSubtotalUsd);
        }
        else
        {
            discountUsd = cartSubtotalUsd * (coupon.DiscountValue / 100m);
            var maxUsd = coupon.MaxDiscountAmountUsd;
            if ((maxUsd is null or <= 0) && coupon.MaxDiscountAmount is decimal maxLocal && maxLocal > 0)
                maxUsd = await _currency.ToUsdAsync(websiteId, maxLocal, null, ct);
            if (maxUsd is decimal cap && cap > 0)
                discountUsd = Math.Min(discountUsd, cap);
        }

        return (true, null, discountUsd);
    }

    public async Task RedeemAsync(int couponId, int orderId, int? clientId, decimal discountAmountUsd, CancellationToken ct = default)
    {
        await using var _lease = await DbLease.OpenAsync(_contextFactory, ct);
        var _context = _lease.Context;

        var coupon = await _context.Coupons.AsNoTracking().FirstOrDefaultAsync(c => c.CouponID == couponId, ct);
        decimal discountLocal = discountAmountUsd;
        if (coupon is not null)
        {
            try
            {
                var money = await _currency.ToDisplayAsync(coupon.WebsiteID, discountAmountUsd, null, ct);
                discountLocal = money.Amount;
            }
            catch (InvalidOperationException)
            {
                // keep USD as local when rates missing
            }
        }

        _context.CouponRedemptions.Add(new CouponRedemption
        {
            CouponID = couponId,
            OrderID = orderId,
            WebsiteClientID = clientId ?? 0,
            DiscountAmount = discountLocal,
            DiscountAmountUsd = discountAmountUsd,
            RedeemedAt = DateTime.UtcNow,
        });

        await _context.Coupons
            .Where(c => c.CouponID == couponId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.TimesUsed, c => c.TimesUsed + 1), ct);

        await _context.SaveChangesAsync(ct);
    }

    private async Task NormalizeCouponAmountsAsync(Coupon coupon, CancellationToken ct)
    {
        // Explicit zero means "no minimum" — clear both columns so legacy USD dual does not re-enable a floor.
        if (coupon.MinOrderAmount <= 0)
        {
            coupon.MinOrderAmount = 0;
            coupon.MinOrderAmountUsd = 0;
        }

        try
        {
            if (coupon.MinOrderAmount > 0)
                coupon.MinOrderAmountUsd = await _currency.ToUsdAsync(coupon.WebsiteID, coupon.MinOrderAmount, null, ct);

            if (coupon.MaxDiscountAmount is decimal max && max > 0)
                coupon.MaxDiscountAmountUsd = await _currency.ToUsdAsync(coupon.WebsiteID, max, null, ct);
            else if (coupon.MaxDiscountAmount is null or <= 0)
            {
                coupon.MaxDiscountAmount = null;
                coupon.MaxDiscountAmountUsd = null;
            }
            else if (coupon.MaxDiscountAmountUsd is decimal maxUsd && maxUsd > 0 && (coupon.MaxDiscountAmount is null or <= 0))
            {
                var m = await _currency.ToDisplayAsync(coupon.WebsiteID, maxUsd, null, ct);
                coupon.MaxDiscountAmount = m.Amount;
            }
        }
        catch (InvalidOperationException)
        {
            // No rate: keep dual columns equal so fixed-amount sites still work.
            if (coupon.MinOrderAmount > 0 && coupon.MinOrderAmountUsd <= 0)
                coupon.MinOrderAmountUsd = coupon.MinOrderAmount;
            if (coupon.MaxDiscountAmount is null && coupon.MaxDiscountAmountUsd is decimal u) coupon.MaxDiscountAmount = u;
            if (coupon.MaxDiscountAmountUsd is null && coupon.MaxDiscountAmount is decimal l) coupon.MaxDiscountAmountUsd = l;
        }
    }
}
