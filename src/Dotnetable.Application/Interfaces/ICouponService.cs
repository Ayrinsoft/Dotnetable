using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Values for <see cref="Coupon.DiscountType"/> (stored as a TINYINT — no dedicated enum exists in
/// <c>Dotnetable.Domain.Enums</c>, so these constants mirror it here).
/// </summary>
public static class DiscountTypes
{
    /// <summary>DiscountValue is a percentage (0-100) of the cart subtotal, optionally capped by MaxDiscountAmount.</summary>
    public const byte Percent = 0;

    /// <summary>DiscountValue is a flat amount in site currency (legacy constant name FixedAmountUsd), capped at the cart subtotal.</summary>
    public const byte FixedAmountUsd = 1;
}

/// <summary>Admin CRUD plus validation/redemption for <see cref="Coupon"/> discount codes.</summary>
public interface ICouponService
{
    Task<PagedResult<Coupon>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default);
    Task<Coupon?> GetByIdAsync(int couponId, CancellationToken ct = default);
    Task<Coupon> CreateAsync(Coupon coupon, CancellationToken ct = default);
    Task<bool> UpdateAsync(Coupon coupon, CancellationToken ct = default);
    Task<bool> DeleteAsync(int couponId, CancellationToken ct = default);

    /// <summary>
    /// Validates a coupon code against a cart and computes the discount it would yield, without
    /// recording a redemption. Checks active flag, date window, minimum order amount, total and
    /// per-client usage limits, and computes the discount (percent capped by MaxDiscountAmountUsd,
    /// or a flat amount capped at the cart subtotal).
    /// </summary>
    Task<(bool Valid, string? Error, decimal DiscountUsd)> ValidateAndComputeAsync(
        int websiteId, string code, int? clientId, decimal cartSubtotalUsd, CancellationToken ct = default);

    /// <summary>Records a redemption (called at order-commit time) and increments the coupon's usage count.</summary>
    Task RedeemAsync(int couponId, int orderId, int? clientId, decimal discountAmountUsd, CancellationToken ct = default);
}
