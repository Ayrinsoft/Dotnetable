using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class PostLocationServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PostService _posts;
    private readonly int _websiteId;

    public PostLocationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _posts = new PostService(new TestDbContextFactory(options));

        var website = new Website
        {
            TradeName = "Site", BrandName = "Site", WebsiteAddress = "site.test",
            AuthCode = Guid.NewGuid(), Active = true, Manager = "Mgr", Mobile = "1",
            Email = "mgr@site.test", RegisterDate = DateOnly.FromDateTime(DateTime.UtcNow),
            DefaultLanguageCode = "en", DefaultCurrencyCode = "USD",
        };
        _context.Websites.Add(website);
        _context.SaveChanges();
        _websiteId = website.WebsiteID;
        _context.PostTypes.Add(new PostType
        {
            WebsiteID = _websiteId, Name = "News", Slug = "news", CommentsEnabled = true,
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task SetLocationAsync_PublishesLocalizedAddressAndCity()
    {
        var country = new Country { CountryCode = "IR", LanguageCode = "en", Title = "Iran", PhonePerfix = "98" };
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();
        var city = new City { CountryID = country.CountryID, Title = "Tehran", LanguageCode = "en", Longitude = 51.4, Active = true };
        city.CityTranslations.Add(new CityTranslation { LanguageCode = "fa", Title = "تهران" });
        _context.Cities.Add(city);
        await _context.SaveChangesAsync();
        var post = await _posts.CreateAsync(new Post
        {
            WebsiteID = _websiteId, PostTypeID = _context.PostTypes.Single().PostTypeID, Slug = "office", Title = "Office",
            Status = PostService.PublishedStatus, IsActive = true,
        });

        await _posts.SetLocationAsync(post.PostID, true, new PostLocationInput
        {
            Latitude = 35.7,
            Longitude = 51.4,
            Kind = PostLocationKind.Store,
            Address = "Valiasr",
            CityID = city.CityID,
        }, [new PostLocationTranslation { LanguageCode = "fa", Address = "ولیعصر" }]);

        _context.ChangeTracker.Clear();
        var detail = await _posts.GetBySlugAsync(_websiteId, "office", "fa");
        detail.Should().NotBeNull();
        detail!.HasLocation.Should().BeTrue();
        detail.Location!.Address.Should().Be("ولیعصر");
        detail.Location.CityName.Should().Be("تهران");
        detail.Location.Latitude.Should().Be(35.7);
        detail.Location.Kind.Should().Be(PostLocationKind.Store);

        var english = await _posts.GetBySlugAsync(_websiteId, "office", "en");
        english!.Location!.Address.Should().Be("Valiasr");
        english.Location.CityName.Should().Be("Tehran");
    }

    [Fact]
    public async Task SetLocationAsync_Off_RemovesTheRow()
    {
        var post = await _posts.CreateAsync(new Post
        {
            WebsiteID = _websiteId, PostTypeID = _context.PostTypes.Single().PostTypeID, Slug = "park", Title = "Park",
            Status = PostService.PublishedStatus, IsActive = true,
        });
        await _posts.SetLocationAsync(post.PostID, true, new PostLocationInput { Latitude = 1, Longitude = 2, Address = "Here" }, []);
        await _posts.SetLocationAsync(post.PostID, false, null, []);

        _context.ChangeTracker.Clear();
        (await _context.PostLocations.AnyAsync()).Should().BeFalse();
        var detail = await _posts.GetBySlugAsync(_websiteId, "park");
        detail!.HasLocation.Should().BeFalse();
        detail.Location.Should().BeNull();
    }

    [Fact]
    public async Task SetLocationAsync_OutOfRange_Throws()
    {
        var post = await _posts.CreateAsync(new Post
        {
            WebsiteID = _websiteId, PostTypeID = _context.PostTypes.Single().PostTypeID, Slug = "bad", Title = "Bad",
            Status = PostService.PublishedStatus, IsActive = true,
        });

        var act = () => _posts.SetLocationAsync(post.PostID, true, new PostLocationInput { Latitude = 120, Longitude = 0 }, []);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SetLocationAsync_UnknownKind_Throws()
    {
        var post = await _posts.CreateAsync(new Post
        {
            WebsiteID = _websiteId, PostTypeID = _context.PostTypes.Single().PostTypeID, Slug = "kind", Title = "Kind",
            Status = PostService.PublishedStatus, IsActive = true,
        });

        var act = () => _posts.SetLocationAsync(post.PostID, true, new PostLocationInput
        {
            Latitude = 1, Longitude = 2, Kind = (PostLocationKind)99,
        }, []);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetBySlug_UnknownStoredKind_ReadsAsUnspecified()
    {
        var post = await _posts.CreateAsync(new Post
        {
            WebsiteID = _websiteId, PostTypeID = _context.PostTypes.Single().PostTypeID, Slug = "legacy", Title = "Legacy",
            Status = PostService.PublishedStatus, IsActive = true, HasLocation = true,
        });
        _context.PostLocations.Add(new PostLocation
        {
            PostID = post.PostID, Latitude = 1, Longitude = 2, Kind = 99,
        });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var detail = await _posts.GetBySlugAsync(_websiteId, "legacy");
        detail!.Location!.Kind.Should().Be(PostLocationKind.Unspecified);
    }

    public void Dispose() => _context.Dispose();
}
