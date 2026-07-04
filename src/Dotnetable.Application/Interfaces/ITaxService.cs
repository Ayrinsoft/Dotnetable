using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>Admin CRUD for <see cref="TaxRate"/>, plus stacked tax computation for checkout.</summary>
public interface ITaxService
{
    Task<List<TaxRate>> GetAllAsync(int websiteId, CancellationToken ct = default);
    Task<TaxRate?> GetByIdAsync(int taxRateId, CancellationToken ct = default);
    Task<TaxRate> CreateAsync(TaxRate rate, CancellationToken ct = default);
    Task<bool> UpdateAsync(TaxRate rate, CancellationToken ct = default);
    Task<bool> DeleteAsync(int taxRateId, CancellationToken ct = default);

    /// <summary>
    /// Sums subtotalUsd * Rate over every active <see cref="TaxRate"/> whose CountryID/StateID is
    /// either null (applies everywhere) or matches the given location. Matching rates stack —
    /// they are not mutually exclusive. Priority is informational only and does not affect the sum.
    /// </summary>
    Task<decimal> ComputeTaxAsync(int websiteId, int? countryId, int? stateId, decimal subtotalUsd, CancellationToken ct = default);
}
