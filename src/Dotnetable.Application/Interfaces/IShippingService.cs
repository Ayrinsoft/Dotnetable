using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Admin CRUD for <see cref="ShippingMethod"/>/<see cref="ShippingRate"/>, plus storefront price resolution.</summary>
public interface IShippingService
{
    // Methods
    Task<List<ShippingMethod>> GetAllAsync(int websiteId, CancellationToken ct = default);
    Task<PagedResult<ShippingMethod>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default);
    Task<ShippingMethod?> GetByIdAsync(int shippingMethodId, CancellationToken ct = default);
    Task<ShippingMethod> CreateAsync(ShippingMethod method, CancellationToken ct = default);
    Task<bool> UpdateAsync(ShippingMethod method, CancellationToken ct = default);
    Task<bool> DeleteAsync(int shippingMethodId, CancellationToken ct = default);

    // Rates (scoped to a method)
    Task<List<ShippingRate>> GetRatesAsync(int shippingMethodId, CancellationToken ct = default);
    Task<PagedResult<ShippingRate>> GetRatesPagedAsync(int shippingMethodId, GridQuery query, CancellationToken ct = default);
    Task<ShippingRate> CreateRateAsync(ShippingRate rate, CancellationToken ct = default);
    Task<bool> UpdateRateAsync(ShippingRate rate, CancellationToken ct = default);
    Task<bool> DeleteRateAsync(int shippingRateId, CancellationToken ct = default);

    /// <summary>
    /// For each active shipping method that supports at least one payment mode, resolves the
    /// best-matching active rate by most-specific location match (city → state → country → wildcard)
    /// whose weight range contains <paramref name="totalWeightKg"/>. Zone rate is floored by the
    /// method's prepaid/COD minimums. Methods with neither prepaid nor COD available are skipped.
    /// </summary>
    Task<List<ShippingQuoteDto>> GetAvailableWithPricesAsync(
        int websiteId, int? countryId, int? stateId, int? cityId, decimal totalWeightKg, CancellationToken ct = default);
}
