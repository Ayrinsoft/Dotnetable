using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class LocationService : ILocationService
{
    private readonly AppDbContext _context;

    public LocationService(AppDbContext context) => _context = context;

    // ── Countries ────────────────────────────────────────────────────

    public async Task<PagedResult<Country>> GetCountriesPagedAsync(GridQuery query, CancellationToken ct = default)
    {
        var q = _context.Countries.AsNoTracking();

        if (query.GetSearch(nameof(Country.Title)) is string title)
            q = q.Where(c => c.Title.Contains(title));
        if (query.GetSearch(nameof(Country.CountryCode)) is string code)
            q = q.Where(c => c.CountryCode.Contains(code));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Country.CountryID))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Country> { Items = items, TotalCount = total };
    }

    public async Task<IEnumerable<Country>> GetAllCountriesAsync(CancellationToken ct = default) =>
        await _context.Countries.AsNoTracking().OrderBy(c => c.Title).ToListAsync(ct);

    public async Task<Country?> GetCountryByIdAsync(int id, CancellationToken ct = default) =>
        await _context.Countries.FindAsync([id], ct);

    public async Task<Country> CreateCountryAsync(Country country, CancellationToken ct = default)
    {
        _context.Countries.Add(country);
        await _context.SaveChangesAsync(ct);
        return country;
    }

    public async Task UpdateCountryAsync(Country country, CancellationToken ct = default)
    {
        _context.Countries.Update(country);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteCountryAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.Countries.FindAsync([id], ct);
        if (entity is null) return;
        _context.Countries.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    // ── States ───────────────────────────────────────────────────────

    public async Task<PagedResult<State>> GetStatesPagedAsync(int? countryId, GridQuery query, CancellationToken ct = default)
    {
        IQueryable<State> q = _context.States.AsNoTracking().Include(s => s.Country);

        if (countryId.HasValue)
            q = q.Where(s => s.CountryID == countryId.Value);
        if (query.GetSearch(nameof(State.Tile)) is string tile)
            q = q.Where(s => s.Tile.Contains(tile));
        if (query.GetSearch(nameof(State.Active)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(s => s.Active == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(State.StateID))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<State> { Items = items, TotalCount = total };
    }

    public async Task<IEnumerable<State>> GetStatesByCountryAsync(int countryId, CancellationToken ct = default) =>
        await _context.States.AsNoTracking()
            .Where(s => s.CountryID == countryId && s.Active)
            .OrderBy(s => s.Tile)
            .ToListAsync(ct);

    public async Task<State?> GetStateByIdAsync(int id, CancellationToken ct = default) =>
        await _context.States.FindAsync([id], ct);

    public async Task<State> CreateStateAsync(State state, CancellationToken ct = default)
    {
        _context.States.Add(state);
        await _context.SaveChangesAsync(ct);
        return state;
    }

    public async Task UpdateStateAsync(State state, CancellationToken ct = default)
    {
        _context.States.Update(state);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteStateAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.States.FindAsync([id], ct);
        if (entity is null) return;
        _context.States.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }

    // ── Cities ───────────────────────────────────────────────────────

    public async Task<PagedResult<City>> GetCitiesPagedAsync(int? countryId, int? stateId, GridQuery query, CancellationToken ct = default)
    {
        IQueryable<City> q = _context.Cities.AsNoTracking()
            .Include(c => c.Country)
            .Include(c => c.State);

        if (countryId.HasValue)
            q = q.Where(c => c.CountryID == countryId.Value);
        if (stateId.HasValue)
            q = q.Where(c => c.StateID == stateId.Value);
        if (query.GetSearch(nameof(City.Title)) is string title)
            q = q.Where(c => c.Title.Contains(title));
        if (query.GetSearch(nameof(City.Active)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(c => c.Active == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(City.CityID))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<City> { Items = items, TotalCount = total };
    }

    public async Task<City?> GetCityByIdAsync(int id, CancellationToken ct = default) =>
        await _context.Cities.FindAsync([id], ct);

    public async Task<City> CreateCityAsync(City city, CancellationToken ct = default)
    {
        _context.Cities.Add(city);
        await _context.SaveChangesAsync(ct);
        return city;
    }

    public async Task UpdateCityAsync(City city, CancellationToken ct = default)
    {
        _context.Cities.Update(city);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteCityAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.Cities.FindAsync([id], ct);
        if (entity is null) return;
        _context.Cities.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
