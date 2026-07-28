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

    public async Task<bool> CountryExistsAsync(string countryCode, string title, int? excludeId = null, CancellationToken ct = default)
    {
        var code = (countryCode ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(code))
            return false;

        var q = _context.Countries.AsNoTracking().AsQueryable();
        if (excludeId.HasValue)
            q = q.Where(c => c.CountryID != excludeId.Value);

        // ISO country code is unique (case-insensitive). Title is accepted for API symmetry with import rows.
        _ = title;
        return await q.AnyAsync(c => c.CountryCode.ToLower() == code.ToLower(), ct);
    }

    public async Task<Country> CreateCountryAsync(Country country, CancellationToken ct = default)
    {
        NormalizeCountry(country);
        if (await CountryExistsAsync(country.CountryCode, country.Title, null, ct))
            throw new InvalidOperationException("A country with this code already exists.");

        _context.Countries.Add(country);
        await _context.SaveChangesAsync(ct);
        return country;
    }

    public async Task UpdateCountryAsync(Country country, CancellationToken ct = default)
    {
        NormalizeCountry(country);
        if (await CountryExistsAsync(country.CountryCode, country.Title, country.CountryID, ct))
            throw new InvalidOperationException("A country with this code already exists.");

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

    public byte[] GetCountryImportSample() =>
        ExcelWorkbook.Write(
            "Countries",
            ["CountryCode", "Title", "LanguageCode", "PhonePrefix"],
            [new object?[] { "IR", "Iran", "fa", "98" }]);

    public async Task<LocationImportResult> ImportCountriesAsync(Stream excel, CancellationToken ct = default)
    {
        var rows = ExcelWorkbook.Read(excel);

        int added = 0, skipped = 0;
        var errors = new List<string>();
        int start = HasHeader(rows, "CountryCode") ? 1 : 0;

        // Existing codes for fast lookup (case-insensitive).
        var existingCodes = await _context.Countries
            .AsNoTracking()
            .Select(c => c.CountryCode.ToLower())
            .ToListAsync(ct);
        var knownCodes = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

        for (int i = start; i < rows.Count; i++)
        {
            var row = rows[i];
            int line = i + 1;
            if (IsEmptyRow(row)) continue;

            if (row.Length < 2)
            {
                errors.Add($"Row {line}: expected CountryCode,Title[,LanguageCode,PhonePrefix].");
                skipped++;
                continue;
            }

            var code = row[0].Trim();
            var title = row[1].Trim();
            var languageCode = row.Length > 2 ? row[2].Trim() : string.Empty;
            var phone = row.Length > 3 ? row[3].Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(title))
            {
                errors.Add($"Row {line}: CountryCode and Title are required.");
                skipped++;
                continue;
            }

            if (code.Length > 2) code = code[..2];
            if (languageCode.Length > 2) languageCode = languageCode[..2];
            if (phone.Length > 3) phone = phone[..3];

            if (knownCodes.Contains(code))
            {
                skipped++;
                continue;
            }

            var entity = new Country
            {
                CountryCode = code.ToUpperInvariant(),
                Title = title,
                LanguageCode = languageCode,
                PhonePerfix = phone,
            };
            _context.Countries.Add(entity);
            knownCodes.Add(code);
            added++;
        }

        if (added > 0)
            await _context.SaveChangesAsync(ct);

        return new LocationImportResult(added, skipped, errors);
    }

    // ── States ───────────────────────────────────────────────────────

    public async Task<PagedResult<State>> GetStatesPagedAsync(int? countryId, GridQuery query, CancellationToken ct = default)
    {
        IQueryable<State> q = _context.States.AsNoTracking().Include(s => s.Country);

        if (countryId.HasValue)
            q = q.Where(s => s.CountryID == countryId.Value);
        if (query.GetSearch(nameof(State.Title)) is string title)
            q = q.Where(s => s.Title.Contains(title));
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
            .OrderBy(s => s.Title)
            .ToListAsync(ct);

    public async Task<State?> GetStateByIdAsync(int id, CancellationToken ct = default) =>
        await _context.States.FindAsync([id], ct);

    public async Task<bool> StateExistsAsync(int countryId, string title, int? excludeId = null, CancellationToken ct = default)
    {
        var name = (title ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(name)) return false;

        var q = _context.States.AsNoTracking().Where(s => s.CountryID == countryId);
        if (excludeId.HasValue)
            q = q.Where(s => s.StateID != excludeId.Value);

        return await q.AnyAsync(s => s.Title.ToLower() == name.ToLower(), ct);
    }

    public async Task<State> CreateStateAsync(State state, CancellationToken ct = default)
    {
        NormalizeState(state);
        if (await StateExistsAsync(state.CountryID, state.Title, null, ct))
            throw new InvalidOperationException("A state with this title already exists in the selected country.");

        _context.States.Add(state);
        await _context.SaveChangesAsync(ct);
        return state;
    }

    public async Task UpdateStateAsync(State state, CancellationToken ct = default)
    {
        NormalizeState(state);
        if (await StateExistsAsync(state.CountryID, state.Title, state.StateID, ct))
            throw new InvalidOperationException("A state with this title already exists in the selected country.");

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

    public byte[] GetStateImportSample() =>
        ExcelWorkbook.Write(
            "States",
            ["CountryCode", "Title", "LanguageCode", "Active"],
            [new object?[] { "IR", "Tehran", "fa", "true" }]);

    public async Task<LocationImportResult> ImportStatesAsync(Stream excel, CancellationToken ct = default)
    {
        var rows = ExcelWorkbook.Read(excel);

        int added = 0, skipped = 0;
        var errors = new List<string>();
        int start = HasHeader(rows, "CountryCode") ? 1 : 0;

        var countries = await _context.Countries.AsNoTracking().ToListAsync(ct);
        var countryByCode = countries
            .GroupBy(c => c.CountryCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var existing = await _context.States.AsNoTracking()
            .Select(s => new { s.CountryID, Title = s.Title.ToLower() })
            .ToListAsync(ct);
        var known = new HashSet<(int CountryId, string Title)>(
            existing.Select(e => (e.CountryID, e.Title)));

        for (int i = start; i < rows.Count; i++)
        {
            var row = rows[i];
            int line = i + 1;
            if (IsEmptyRow(row)) continue;

            if (row.Length < 2)
            {
                errors.Add($"Row {line}: expected CountryCode,Title[,LanguageCode,Active].");
                skipped++;
                continue;
            }

            var countryCode = row[0].Trim();
            var title = row[1].Trim();
            var languageCode = row.Length > 2 ? row[2].Trim() : "en";
            var activeRaw = row.Length > 3 ? row[3].Trim() : "true";

            if (string.IsNullOrWhiteSpace(countryCode) || string.IsNullOrWhiteSpace(title))
            {
                errors.Add($"Row {line}: CountryCode and Title are required.");
                skipped++;
                continue;
            }

            if (!countryByCode.TryGetValue(countryCode, out var country))
            {
                errors.Add($"Row {line}: country code '{countryCode}' not found.");
                skipped++;
                continue;
            }

            if (languageCode.Length > 2) languageCode = languageCode[..2];
            var active = !bool.TryParse(activeRaw, out var a) || a;

            var key = (country.CountryID, title.ToLowerInvariant());
            if (known.Contains(key))
            {
                skipped++;
                continue;
            }

            _context.States.Add(new State
            {
                CountryID = country.CountryID,
                Title = title,
                LanguageCode = string.IsNullOrEmpty(languageCode) ? "en" : languageCode,
                Active = active,
            });
            known.Add(key);
            added++;
        }

        if (added > 0)
            await _context.SaveChangesAsync(ct);

        return new LocationImportResult(added, skipped, errors);
    }

    // ── Cities ───────────────────────────────────────────────────────

    public async Task<IEnumerable<City>> GetCitiesByCountryAsync(int countryId, CancellationToken ct = default) =>
        await _context.Cities.AsNoTracking()
            .Where(c => c.CountryID == countryId && c.Active)
            .OrderBy(c => c.Title)
            .ToListAsync(ct);

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

    public async Task<bool> CityExistsAsync(int countryId, string title, int? excludeId = null, CancellationToken ct = default)
    {
        var name = (title ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(name)) return false;

        var q = _context.Cities.AsNoTracking().Where(c => c.CountryID == countryId);
        if (excludeId.HasValue)
            q = q.Where(c => c.CityID != excludeId.Value);

        // Same title is only a duplicate within the same country.
        return await q.AnyAsync(c => c.Title.ToLower() == name.ToLower(), ct);
    }

    public async Task<City> CreateCityAsync(City city, CancellationToken ct = default)
    {
        NormalizeCity(city);
        if (await CityExistsAsync(city.CountryID, city.Title, null, ct))
            throw new InvalidOperationException("A city with this title already exists in the selected country.");

        _context.Cities.Add(city);
        await _context.SaveChangesAsync(ct);
        return city;
    }

    public async Task UpdateCityAsync(City city, CancellationToken ct = default)
    {
        NormalizeCity(city);
        if (await CityExistsAsync(city.CountryID, city.Title, city.CityID, ct))
            throw new InvalidOperationException("A city with this title already exists in the selected country.");

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

    public byte[] GetCityImportSample() =>
        ExcelWorkbook.Write(
            "Cities",
            ["CountryCode", "Title", "StateTitle", "LanguageCode", "Latitude", "Longitude", "Active"],
            [new object?[] { "IR", "Tehran", "Tehran", "fa", "35.6892", "51.3890", "true" }]);

    public async Task<LocationImportResult> ImportCitiesAsync(Stream excel, CancellationToken ct = default)
    {
        var rows = ExcelWorkbook.Read(excel);

        int added = 0, skipped = 0;
        var errors = new List<string>();
        int start = HasHeader(rows, "CountryCode") ? 1 : 0;

        var countries = await _context.Countries.AsNoTracking().ToListAsync(ct);
        var countryByCode = countries
            .GroupBy(c => c.CountryCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var states = await _context.States.AsNoTracking().ToListAsync(ct);

        var existing = await _context.Cities.AsNoTracking()
            .Select(c => new { c.CountryID, Title = c.Title.ToLower() })
            .ToListAsync(ct);
        var known = new HashSet<(int CountryId, string Title)>(
            existing.Select(e => (e.CountryID, e.Title)));

        for (int i = start; i < rows.Count; i++)
        {
            var row = rows[i];
            int line = i + 1;
            if (IsEmptyRow(row)) continue;

            if (row.Length < 2)
            {
                errors.Add($"Row {line}: expected CountryCode,Title[,StateTitle,LanguageCode,Latitude,Longitude,Active].");
                skipped++;
                continue;
            }

            var countryCode = row[0].Trim();
            var title = row[1].Trim();
            var stateTitle = row.Length > 2 ? row[2].Trim() : string.Empty;
            var languageCode = row.Length > 3 ? row[3].Trim() : "en";
            var latRaw = row.Length > 4 ? row[4].Trim() : string.Empty;
            var lngRaw = row.Length > 5 ? row[5].Trim() : "0";
            var activeRaw = row.Length > 6 ? row[6].Trim() : "true";

            if (string.IsNullOrWhiteSpace(countryCode) || string.IsNullOrWhiteSpace(title))
            {
                errors.Add($"Row {line}: CountryCode and Title are required.");
                skipped++;
                continue;
            }

            if (!countryByCode.TryGetValue(countryCode, out var country))
            {
                errors.Add($"Row {line}: country code '{countryCode}' not found.");
                skipped++;
                continue;
            }

            int? stateId = null;
            if (!string.IsNullOrWhiteSpace(stateTitle))
            {
                var state = states.FirstOrDefault(s =>
                    s.CountryID == country.CountryID
                    && string.Equals(s.Title, stateTitle, StringComparison.OrdinalIgnoreCase));
                if (state is null)
                {
                    errors.Add($"Row {line}: state '{stateTitle}' not found in country '{countryCode}'.");
                    skipped++;
                    continue;
                }
                stateId = state.StateID;
            }

            if (languageCode.Length > 2) languageCode = languageCode[..2];
            double? latitude = double.TryParse(latRaw, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var lat) ? lat : null;
            double longitude = double.TryParse(lngRaw, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var lng) ? lng : 0;
            var active = !bool.TryParse(activeRaw, out var a) || a;

            var key = (country.CountryID, title.ToLowerInvariant());
            if (known.Contains(key))
            {
                // Duplicate city title within same country (code + title) — skip silently.
                skipped++;
                continue;
            }

            _context.Cities.Add(new City
            {
                CountryID = country.CountryID,
                StateID = stateId,
                Title = title,
                LanguageCode = string.IsNullOrEmpty(languageCode) ? "en" : languageCode,
                Latitude = latitude,
                Longitude = longitude,
                Active = active,
            });
            known.Add(key);
            added++;
        }

        if (added > 0)
            await _context.SaveChangesAsync(ct);

        return new LocationImportResult(added, skipped, errors);
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private static void NormalizeCountry(Country country)
    {
        country.CountryCode = (country.CountryCode ?? string.Empty).Trim().ToUpperInvariant();
        country.Title = (country.Title ?? string.Empty).Trim();
        country.LanguageCode = (country.LanguageCode ?? string.Empty).Trim();
        country.PhonePerfix = (country.PhonePerfix ?? string.Empty).Trim();
    }

    private static void NormalizeState(State state)
    {
        state.Title = (state.Title ?? string.Empty).Trim();
        state.LanguageCode = (state.LanguageCode ?? string.Empty).Trim();
    }

    private static void NormalizeCity(City city)
    {
        city.Title = (city.Title ?? string.Empty).Trim();
        city.LanguageCode = (city.LanguageCode ?? string.Empty).Trim();
    }

    private static bool HasHeader(List<string[]> rows, string firstColumn) =>
        rows.Count > 0 && rows[0].Length > 0
        && rows[0][0].Trim().Equals(firstColumn, StringComparison.OrdinalIgnoreCase);

    private static bool IsEmptyRow(string[] row) =>
        row.Length == 0 || row.All(string.IsNullOrWhiteSpace);
}
