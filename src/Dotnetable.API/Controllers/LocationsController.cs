using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dotnetable.API.Controllers;

/// <summary>Public reference data (countries/cities) — not website-scoped, used to populate address forms.</summary>
[AllowAnonymous]
public class LocationsController : BaseController
{
    private readonly ILocationService _locationService;

    public LocationsController(ILocationService locationService) => _locationService = locationService;

    [HttpGet("countries")]
    public async Task<IActionResult> GetCountries(CancellationToken ct = default)
    {
        var countries = await _locationService.GetAllCountriesAsync(ct);
        return Ok(countries.Select(c => new LocationOptionDto { Id = c.CountryID, Title = c.Title }));
    }

    [HttpGet("countries/{countryId:int}/cities")]
    public async Task<IActionResult> GetCities(int countryId, CancellationToken ct = default)
    {
        var cities = await _locationService.GetCitiesByCountryAsync(countryId, ct);
        return Ok(cities.Select(c => new LocationOptionDto { Id = c.CityID, Title = c.Title }));
    }
}
