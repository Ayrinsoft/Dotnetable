using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class ThemeServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ThemeService _service;
    private readonly Website _website;
    private readonly string _themesRoot;

    public ThemeServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        // Services open a context per call now, so they get a factory over the same options;
        // the fixture keeps its own _context for seeding and asserting.
        var factory = new TestDbContextFactory(opts);
        _themesRoot = Path.Combine(Path.GetTempPath(), "dn-theme-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_themesRoot, ThemeService.BuiltinSlug, "Views"));
        File.WriteAllText(Path.Combine(_themesRoot, ThemeService.BuiltinSlug, "theme.json"),
            """{"name":"Default","slug":"Default","version":"1.0.0"}""");

        _service = new ThemeService(factory, _themesRoot);

        _website = NewWebsite("Test", "test.com");
        _context.Websites.Add(_website);
        _context.SaveChanges();
    }

    private static Website NewWebsite(string trade, string address) => new()
    {
        TradeName = trade, BrandName = trade, WebsiteAddress = address,
        AuthCode = Guid.NewGuid(), Active = true, Manager = "Mgr", Mobile = "123",
        Email = $"admin@{address}", RegisterDate = DateOnly.FromDateTime(DateTime.Today),
        DefaultLanguageCode = "en", DefaultCurrencyCode = "USD",
    };

    private async Task<WebsiteTheme> SeedPackageAsync(string slug, string name, bool active = false, int? websiteId = null)
    {
        var wid = websiteId ?? _website.WebsiteID;
        var dir = Path.Combine(_themesRoot, wid.ToString(), slug, "Views");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(_themesRoot, wid.ToString(), slug, "theme.json"),
            $$"""{"name":"{{name}}","slug":"{{slug}}","version":"1.0.0"}""");

        var entity = new WebsiteTheme
        {
            WebsiteID = wid,
            Slug = slug,
            Name = name,
            Version = "1.0.0",
            IsActive = active,
            CreatedAt = DateTime.UtcNow,
        };
        _context.WebsiteThemes.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    private static MemoryStream SampleThemeZip(string slug = "ocean", string name = "Ocean")
    {
        var ms = new MemoryStream();
        using (var zip = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Create, leaveOpen: true))
        {
            var manifest = zip.CreateEntry("theme.json");
            using (var w = new StreamWriter(manifest.Open()))
                w.Write($$"""{"name":"{{name}}","slug":"{{slug}}","version":"2.0.0","author":"Test"}""");

            var layout = zip.CreateEntry("Views/Shared/_Layout.cshtml");
            using (var w = new StreamWriter(layout.Open()))
                w.Write("@RenderBody()");
        }
        ms.Position = 0;
        return ms;
    }

    [Fact]
    public async Task GetThemesAsync_IncludesBuiltinDefault()
    {
        var list = await _service.GetThemesAsync(_website.WebsiteID);
        list.Should().ContainSingle(t => t.IsBuiltin && t.Slug == "Default" && t.IsActive);
    }

    [Fact]
    public async Task InstallFromZipAsync_PersistsPackageAndFiles()
    {
        await using var zip = SampleThemeZip();
        var dto = await _service.InstallFromZipAsync(_website.WebsiteID, zip, "ocean.zip");

        dto.Slug.Should().Be("ocean");
        dto.Name.Should().Be("Ocean");
        dto.IsBuiltin.Should().BeFalse();
        Directory.Exists(Path.Combine(_themesRoot, _website.WebsiteID.ToString(), "ocean", "Views")).Should().BeTrue();
        (await _context.WebsiteThemes.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task ActivateThemeAsync_Package_DeactivatesSiblings()
    {
        var other = NewWebsite("Other", "other.com");
        _context.Websites.Add(other);
        await _context.SaveChangesAsync();

        await SeedPackageAsync("a", "A", active: true);
        await SeedPackageAsync("b", "B");
        await SeedPackageAsync("foreign", "Foreign", active: true, websiteId: other.WebsiteID);

        await _service.ActivateThemeAsync(_website.WebsiteID, "b");

        // The service wrote through its own short-lived context, so this fixture's context must
        // re-read rather than answer from entities it is still tracking.
        _context.ChangeTracker.Clear();

        (await _context.WebsiteThemes.SingleAsync(t => t.Slug == "a")).IsActive.Should().BeFalse();
        (await _context.WebsiteThemes.SingleAsync(t => t.Slug == "b")).IsActive.Should().BeTrue();
        (await _context.WebsiteThemes.SingleAsync(t => t.Slug == "foreign")).IsActive.Should().BeTrue();

        var active = await _service.GetActiveThemeAsync(_website.WebsiteID);
        active.Slug.Should().Be("b");
        active.ViewRoot.Should().Be($"{_website.WebsiteID}/b");
    }

    [Fact]
    public async Task ActivateThemeAsync_Default_ClearsPackageActiveFlags()
    {
        await SeedPackageAsync("ocean", "Ocean", active: true);
        await _service.ActivateThemeAsync(_website.WebsiteID, "Default");

        _context.WebsiteThemes.All(t => !t.IsActive).Should().BeTrue();
        (await _service.GetActiveThemeAsync(_website.WebsiteID)).Slug.Should().Be("Default");
    }

    [Fact]
    public async Task DeleteThemeAsync_RemovesFilesAndRow()
    {
        await SeedPackageAsync("doomed", "Doomed");
        await _service.DeleteThemeAsync(_website.WebsiteID, "doomed");

        _context.WebsiteThemes.Should().BeEmpty();
        Directory.Exists(Path.Combine(_themesRoot, _website.WebsiteID.ToString(), "doomed")).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteThemeAsync_Active_Throws()
    {
        await SeedPackageAsync("live", "Live", active: true);
        await _service.Invoking(s => s.DeleteThemeAsync(_website.WebsiteID, "live"))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteThemeAsync_Builtin_Throws()
    {
        await _service.Invoking(s => s.DeleteThemeAsync(_website.WebsiteID, "Default"))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ExportZipAsync_ReturnsBytes()
    {
        await using var zip = SampleThemeZip("export-me", "Export Me");
        await _service.InstallFromZipAsync(_website.WebsiteID, zip);

        var (bytes, fileName) = await _service.ExportZipAsync(_website.WebsiteID, "export-me");
        bytes.Length.Should().BeGreaterThan(20);
        fileName.Should().Be("export-me.zip");
    }

    [Fact]
    public async Task GetActiveThemeAsync_NoActivePackage_ReturnsDefault()
    {
        await SeedPackageAsync("idle", "Idle", active: false);
        var active = await _service.GetActiveThemeAsync(_website.WebsiteID);
        active.Slug.Should().Be("Default");
        active.ViewRoot.Should().Be("Default");
    }

    public void Dispose()
    {
        _context.Dispose();
        try { if (Directory.Exists(_themesRoot)) Directory.Delete(_themesRoot, recursive: true); }
        catch { /* ignore */ }
    }
}
