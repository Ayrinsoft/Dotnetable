using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class LocalizationServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly LocalizationService _service;
    private readonly TranslationCache _cache;

    public LocalizationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _cache = new TranslationCache();
        _service = new LocalizationService(_context, _cache);
    }

    [Fact]
    public async Task SetAsync_ThenGet_ShouldReturnCorrectValue()
    {
        await _service.SetAsync(websiteId: 1, languageCode: "en", key: "hello", value: "Hello World");

        var result = _service.Get(websiteId: 1, languageCode: "en", key: "hello");

        result.Should().Be("Hello World");
    }

    [Fact]
    public async Task LoadAsync_ShouldPopulateCache()
    {
        var key = new LocalizationKey { WebsiteID = 1, ItemKey = "welcome", DefaultValue = "Welcome" };
        key.LocalizationValues.Add(new LocalizationValue { LanguageCode = "en", ItemValue = "Welcome" });
        _context.LocalizationKeys.Add(key);
        await _context.SaveChangesAsync();

        await _service.LoadAsync(websiteId: 1, languageCode: "en");

        var result = _service.Get(websiteId: 1, languageCode: "en", key: "welcome");
        result.Should().Be("Welcome");
    }

    [Fact]
    public void Get_WithMissingKey_ShouldReturnFallback()
    {
        var result = _service.Get(websiteId: 1, languageCode: "en", key: "missing.key", fallback: "Default");

        result.Should().Be("Default");
    }

    public void Dispose() => _context.Dispose();
}
