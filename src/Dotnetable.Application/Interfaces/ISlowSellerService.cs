using Dotnetable.Application.DTOs;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Lists published products that sold few units in the last <see cref="WindowDays"/> days so an admin
/// can decide a discount. The service never writes a price, coupon, or compare-at value.
/// </summary>
public interface ISlowSellerService
{
    public const int WindowDays = 30;
    public const int DefaultMaxUnitsSold = 2;

    /// <summary>
    /// Active published products of <paramref name="websiteId"/> whose sold units in the window are
    /// at most <paramref name="maxUnitsSold"/>. Pending, cancelled, and refunded orders are ignored.
    /// Sales are dated by <c>PaidAt</c>, or by <c>CreatedAt</c> when the order has no paid time.
    /// </summary>
    Task<SlowSellerReport> GetAsync(int websiteId, int maxUnitsSold, string? search, GridQuery query, CancellationToken ct = default);
}
