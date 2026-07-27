using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Admin CRUD for <see cref="TaxRate"/>, plus tax computation driven by each website's tax settings.</summary>
public interface ITaxService
{
    Task<List<TaxRate>> GetAllAsync(int websiteId, CancellationToken ct = default);
    Task<PagedResult<TaxRate>> GetPagedAsync(int websiteId, GridQuery query, CancellationToken ct = default);
    Task<TaxRate?> GetByIdAsync(int taxRateId, CancellationToken ct = default);
    Task<TaxRate> CreateAsync(TaxRate rate, CancellationToken ct = default);
    Task<bool> UpdateAsync(TaxRate rate, CancellationToken ct = default);
    Task<bool> DeleteAsync(int taxRateId, CancellationToken ct = default);

    /// <summary>
    /// Computes tax for <paramref name="websiteId"/> using that site's TaxEnabled / PricesIncludeTax / TaxOnShipping
    /// and active matching rates (country/state). Matching rates stack.
    /// When PricesIncludeTax, tax is extracted from the taxable base rather than added on top.
    /// </summary>
    Task<TaxComputationResult> ComputeTaxDetailedAsync(
        int websiteId,
        int? countryId,
        int? stateId,
        decimal merchandiseSubtotal,
        decimal shippingAmount = 0,
        CancellationToken ct = default);

    /// <summary>Convenience wrapper: tax amount only (merchandise base, no shipping).</summary>
    Task<decimal> ComputeTaxAsync(int websiteId, int? countryId, int? stateId, decimal subtotalUsd, CancellationToken ct = default);
}
