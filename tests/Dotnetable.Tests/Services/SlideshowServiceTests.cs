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
        _service = new SlideshowService(_context);

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
    public async Task CreateSlideshowAsync_AppliesEntityDefaults_AndCreatedAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-2);
        var created = await _service.CreateSlideshowAsync(new Slideshow
        {
            WebsiteID = _website.WebsiteID,
            Name = "Home hero",
        });

        created.SlideshowID.Should().BeGreaterThan(0);
        created.CreatedAt.Should().BeOnOrAfter(before);
        created.TransitionEffect.Should().Be(1);
        created.AutoPlay.Should().BeTrue();
        created.IntervalMs.Should().Be(5000);
        created.ShowArrows.Should().BeTrue();
        created.ShowDots.Should().BeTrue();
        created.EnableLightbox.Should().BeTrue();
        created.IsActive.Should().BeTrue();

        var stored = await _context.Slideshows.FindAsync(created.SlideshowID);
        stored.Should().NotBeNull();
        stored!.AutoPlay.Should().BeTrue();
        stored.IntervalMs.Should().Be(5000);
        stored.CreatedAt.Should().Be(created.CreatedAt);
    }

    [Fact]
    public async Task CreateSlideAsync_AppliesIsActiveDefault()
    {
        var show = await _service.CreateSlideshowAsync(new Slideshow
        {
            WebsiteID = _website.WebsiteID,
            Name = "Banner",
        });

        // In-memory provider does not enforce FKs; FileID is only stored.
        var slide = await _service.CreateSlideAsync(new SlideshowSlide
        {
            SlideshowID = show.SlideshowID,
            FileID = 1,
            Title = "One",
        });

        slide.IsActive.Should().BeTrue();
        slide.OpenInNewTab.Should().BeFalse();
        slide.SortOrder.Should().Be(0);
    }

    public void Dispose() => _context.Dispose();
}
