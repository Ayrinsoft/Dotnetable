using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class SlideshowServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly SlideshowService _service;
    private readonly Website _website;

    public SlideshowServiceTests()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(opts);
        // Services open a context per call now, so they get a factory over the same options;
        // the fixture keeps its own _context for seeding and asserting.
        var factory = new TestDbContextFactory(opts);
        _service = new SlideshowService(factory);

        _website = new Website
        {
            TradeName = "Test", BrandName = "Test", WebsiteAddress = "test.com",
            AuthCode = Guid.NewGuid(), Active = true, Manager = "Mgr", Mobile = "1",
            Email = "a@test.com", RegisterDate = DateOnly.FromDateTime(DateTime.Today),
            DefaultLanguageCode = "en", DefaultCurrencyCode = "USD",
        };
        _context.Websites.Add(_website);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateSlideshowAsync_SetsCreatedAt_AndFillsMissingTransitionInterval()
    {
        var before = DateTime.UtcNow.AddSeconds(-2);
        var created = await _service.CreateSlideshowAsync(new Slideshow
        {
            WebsiteID = _website.WebsiteID,
            Name = "Home hero",
            // Explicit business values (not entity defaults) — what the admin UI would send:
            TransitionEffect = 1,
            AutoPlay = true,
            IntervalMs = 5000,
            ShowArrows = true,
            ShowDots = true,
            EnableLightbox = true,
            IsActive = true,
        });

        created.SlideshowID.Should().BeGreaterThan(0);
        created.CreatedAt.Should().BeOnOrAfter(before);
        created.IntervalMs.Should().Be(5000);
        created.TransitionEffect.Should().Be(1);

        var stored = await _context.Slideshows.FindAsync(created.SlideshowID);
        stored!.CreatedAt.Should().Be(created.CreatedAt);
    }

    [Fact]
    public async Task CreateSlideshowAsync_AppliesServiceFallback_WhenTransitionAndIntervalUnset()
    {
        var created = await _service.CreateSlideshowAsync(new Slideshow
        {
            WebsiteID = _website.WebsiteID,
            Name = "Minimal",
            // TransitionEffect/IntervalMs left at CLR 0 — service fills them
            AutoPlay = true,
            IsActive = true,
        });

        created.TransitionEffect.Should().Be(1);
        created.IntervalMs.Should().Be(5000);
    }

    [Fact]
    public async Task CreateSlideAsync_PersistsSlide()
    {
        var show = await _service.CreateSlideshowAsync(new Slideshow
        {
            WebsiteID = _website.WebsiteID,
            Name = "Banner",
            TransitionEffect = 1,
            IntervalMs = 5000,
            IsActive = true,
        });

        var slide = await _service.CreateSlideAsync(new SlideshowSlide
        {
            SlideshowID = show.SlideshowID,
            FileID = 1,
            Title = "One",
            IsActive = true,
            SortOrder = 0,
        });

        slide.SlideshowSlideID.Should().BeGreaterThan(0);
        slide.IsActive.Should().BeTrue();
    }

    public void Dispose() => _context.Dispose();
}
