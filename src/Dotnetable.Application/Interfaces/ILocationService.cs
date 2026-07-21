using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface ILocationService
{
    // Countries
    Task<PagedResult<Country>> GetCountriesPagedAsync(GridQuery query, CancellationToken ct = default);
    Task<IEnumerable<Country>> GetAllCountriesAsync(CancellationToken ct = default);
    Task<Country?> GetCountryByIdAsync(int id, CancellationToken ct = default);
    Task<Country> CreateCountryAsync(Country country, CancellationToken ct = default);
    Task UpdateCountryAsync(Country country, CancellationToken ct = default);
    Task DeleteCountryAsync(int id, CancellationToken ct = default);
    Task<bool> CountryExistsAsync(string countryCode, string title, int? excludeId = null, CancellationToken ct = default);
    byte[] GetCountryImportSample();
    Task<LocationImportResult> ImportCountriesAsync(Stream csv, CancellationToken ct = default);

    // States
    Task<PagedResult<State>> GetStatesPagedAsync(int? countryId, GridQuery query, CancellationToken ct = default);
    Task<IEnumerable<State>> GetStatesByCountryAsync(int countryId, CancellationToken ct = default);
    Task<State?> GetStateByIdAsync(int id, CancellationToken ct = default);
    Task<State> CreateStateAsync(State state, CancellationToken ct = default);
    Task UpdateStateAsync(State state, CancellationToken ct = default);
    Task DeleteStateAsync(int id, CancellationToken ct = default);
    Task<bool> StateExistsAsync(int countryId, string title, int? excludeId = null, CancellationToken ct = default);
    byte[] GetStateImportSample();
    Task<LocationImportResult> ImportStatesAsync(Stream csv, CancellationToken ct = default);

    // Cities
    /// <summary>All active cities for a country, unpaged — for populating a dependent dropdown.</summary>
    Task<IEnumerable<City>> GetCitiesByCountryAsync(int countryId, CancellationToken ct = default);
    Task<PagedResult<City>> GetCitiesPagedAsync(int? countryId, int? stateId, GridQuery query, CancellationToken ct = default);
    Task<City?> GetCityByIdAsync(int id, CancellationToken ct = default);
    Task<City> CreateCityAsync(City city, CancellationToken ct = default);
    Task UpdateCityAsync(City city, CancellationToken ct = default);
    Task DeleteCityAsync(int id, CancellationToken ct = default);
    /// <summary>
    /// True when another city already has the same title within the same country.
    /// Same title in a different country is allowed.
    /// </summary>
    Task<bool> CityExistsAsync(int countryId, string title, int? excludeId = null, CancellationToken ct = default);
    byte[] GetCityImportSample();
    Task<LocationImportResult> ImportCitiesAsync(Stream csv, CancellationToken ct = default);
}
