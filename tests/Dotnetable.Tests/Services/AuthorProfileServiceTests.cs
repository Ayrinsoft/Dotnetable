using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class AuthorProfileServiceTests : IDisposable
{
    private const int SiteId = 1;
    private const int OtherSiteId = 2;
    private const int WriterId = 40;
    private const int ColleagueId = 41;
    private const int InactiveId = 42;
    private const int OtherSiteWriterId = 43;
    private const int PostTypeId = 1;

    private readonly RelationalTestDb _db;
    private readonly AppDbContext _context;
    private readonly AuthorProfileService _service;
    private readonly PostService _posts;

    public AuthorProfileServiceTests()
    {
        _db = new RelationalTestDb();
        _context = _db.NewContext();
        var factory = new TestDbContextFactory(_db.Options);
        _service = new AuthorProfileService(factory);
        _posts = new PostService(factory);
        Seed();
    }

    public void Dispose()
    {
        _context.Dispose();
        _db.Dispose();
    }

    private void Seed()
    {
        _context.Members.AddRange(
            NewMember(WriterId, SiteId, "sara", "Sara", "Ahmadi"),
            NewMember(ColleagueId, SiteId, "ali", "Ali", "Ahmadi"),
            NewMember(InactiveId, SiteId, "gone", "Gone", "Writer", active: false),
            NewMember(OtherSiteWriterId, OtherSiteId, "other", "Sara", "Ahmadi"));
        _context.PostTypes.Add(new PostType { PostTypeID = PostTypeId, WebsiteID = SiteId, Name = "Blog", Slug = "blog", CommentsEnabled = true });
        _context.Posts.AddRange(
            NewPost(10, WriterId),
            NewPost(11, WriterId),
            NewPost(12, WriterId, status: 0),
            NewPost(13, ColleagueId));
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
    }

    private static Member NewMember(int id, int websiteId, string username, string given, string surname, bool active = true) => new()
    {
        MemberID = id, WebsiteID = websiteId, Username = username, Password = "x", Active = active,
        Email = $"{username}@example.com", CellphoneNumber = "0", CountryCode = "98",
        Givenname = given, Surname = surname,
    };

    private static Post NewPost(int id, int authorId, byte status = PostService.PublishedStatus) => new()
    {
        PostID = id, WebsiteID = SiteId, PostTypeID = PostTypeId, AuthorMemberID = authorId,
        Slug = $"p{id}", Title = $"Post {id}", Status = status, IsActive = true,
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
    };

    private Task<AuthorProfile> SaveProfileAsync(int memberId, Action<AuthorProfile>? configure = null,
        IReadOnlyList<AuthorProfileTranslation>? translations = null)
    {
        var profile = new AuthorProfile { MemberID = memberId, ShowBioOnPosts = true, ResumeEnabled = true, Bio = "I write about .NET." };
        configure?.Invoke(profile);
        return _service.SaveAsync(profile, translations ?? []);
    }

    // ── Save ────────────────────────────────────────────────────────

    [Fact]
    public async Task Save_Creates_Profile_On_Members_Website_With_Slug_From_Name()
    {
        var saved = await SaveProfileAsync(WriterId, p => p.WebsiteID = OtherSiteId);

        saved.Slug.Should().Be("sara-ahmadi");
        saved.WebsiteID.Should().Be(SiteId, "the website always follows the member, whatever the caller passes");
    }

    [Fact]
    public async Task Save_Twice_Updates_The_Same_Row()
    {
        await SaveProfileAsync(WriterId);
        await SaveProfileAsync(WriterId, p => p.Bio = "Updated");

        (await _context.AuthorProfiles.CountAsync(p => p.MemberID == WriterId)).Should().Be(1);
        (await _service.GetByMemberAsync(WriterId))!.Bio.Should().Be("Updated");
    }

    [Fact]
    public async Task Save_Makes_Slug_Unique_Per_Website_Only()
    {
        await SaveProfileAsync(WriterId, p => p.Slug = "writer");
        var colleague = await SaveProfileAsync(ColleagueId, p => p.Slug = "Writer");
        var otherSite = await SaveProfileAsync(OtherSiteWriterId, p => p.Slug = "writer");

        colleague.Slug.Should().Be("writer-2");
        otherSite.Slug.Should().Be("writer");
    }

    [Fact]
    public async Task Save_Keeps_Own_Slug_When_Unchanged()
    {
        await SaveProfileAsync(WriterId, p => p.Slug = "writer");
        var again = await SaveProfileAsync(WriterId, p => p.Slug = "writer");

        again.Slug.Should().Be("writer");
    }

    [Fact]
    public async Task Save_Drops_Unsafe_Urls_And_Normalizes_Skills()
    {
        var saved = await SaveProfileAsync(WriterId, p =>
        {
            p.WebsiteUrl = "javascript:alert(1)";
            p.Skills = " C#, Blazor ,c#,, SQL ";
            p.SocialLinksJson = AuthorSocialLinks.Serialize(
            [
                new AuthorSocialLink { Network = "github", Url = "https://github.com/sara" },
                new AuthorSocialLink { Network = "evil", Url = "https://example.com" },
                new AuthorSocialLink { Network = "x", Url = "javascript:alert(1)" },
            ]);
        });

        saved.WebsiteUrl.Should().BeNull();
        saved.Skills.Should().Be("C#, Blazor, SQL");
        var links = AuthorSocialLinks.Parse(saved.SocialLinksJson);
        links.Select(l => (l.Network, l.Url)).Should().Equal(
            ("github", "https://github.com/sara"),
            ("website", "https://example.com"));
    }

    [Fact]
    public async Task Save_Replaces_Translations_And_Skips_Blank_Ones()
    {
        await SaveProfileAsync(WriterId, translations:
        [
            new AuthorProfileTranslation { LanguageCode = "fa", Bio = "دربارهٔ من" },
            new AuthorProfileTranslation { LanguageCode = "de", Bio = "Über mich" },
        ]);
        await SaveProfileAsync(WriterId, translations:
        [
            new AuthorProfileTranslation { LanguageCode = "fa", Bio = "بیوی جدید" },
            new AuthorProfileTranslation { LanguageCode = "de", Bio = "  " },
        ]);

        var rows = await _context.AuthorProfileTranslations.AsNoTracking().ToListAsync();
        rows.Should().ContainSingle();
        rows[0].LanguageCode.Should().Be("fa");
        rows[0].Bio.Should().Be("بیوی جدید");
    }

    // ── Résumé items ────────────────────────────────────────────────

    [Fact]
    public async Task SaveResumeItem_Requires_A_Profile()
    {
        var act = () => _service.SaveResumeItemAsync(WriterId, new AuthorResumeItem { Title = "Dev" }, []);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SaveResumeItem_Rejects_Another_Members_Item()
    {
        await SaveProfileAsync(WriterId);
        await SaveProfileAsync(ColleagueId);
        var item = await _service.SaveResumeItemAsync(WriterId, new AuthorResumeItem { Title = "Dev", IsActive = true }, []);

        var hijack = () => _service.SaveResumeItemAsync(ColleagueId,
            new AuthorResumeItem { AuthorResumeItemID = item.AuthorResumeItemID, Title = "Mine now" }, []);

        await hijack.Should().ThrowAsync<InvalidOperationException>();
        (await _context.AuthorResumeItems.AsNoTracking().SingleAsync()).Title.Should().Be("Dev");
    }

    [Fact]
    public async Task SaveResumeItem_Clears_EndDate_When_Current_And_Rejects_Reversed_Dates()
    {
        await SaveProfileAsync(WriterId);

        var current = await _service.SaveResumeItemAsync(WriterId, new AuthorResumeItem
        {
            Title = "Lead", IsCurrent = true, StartDate = new DateOnly(2022, 1, 1), EndDate = new DateOnly(2023, 1, 1),
        }, []);
        current.EndDate.Should().BeNull();

        var reversed = () => _service.SaveResumeItemAsync(WriterId, new AuthorResumeItem
        {
            Title = "Oops", StartDate = new DateOnly(2023, 1, 1), EndDate = new DateOnly(2022, 1, 1),
        }, []);
        await reversed.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteResumeItem_Ignores_Another_Members_Item()
    {
        await SaveProfileAsync(WriterId);
        var item = await _service.SaveResumeItemAsync(WriterId, new AuthorResumeItem { Title = "Dev" },
            [new AuthorResumeItemTranslation { LanguageCode = "fa", Title = "توسعه‌دهنده" }]);

        await _service.DeleteResumeItemAsync(ColleagueId, item.AuthorResumeItemID);
        (await _context.AuthorResumeItems.CountAsync()).Should().Be(1);

        await _service.DeleteResumeItemAsync(WriterId, item.AuthorResumeItemID);
        (await _context.AuthorResumeItems.CountAsync()).Should().Be(0);
        (await _context.AuthorResumeItemTranslations.CountAsync()).Should().Be(0);
    }

    // ── Public page ─────────────────────────────────────────────────

    [Fact]
    public async Task GetPublic_Returns_Null_When_Resume_Disabled_Member_Inactive_Or_Other_Site()
    {
        var disabled = await SaveProfileAsync(WriterId, p => p.ResumeEnabled = false);
        var inactive = await SaveProfileAsync(InactiveId);

        (await _service.GetPublicAsync(SiteId, disabled.Slug)).Should().BeNull();
        (await _service.GetPublicAsync(SiteId, inactive.Slug)).Should().BeNull();

        var enabled = await SaveProfileAsync(WriterId);
        (await _service.GetPublicAsync(OtherSiteId, enabled.Slug)).Should().BeNull();
        (await _service.GetPublicAsync(SiteId, enabled.Slug)).Should().NotBeNull();
    }

    [Fact]
    public async Task GetPublic_Builds_Sections_Timeline_And_Counts_Published_Posts()
    {
        var profile = await SaveProfileAsync(WriterId, p => p.Skills = "C#, SQL");
        await _service.SaveResumeItemAsync(WriterId, new AuthorResumeItem
        {
            ItemType = (byte)ResumeItemType.Education, Title = "B.Sc.", StartDate = new DateOnly(2010, 9, 1),
            EndDate = new DateOnly(2014, 6, 1), ShowInTimeline = true, IsActive = true,
        }, []);
        await _service.SaveResumeItemAsync(WriterId, new AuthorResumeItem
        {
            ItemType = (byte)ResumeItemType.Experience, Title = "Junior dev", StartDate = new DateOnly(2014, 7, 1),
            EndDate = new DateOnly(2018, 1, 1), ShowInTimeline = true, IsActive = true,
        }, []);
        await _service.SaveResumeItemAsync(WriterId, new AuthorResumeItem
        {
            ItemType = (byte)ResumeItemType.Experience, Title = "Lead dev", StartDate = new DateOnly(2018, 2, 1),
            IsCurrent = true, ShowInTimeline = true, IsActive = true,
        }, [new AuthorResumeItemTranslation { LanguageCode = "fa", Title = "سرپرست توسعه" }]);
        await _service.SaveResumeItemAsync(WriterId, new AuthorResumeItem
        {
            ItemType = (byte)ResumeItemType.Project, Title = "Side project", ShowInTimeline = false, IsActive = true,
        }, []);
        await _service.SaveResumeItemAsync(WriterId, new AuthorResumeItem
        {
            ItemType = (byte)ResumeItemType.Award, Title = "Hidden", ShowInTimeline = true, IsActive = false,
        }, []);

        var page = await _service.GetPublicAsync(SiteId, profile.Slug, "fa");

        page.Should().NotBeNull();
        page!.PostCount.Should().Be(2, "the draft is not counted");
        page.Skills.Should().Equal("C#", "SQL");
        page.Sections.Select(s => s.Type).Should().Equal("experience", "education", "project");
        page.Timeline.Select(i => i.Title).Should().Equal("سرپرست توسعه", "Junior dev", "B.Sc.");
        page.Timeline[0].IsCurrent.Should().BeTrue();
    }

    [Fact]
    public async Task GetAuthors_Lists_Only_That_Websites_Members()
    {
        await SaveProfileAsync(WriterId);

        var rows = await _service.GetAuthorsAsync(SiteId);

        rows.Select(r => r.MemberID).Should().BeEquivalentTo([WriterId, ColleagueId, InactiveId]);
        var writer = rows.Single(r => r.MemberID == WriterId);
        writer.HasProfile.Should().BeTrue();
        writer.HasBio.Should().BeTrue();
        writer.PostCount.Should().Be(3);
    }

    // ── Author box on posts ─────────────────────────────────────────

    [Fact]
    public async Task Post_Carries_Author_Card_Respecting_Bio_And_Page_Switches()
    {
        await SaveProfileAsync(WriterId, p =>
        {
            p.DisplayName = "Sara A.";
            p.Headline = "Engineer";
            p.ShowBioOnPosts = false;
            p.ResumeEnabled = true;
        });

        var post = await _posts.GetBySlugAsync(SiteId, "p10");
        post!.Author.Should().NotBeNull();
        post.Author!.Name.Should().Be("Sara A.");
        post.AuthorName.Should().Be("Sara A.");
        post.Author.Headline.Should().Be("Engineer");
        post.Author.Bio.Should().BeNull("the author chose not to show the bio under posts");
        post.Author.Slug.Should().Be("sara-a");

        var noProfile = await _posts.GetBySlugAsync(SiteId, "p13");
        noProfile!.Author!.Name.Should().Be("Ali Ahmadi");
        noProfile.Author.Slug.Should().BeNull();
        noProfile.Author.Bio.Should().BeNull();
    }

    [Fact]
    public async Task Posts_Can_Be_Filtered_By_Author_Slug()
    {
        var profile = await SaveProfileAsync(WriterId);

        var result = await _posts.GetPublishedAsync(SiteId, null, null, null, 1, 10, authorSlug: profile.Slug);

        result.TotalCount.Should().Be(2);
        result.Items.Select(p => p.PostID).Should().BeEquivalentTo([10, 11]);
    }
}
