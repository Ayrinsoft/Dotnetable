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
    /// For each active shipping method, resolves the best-matching active rate by most-specific
    /// location match (city, then state, then country, else a fully-wildcard rate) whose weight
    /// range contains <paramref name="totalWeightKg"/>. Methods with no matching rate are skipped.
    /// </summary>
    Task<List<(ShippingMethod Method, decimal PriceUsd)>> GetAvailableWithPricesAsync(
        int websiteId, int? countryId, int? stateId, int? cityId, decimal totalWeightKg, CancellationToken ct = default);
}
