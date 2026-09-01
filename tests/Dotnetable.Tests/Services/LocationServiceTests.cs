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
        _service = new LocationService(new TestDbContextFactory(options));
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

        OfficeOpenXml.ExcelPackage.License.SetNonCommercialOrganization("Dotnetable");
        using var package = new OfficeOpenXml.ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Cities");
        string[] headers = ["CountryCode", "Title", "StateTitle", "LanguageCode", "Latitude", "Longitude", "Active"];
        for (var c = 0; c < headers.Length; c++)
            sheet.Cells[1, c + 1].Value = headers[c];
        object?[][] data =
        [
            ["IR", "Tehran", "", "fa", "35.6", "51.3", "true"],
            ["IR", "Tehran", "", "fa", "35.6", "51.3", "true"],
            ["TR", "Tehran", "", "tr", "41.0", "28.9", "true"],
        ];
        for (var r = 0; r < data.Length; r++)
            for (var c = 0; c < data[r].Length; c++)
                sheet.Cells[r + 2, c + 1].Value = data[r][c];

        await using var stream = new MemoryStream(package.GetAsByteArray());
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

        OfficeOpenXml.ExcelPackage.License.SetNonCommercialOrganization("Dotnetable");
        using var package = new OfficeOpenXml.ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add("Countries");
        sheet.Cells[1, 1].Value = "CountryCode";
        sheet.Cells[1, 2].Value = "Title";
        sheet.Cells[1, 3].Value = "LanguageCode";
        sheet.Cells[1, 4].Value = "PhonePrefix";
        sheet.Cells[2, 1].Value = "IR";
        sheet.Cells[2, 2].Value = "Iran Again";
        sheet.Cells[2, 3].Value = "fa";
        sheet.Cells[2, 4].Value = "98";
        sheet.Cells[3, 1].Value = "TR";
        sheet.Cells[3, 2].Value = "Turkey";
        sheet.Cells[3, 3].Value = "tr";
        sheet.Cells[3, 4].Value = "90";
        await using var stream = new MemoryStream(package.GetAsByteArray());
        var result = await _service.ImportCountriesAsync(stream);

        result.Added.Should().Be(1);
        result.Skipped.Should().Be(1);
        (await _context.Countries.CountAsync()).Should().Be(2);
    }

    [Fact]
    public void GetCountryImportSample_ContainsOneDataRow()
    {
        OfficeOpenXml.ExcelPackage.License.SetNonCommercialOrganization("Dotnetable");
        var bytes = _service.GetCountryImportSample();
        using var package = new OfficeOpenXml.ExcelPackage(new MemoryStream(bytes));
        var sheet = package.Workbook.Worksheets.First();
        sheet.Cells[1, 1].Text.Should().Be("CountryCode");
        sheet.Cells[2, 1].Text.Should().Be("IR");
        sheet.Dimension.Rows.Should().Be(2); // header + 1 sample
    }

    public void Dispose() => _context.Dispose();
}
