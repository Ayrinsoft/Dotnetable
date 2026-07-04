using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Manages the global <see cref="Currency"/> master list (mirrors Country/State/City: editable only
/// from the master website, shared by every tenant). Per-website exchange rates live in
/// <see cref="ICurrencyConversionService"/> / <see cref="Domain.Entities.CurrencyRate"/> instead.
/// </summary>
public interface ICurrencyService
{
    Task<List<Currency>> GetAllAsync(CancellationToken ct = default);

    Task<Currency?> GetByCodeAsync(string currencyCode, CancellationToken ct = default);

    Task CreateAsync(Currency currency, CancellationToken ct = default);

    Task<bool> UpdateAsync(Currency currency, CancellationToken ct = default);

    Task<bool> DeleteAsync(string currencyCode, CancellationToken ct = default);
}
