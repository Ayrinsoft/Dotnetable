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
        await _service.SetAsync(websiteId: 1, languageId: 1, key: "hello", value: "Hello World");

        var result = _service.Get("hello", websiteId: 1, languageId: 1);

        result.Should().Be("Hello World");
    }

    [Fact]
    public async Task LoadAsync_ShouldPopulateCache()
    {
        _context.Translations.Add(new Translation
        {
            LanguageId = 1,
            WebsiteId = 1,
            Key = "welcome",
            Value = "Welcome"
        });
        await _context.SaveChangesAsync();

        await _service.LoadAsync(websiteId: 1, languageId: 1);

        var result = _service.Get("welcome", websiteId: 1, languageId: 1);
        result.Should().Be("Welcome");
    }

    [Fact]
    public void Get_WithMissingKey_ShouldReturnFallback()
    {
        var result = _service.Get("missing.key", websiteId: 1, languageId: 1, fallback: "Default");

        result.Should().Be("Default");
    }

    public void Dispose() => _context.Dispose();
}
