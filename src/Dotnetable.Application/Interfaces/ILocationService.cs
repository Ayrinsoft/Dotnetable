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

    // States
    Task<PagedResult<State>> GetStatesPagedAsync(int? countryId, GridQuery query, CancellationToken ct = default);
    Task<IEnumerable<State>> GetStatesByCountryAsync(int countryId, CancellationToken ct = default);
    Task<State?> GetStateByIdAsync(int id, CancellationToken ct = default);
    Task<State> CreateStateAsync(State state, CancellationToken ct = default);
    Task UpdateStateAsync(State state, CancellationToken ct = default);
    Task DeleteStateAsync(int id, CancellationToken ct = default);

    // Cities
    Task<PagedResult<City>> GetCitiesPagedAsync(int? countryId, int? stateId, GridQuery query, CancellationToken ct = default);
    Task<City?> GetCityByIdAsync(int id, CancellationToken ct = default);
    Task<City> CreateCityAsync(City city, CancellationToken ct = default);
    Task UpdateCityAsync(City city, CancellationToken ct = default);
    Task DeleteCityAsync(int id, CancellationToken ct = default);
}
