using System.Text;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class LocationServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly LocationService _service;

    public LocationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _service = new LocationService(_context);
    }

    [Fact]
    public async Task CreateCountryAsync_RejectsDuplicateCountryCode()
    {
        await _service.CreateCountryAsync(new Country
        {
            CountryCode = "IR",
            Title = "Iran",
            LanguageCode = "fa",
            PhonePerfix = "98",
        });

        var act = async () => await _service.CreateCountryAsync(new Country
        {
            CountryCode = "ir",
            Title = "Islamic Republic of Iran",
            LanguageCode = "en",
            PhonePerfix = "98",
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreateCityAsync_AllowsSameTitleInDifferentCountries()
    {
        var ir = await _service.CreateCountryAsync(new Country
        {
            CountryCode = "IR", Title = "Iran", LanguageCode = "fa", PhonePerfix = "98",
        });
        var tr = await _service.CreateCountryAsync(new Country
        {
            CountryCode = "TR", Title = "Turkey", LanguageCode = "tr", PhonePerfix = "90",
        });

        await _service.CreateCityAsync(new City
        {
            CountryID = ir.CountryID, Title = "Springfield", LanguageCode = "en", Active = true, Longitude = 0,
        });

        var other = await _service.CreateCityAsync(new City
        {
            CountryID = tr.CountryID, Title = "Springfield", LanguageCode = "en", Active = true, Longitude = 0,
        });

        other.CityID.Should().BeGreaterThan(0);
        (await _context.Cities.CountAsync(c => c.Title == "Springfield")).Should().Be(2);
    }

    [Fact]
    public async Task CreateCityAsync_RejectsDuplicateTitleInSameCountry()
    {
        var ir = await _service.CreateCountryAsync(new Country
        {
            CountryCode = "IR", Title = "Iran", LanguageCode = "fa", PhonePerfix = "98",
        });

        await _service.CreateCityAsync(new City
        {
            CountryID = ir.CountryID, Title = "Tehran", LanguageCode = "fa", Active = true, Longitude = 0,
        });

        var act = async () => await _service.CreateCityAsync(new City
        {
            CountryID = ir.CountryID, Title = "tehran", LanguageCode = "en", Active = true, Longitude = 0,
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreateStateAsync_RejectsDuplicateTitleInSameCountry()
    {
        var ir = await _service.CreateCountryAsync(new Country
        {
            CountryCode = "IR", Title = "Iran", LanguageCode = "fa", PhonePerfix = "98",
        });

        await _service.CreateStateAsync(new State
        {
            CountryID = ir.CountryID, Title = "Tehran", LanguageCode = "fa", Active = true,
        });

        var act = async () => await _service.CreateStateAsync(new State
        {
            CountryID = ir.CountryID, Title = "Tehran", LanguageCode = "en", Active = true,
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ImportCitiesAsync_SkipsDuplicatesAndAllowsCrossCountrySameName()
    {
        await _service.CreateCountryAsync(new Country
        {
            CountryCode = "IR", Title = "Iran", LanguageCode = "fa", PhonePerfix = "98",
        });
        await _service.CreateCountryAsync(new Country
        {
            CountryCode = "TR", Title = "Turkey", LanguageCode = "tr", PhonePerfix = "90",
        });

        var csv = """
            CountryCode,Title,StateTitle,LanguageCode,Latitude,Longitude,Active
            IR,Tehran,,fa,35.6,51.3,true
            IR,Tehran,,fa,35.6,51.3,true
            TR,Tehran,,tr,41.0,28.9,true
            """;

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var result = await _service.ImportCitiesAsync(stream);

        result.Added.Should().Be(2);
        result.Skipped.Should().Be(1);
        (await _context.Cities.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task ImportCountriesAsync_SkipsDuplicateCodes()
    {
        await _service.CreateCountryAsync(new Country
        {
            CountryCode = "IR", Title = "Iran", LanguageCode = "fa", PhonePerfix = "98",
        });

        var csv = """
            CountryCode,Title,LanguageCode,PhonePrefix
            IR,Iran Again,fa,98
            TR,Turkey,tr,90
            """;

        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var result = await _service.ImportCountriesAsync(stream);

        result.Added.Should().Be(1);
        result.Skipped.Should().Be(1);
        (await _context.Countries.CountAsync()).Should().Be(2);
    }

    [Fact]
    public void GetCountryImportSample_ContainsOneDataRow()
    {
        var text = Encoding.UTF8.GetString(_service.GetCountryImportSample());
        var lines = text.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Length.Should().Be(2); // header + 1 sample
        lines[0].Should().Contain("CountryCode");
    }

    public void Dispose() => _context.Dispose();
}
