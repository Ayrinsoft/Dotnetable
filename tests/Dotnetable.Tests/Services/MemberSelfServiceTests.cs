using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

/// <summary>The "My account" writes a member makes to their own row.</summary>
public class MemberSelfServiceTests : IDisposable
{
    private const int SiteId = 1;
    private const int OtherSiteId = 2;
    private const int MemberId = 40;
    private const string Password = "Correct-Horse-9!";

    private readonly RelationalTestDb _db;
    private readonly AppDbContext _context;
    private readonly MemberService _service;
    private readonly PasswordHasher<Member> _hasher = new();

    public MemberSelfServiceTests()
    {
        _db = new RelationalTestDb();
        _context = _db.NewContext();
        _service = new MemberService(new TestDbContextFactory(_db.Options), _hasher);

        var member = new Member
        {
            MemberID = MemberId, WebsiteID = SiteId, PolicyID = 7, Active = true, Username = "writer",
            Email = "writer@example.com", CellphoneNumber = "0", CountryCode = "98",
            Givenname = "Old", Surname = "Name",
        };
        member.Password = _hasher.HashPassword(member, Password);
        _context.Members.Add(member);
        _context.FileRecords.AddRange(
            NewFile(1, SiteId, FileCategory.Image),
            NewFile(2, OtherSiteId, FileCategory.Image),
            NewFile(3, SiteId, FileCategory.Document),
            NewFile(4, SiteId, FileCategory.Image, deleted: true));
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
    }

    public void Dispose()
    {
        _context.Dispose();
        _db.Dispose();
    }

    private static FileRecord NewFile(int id, int websiteId, FileCategory category, bool deleted = false) => new()
    {
        FileRecordID = id, WebsiteID = websiteId, FileCategory = (byte)category, IsDeleted = deleted,
        OriginalFileName = $"f{id}", StoredFileName = $"f{id}", MimeType = "image/png",
        CNDUrl = $"https://cdn/f{id}.png", ThumbnailCDN = $"https://cdn/t{id}.png", UploadDate = DateTime.UtcNow,
    };

    [Fact]
    public async Task UpdateOwnProfile_Changes_Only_Personal_Fields()
    {
        await _service.UpdateOwnProfileAsync(MemberId, " Sara ", "Ahmadi", "1", "5550100", false);

        var m = await _context.Members.AsNoTracking().SingleAsync();
        (m.Givenname, m.Surname, m.CountryCode, m.CellphoneNumber, m.Gender).Should().Be(("Sara", "Ahmadi", "1", "5550100", (bool?)false));
        (m.PolicyID, m.Active, m.Username, m.WebsiteID).Should().Be((7, true, "writer", SiteId));
    }

    [Fact]
    public async Task UpdateOwnProfile_Requires_A_Name()
    {
        var act = () => _service.UpdateOwnProfileAsync(MemberId, " ", "Ahmadi", null, null, null);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task SetAvatar_Accepts_Own_Image_And_Returns_Thumbnail_Url()
    {
        await _service.SetAvatarAsync(MemberId, 1);

        (await _service.GetAvatarUrlAsync(MemberId)).Should().Be("https://cdn/t1.png");

        await _service.SetAvatarAsync(MemberId, null);
        (await _service.GetAvatarUrlAsync(MemberId)).Should().BeNull();
    }

    [Theory]
    [InlineData(2)] // another website's file
    [InlineData(3)] // not an image
    [InlineData(4)] // deleted
    [InlineData(99)] // missing
    public async Task SetAvatar_Rejects_Files_That_Cannot_Be_A_Public_Avatar(int fileId)
    {
        var act = () => _service.SetAvatarAsync(MemberId, fileId);

        await act.Should().ThrowAsync<InvalidOperationException>();
        (await _context.Members.AsNoTracking().SingleAsync()).AvatarID.Should().BeNull();
    }

    [Fact]
    public async Task ChangeOwnPassword_Requires_The_Current_Password()
    {
        (await _service.ChangeOwnPasswordAsync(MemberId, "wrong", "Another-Strong-8!")).Should().BeFalse();
        (await _service.ChangeOwnPasswordAsync(MemberId, Password, "Another-Strong-8!")).Should().BeTrue();

        var m = await _context.Members.AsNoTracking().SingleAsync();
        _hasher.VerifyHashedPassword(m, m.Password, "Another-Strong-8!").Should().NotBe(PasswordVerificationResult.Failed);
    }
}
