using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dotnetable.Tests.Services;

public class ContentCommentServiceTests : IDisposable
{
    private const int SiteId = 1;
    private const int OtherSiteId = 2;
    private const int OpenPostId = 10;
    private const int ClosedPostId = 11;
    private const int ClosedTypePostId = 12;
    private const int DraftPostId = 13;
    private const int OpenPageId = 20;
    private const int ClosedPageId = 21;
    private const int ClientId = 30;
    private const int MemberId = 40;

    private readonly RelationalTestDb _db;
    private readonly AppDbContext _context;
    private readonly ContentCommentService _service;

    public ContentCommentServiceTests()
    {
        _db = new RelationalTestDb();
        _context = _db.NewContext();
        _service = new ContentCommentService(new TestDbContextFactory(_db.Options));
        Seed();
    }

    public void Dispose()
    {
        _context.Dispose();
        _db.Dispose();
    }

    private void Seed()
    {
        _context.PostTypes.AddRange(
            new PostType { PostTypeID = 1, WebsiteID = SiteId, Name = "Blog", Slug = "blog", CommentsEnabled = true },
            new PostType { PostTypeID = 2, WebsiteID = SiteId, Name = "News", Slug = "news", CommentsEnabled = false });
        _context.Posts.AddRange(
            NewPost(OpenPostId, 1, commentsEnabled: true),
            NewPost(ClosedPostId, 1, commentsEnabled: false),
            NewPost(ClosedTypePostId, 2, commentsEnabled: true),
            NewPost(DraftPostId, 1, commentsEnabled: true, status: 0));
        _context.Pages.AddRange(
            NewPage(OpenPageId, commentsEnabled: true),
            NewPage(ClosedPageId, commentsEnabled: false));
        _context.WebsiteClients.Add(new WebsiteClient
        {
            WebsiteClientID = ClientId, WebsiteID = SiteId, Active = true,
            Givenname = "Sara", Surname = "Ahmadi", Email = "sara@example.com",
        });
        _context.Members.Add(new Member
        {
            MemberID = MemberId, WebsiteID = SiteId, Username = "editor", Password = "x",
            Email = "editor@example.com", CellphoneNumber = "0", CountryCode = "98",
            Givenname = "Site", Surname = "Editor",
        });
        _context.SaveChanges();
        _context.ChangeTracker.Clear();
    }

    private static Post NewPost(int id, int typeId, bool commentsEnabled, byte status = PostService.PublishedStatus) => new()
    {
        PostID = id, WebsiteID = SiteId, PostTypeID = typeId, Slug = $"p{id}", Title = $"Post {id}",
        Status = status, IsActive = true, CommentsEnabled = commentsEnabled,
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
    };

    private static Page NewPage(int id, bool commentsEnabled) => new()
    {
        PageID = id, WebsiteID = SiteId, Slug = $"pg{id}", Title = $"Page {id}",
        Status = 1, IsActive = true, CommentsEnabled = commentsEnabled,
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
    };

    private static CommentSubmission Guest(CommentTarget target, int targetId, string body = "Nice post",
        int? parentId = null, string? name = "Guest", string? email = null, int websiteId = SiteId) =>
        new(websiteId, target, targetId, parentId, null, name, email, body, "127.0.0.1", "tests");

    private async Task<int> ApprovedCommentAsync(CommentTarget target, int targetId, int? parentId = null)
    {
        var result = await _service.SubmitAsync(Guest(target, targetId, parentId: parentId));
        result.Success.Should().BeTrue(result.Error);
        (await _service.ModerateAsync(SiteId, result.CommentId!.Value, approve: true, MemberId)).Should().BeTrue();
        return result.CommentId!.Value;
    }

    private static GridQuery FirstPage => new() { PageIndex = 1, PageSize = 20 };

    [Fact]
    public async Task Guest_Comment_Is_Stored_Pending_And_Hidden_Until_Approved()
    {
        var result = await _service.SubmitAsync(Guest(CommentTarget.Post, OpenPostId, email: "g@example.com"));

        result.Success.Should().BeTrue();
        var stored = await _context.ContentComments.AsNoTracking().SingleAsync();
        stored.Status.Should().Be((byte)ModerationStatus.Pending);
        stored.PostID.Should().Be(OpenPostId);
        stored.PageID.Should().BeNull();
        stored.AuthorEmail.Should().Be("g@example.com");

        (await _service.GetApprovedAsync(SiteId, CommentTarget.Post, OpenPostId, FirstPage)).Items.Should().BeEmpty();

        await _service.ModerateAsync(SiteId, result.CommentId!.Value, approve: true, MemberId);

        var visible = await _service.GetApprovedAsync(SiteId, CommentTarget.Post, OpenPostId, FirstPage);
        visible.TotalCount.Should().Be(1);
        visible.Items.Single().Body.Should().Be("Nice post");
    }

    [Theory]
    [InlineData(CommentTarget.Post, ClosedPostId)]
    [InlineData(CommentTarget.Post, ClosedTypePostId)]
    [InlineData(CommentTarget.Post, DraftPostId)]
    [InlineData(CommentTarget.Post, 999)]
    [InlineData(CommentTarget.Page, ClosedPageId)]
    public async Task Submit_Is_Refused_When_Comments_Are_Not_Available(CommentTarget target, int targetId)
    {
        var result = await _service.SubmitAsync(Guest(target, targetId));

        result.Success.Should().BeFalse();
        result.NotFound.Should().BeTrue();
        (await _context.ContentComments.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Submit_Is_Refused_For_Content_Of_Another_Website()
    {
        var result = await _service.SubmitAsync(Guest(CommentTarget.Page, OpenPageId, websiteId: OtherSiteId));

        result.NotFound.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Guest", null)]
    [InlineData("x", "Guest", null)]
    [InlineData("Valid body", "", null)]
    [InlineData("Valid body", "Guest", "not-an-email")]
    public async Task Invalid_Guest_Input_Is_Rejected(string body, string name, string? email)
    {
        var result = await _service.SubmitAsync(Guest(CommentTarget.Page, OpenPageId, body, name: name, email: email));

        result.Success.Should().BeFalse();
        result.NotFound.Should().BeFalse();
    }

    [Fact]
    public async Task Body_Over_The_Limit_Is_Rejected()
    {
        var body = new string('a', IContentCommentService.BodyMaxLength + 1);

        var result = await _service.SubmitAsync(Guest(CommentTarget.Page, OpenPageId, body));

        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Signed_In_Customer_Name_And_Email_Come_From_The_Profile()
    {
        var submission = new CommentSubmission(SiteId, CommentTarget.Post, OpenPostId, null, ClientId,
            "Spoofed name", "spoof@example.com", "Hello", null, null);

        var result = await _service.SubmitAsync(submission);

        result.Success.Should().BeTrue();
        var stored = await _context.ContentComments.AsNoTracking().SingleAsync();
        stored.AuthorName.Should().Be("Sara Ahmadi");
        stored.AuthorEmail.Should().Be("sara@example.com");
        stored.WebsiteClientID.Should().Be(ClientId);
    }

    [Fact]
    public async Task Reply_Must_Target_An_Approved_Comment_Of_The_Same_Content()
    {
        var pending = await _service.SubmitAsync(Guest(CommentTarget.Post, OpenPostId));
        var approvedOnPage = await ApprovedCommentAsync(CommentTarget.Page, OpenPageId);

        (await _service.SubmitAsync(Guest(CommentTarget.Post, OpenPostId, parentId: pending.CommentId))).Success
            .Should().BeFalse("replying to a pending comment would reveal it");
        (await _service.SubmitAsync(Guest(CommentTarget.Post, OpenPostId, parentId: approvedOnPage))).Success
            .Should().BeFalse("the parent belongs to a different page");
    }

    [Fact]
    public async Task Approved_Replies_Are_Nested_Under_Their_Parent()
    {
        var root = await ApprovedCommentAsync(CommentTarget.Post, OpenPostId);
        var reply = await ApprovedCommentAsync(CommentTarget.Post, OpenPostId, parentId: root);
        await ApprovedCommentAsync(CommentTarget.Post, OpenPostId, parentId: reply);
        await _service.SubmitAsync(Guest(CommentTarget.Post, OpenPostId, parentId: root)); // pending, hidden

        var result = await _service.GetApprovedAsync(SiteId, CommentTarget.Post, OpenPostId, FirstPage);

        result.TotalCount.Should().Be(1, "only top-level comments are counted for paging");
        var top = result.Items.Single();
        top.CommentID.Should().Be(root);
        top.Replies.Should().ContainSingle().Which.CommentID.Should().Be(reply);
        top.Replies[0].Replies.Should().ContainSingle();
    }

    [Fact]
    public async Task Public_List_Is_Empty_Once_Comments_Are_Switched_Off()
    {
        await ApprovedCommentAsync(CommentTarget.Page, OpenPageId);
        await _context.Pages.Where(p => p.PageID == OpenPageId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.CommentsEnabled, false));

        (await _service.GetApprovedAsync(SiteId, CommentTarget.Page, OpenPageId, FirstPage)).Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Staff_Reply_Is_Published_And_Approves_A_Pending_Parent()
    {
        var pending = await _service.SubmitAsync(Guest(CommentTarget.Post, OpenPostId));

        var reply = await _service.ReplyAsync(SiteId, pending.CommentId!.Value, MemberId, "Thanks for reading");

        reply.Should().NotBeNull();
        reply!.Status.Should().Be((byte)ModerationStatus.Approved);
        reply.AuthorName.Should().Be("Site Editor");
        var list = await _service.GetApprovedAsync(SiteId, CommentTarget.Post, OpenPostId, FirstPage);
        var top = list.Items.Single();
        top.Replies.Should().ContainSingle().Which.IsStaff.Should().BeTrue();
        (await _service.CountPendingAsync(SiteId)).Should().Be(0);
    }

    [Fact]
    public async Task Moderation_Is_Scoped_To_The_Callers_Website()
    {
        var pending = await _service.SubmitAsync(Guest(CommentTarget.Post, OpenPostId));
        var id = pending.CommentId!.Value;

        (await _service.ModerateAsync(OtherSiteId, id, approve: true, MemberId)).Should().BeFalse();
        (await _service.DeleteAsync(OtherSiteId, id)).Should().BeFalse();
        (await _service.ReplyAsync(OtherSiteId, id, MemberId, "Hi there")).Should().BeNull();
        (await _service.GetForModerationAsync(OtherSiteId, new CommentModerationFilter(), FirstPage)).TotalCount.Should().Be(0);

        (await _service.GetForModerationAsync(null, new CommentModerationFilter(), FirstPage)).TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Moderation_List_Filters_By_Status_Target_And_Search()
    {
        await ApprovedCommentAsync(CommentTarget.Page, OpenPageId);
        await _service.SubmitAsync(Guest(CommentTarget.Post, OpenPostId, body: "Pending about pizza", name: "Reza"));

        var pending = await _service.GetForModerationAsync(SiteId,
            new CommentModerationFilter { Status = (byte)ModerationStatus.Pending }, FirstPage);
        pending.Items.Should().ContainSingle().Which.Post!.Title.Should().Be($"Post {OpenPostId}");

        (await _service.GetForModerationAsync(SiteId, new CommentModerationFilter { Target = CommentTarget.Page }, FirstPage))
            .Items.Should().ContainSingle().Which.PageID.Should().Be(OpenPageId);
        (await _service.GetForModerationAsync(SiteId, new CommentModerationFilter { Search = "pizza" }, FirstPage))
            .TotalCount.Should().Be(1);
        (await _service.GetForModerationAsync(SiteId, new CommentModerationFilter { Search = "Reza" }, FirstPage))
            .TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Delete_Removes_The_Whole_Reply_Subtree_Only()
    {
        var root = await ApprovedCommentAsync(CommentTarget.Post, OpenPostId);
        var reply = await ApprovedCommentAsync(CommentTarget.Post, OpenPostId, parentId: root);
        await ApprovedCommentAsync(CommentTarget.Post, OpenPostId, parentId: reply);
        var survivor = await ApprovedCommentAsync(CommentTarget.Post, OpenPostId);

        (await _service.DeleteAsync(SiteId, root)).Should().BeTrue();

        var remaining = await _context.ContentComments.AsNoTracking().Select(c => c.ContentCommentID).ToListAsync();
        remaining.Should().Equal(survivor);
    }

    [Fact]
    public async Task Rejecting_Hides_A_Previously_Approved_Comment()
    {
        var id = await ApprovedCommentAsync(CommentTarget.Page, OpenPageId);

        await _service.ModerateAsync(SiteId, id, approve: false, MemberId);

        (await _service.GetApprovedAsync(SiteId, CommentTarget.Page, OpenPageId, FirstPage)).Items.Should().BeEmpty();
        var stored = await _context.ContentComments.AsNoTracking().SingleAsync();
        stored.Status.Should().Be((byte)ModerationStatus.Rejected);
        stored.ModeratedByMemberID.Should().Be(MemberId);
    }
}
