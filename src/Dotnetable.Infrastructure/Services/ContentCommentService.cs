using System.Net.Mail;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ContentCommentService : IContentCommentService
{
    private const int MinBodyLength = 2;
    private const int EmailMaxLength = 256;
    private const int IpMaxLength = 64;
    private const int UserAgentMaxLength = 512;

    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public ContentCommentService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<CommentSubmitResult> SubmitAsync(CommentSubmission submission, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        if (!await AcceptsCommentsAsync(_context, submission.WebsiteId, submission.Target, submission.TargetId, ct))
            return CommentSubmitResult.Missing("Comments are not available for this content.");

        var body = submission.Body?.Trim() ?? string.Empty;
        if (body.Length < MinBodyLength)
            return CommentSubmitResult.Invalid("Comment text is required.");
        if (body.Length > IContentCommentService.BodyMaxLength)
            return CommentSubmitResult.Invalid($"Comment must be at most {IContentCommentService.BodyMaxLength} characters.");

        if (submission.ParentCommentId is int parentId)
        {
            var parentOk = await ForTarget(_context.ContentComments, submission.Target, submission.TargetId)
                .AnyAsync(c => c.ContentCommentID == parentId && c.WebsiteID == submission.WebsiteId
                               && c.Status == (byte)ModerationStatus.Approved, ct);
            if (!parentOk)
                return CommentSubmitResult.Invalid("The comment you are replying to does not exist.");
        }

        string name;
        string? email;
        if (submission.WebsiteClientId is int clientId)
        {
            var client = await _context.WebsiteClients.AsNoTracking()
                .FirstOrDefaultAsync(c => c.WebsiteClientID == clientId && c.WebsiteID == submission.WebsiteId && c.Active, ct);
            if (client is null)
                return CommentSubmitResult.Invalid("Your account could not be found.");

            name = $"{client.Givenname} {client.Surname}".Trim();
            if (name.Length == 0) name = submission.AuthorName?.Trim() ?? string.Empty;
            if (name.Length == 0) name = "Customer";
            email = client.Email;
        }
        else
        {
            name = submission.AuthorName?.Trim() ?? string.Empty;
            if (name.Length == 0)
                return CommentSubmitResult.Invalid("Name is required.");

            email = string.IsNullOrWhiteSpace(submission.AuthorEmail) ? null : submission.AuthorEmail.Trim();
            if (email is not null && (email.Length > EmailMaxLength || !MailAddress.TryCreate(email, out _)))
                return CommentSubmitResult.Invalid("Email address is not valid.");
        }
        if (name.Length > IContentCommentService.NameMaxLength)
            name = name[..IContentCommentService.NameMaxLength];

        var comment = new ContentComment
        {
            WebsiteID = submission.WebsiteId,
            PostID = submission.Target == CommentTarget.Post ? submission.TargetId : null,
            PageID = submission.Target == CommentTarget.Page ? submission.TargetId : null,
            ParentCommentID = submission.ParentCommentId,
            WebsiteClientID = submission.WebsiteClientId,
            AuthorName = name,
            AuthorEmail = email,
            Body = body,
            Status = (byte)ModerationStatus.Pending,
            IpAddress = Truncate(submission.IpAddress, IpMaxLength),
            UserAgent = Truncate(submission.UserAgent, UserAgentMaxLength),
            CreatedAt = DateTime.UtcNow,
        };
        _context.ContentComments.Add(comment);
        await _context.SaveChangesAsync(ct);
        return CommentSubmitResult.Ok(comment.ContentCommentID);
    }

    public async Task<PagedResult<CommentDto>> GetApprovedAsync(
        int websiteId, CommentTarget target, int targetId, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        if (!await AcceptsCommentsAsync(_context, websiteId, target, targetId, ct))
            return new PagedResult<CommentDto>();

        var approved = ForTarget(_context.ContentComments.AsNoTracking(), target, targetId)
            .Where(c => c.WebsiteID == websiteId && c.Status == (byte)ModerationStatus.Approved)
            // Staff replies show the writer's admin avatar.
            .Include(c => c.AuthorMember).ThenInclude(m => m!.Avatar);

        var roots = approved.Where(c => c.ParentCommentID == null);
        var total = await roots.CountAsync(ct);
        var page = await roots.OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.ContentCommentID)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);
        if (page.Count == 0)
            return new PagedResult<CommentDto> { TotalCount = total };

        // Replies are few relative to roots, so load every approved reply of this target once and
        // stitch the tree in memory rather than issuing a query per level.
        var replies = await approved.Where(c => c.ParentCommentID != null)
            .OrderBy(c => c.CreatedAt).ThenBy(c => c.ContentCommentID)
            .ToListAsync(ct);
        var byParent = replies.ToLookup(c => c.ParentCommentID!.Value);

        return new PagedResult<CommentDto>
        {
            Items = page.Select(c => Project(c, byParent, depth: 0)).ToList(),
            TotalCount = total,
        };
    }

    public async Task<PagedResult<ContentComment>> GetForModerationAsync(
        int? websiteId, CommentModerationFilter filter, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.ContentComments.AsNoTracking()
            .Include(c => c.Post)
            .Include(c => c.Page)
            .Include(c => c.ParentComment)
            .Include(c => c.WebsiteClient)
            .Include(c => c.AuthorMember)
            .AsQueryable();

        if (websiteId is int wid) q = q.Where(c => c.WebsiteID == wid);
        if (filter.Status is byte status) q = q.Where(c => c.Status == status);
        if (filter.Target == CommentTarget.Post) q = q.Where(c => c.PostID != null);
        if (filter.Target == CommentTarget.Page) q = q.Where(c => c.PageID != null);
        if (filter.PostId is int postId) q = q.Where(c => c.PostID == postId);
        if (filter.PageId is int pageId) q = q.Where(c => c.PageID == pageId);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim();
            q = q.Where(c => c.Body.Contains(s) || c.AuthorName.Contains(s) || (c.AuthorEmail != null && c.AuthorEmail.Contains(s)));
        }

        var total = await q.CountAsync(ct);
        // The pending queue is worked oldest-first; history reads newest-first.
        var ordered = filter.Status == (byte)ModerationStatus.Pending
            ? q.OrderBy(c => c.CreatedAt).ThenBy(c => c.ContentCommentID)
            : q.OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.ContentCommentID);
        var items = await ordered.Skip(query.Skip).Take(query.Take).ToListAsync(ct);
        return new PagedResult<ContentComment> { Items = items, TotalCount = total };
    }

    public async Task<int> CountPendingAsync(int? websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.ContentComments.Where(c => c.Status == (byte)ModerationStatus.Pending);
        if (websiteId is int wid) q = q.Where(c => c.WebsiteID == wid);
        return await q.CountAsync(ct);
    }

    public async Task<bool> ModerateAsync(int? websiteId, int commentId, bool approve, int? memberId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var comment = await FindAsync(_context, websiteId, commentId, ct);
        if (comment is null) return false;

        SetStatus(comment, approve ? ModerationStatus.Approved : ModerationStatus.Rejected, memberId);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ContentComment?> ReplyAsync(int? websiteId, int parentCommentId, int memberId, string body, CancellationToken ct = default)
    {
        body = body?.Trim() ?? string.Empty;
        if (body.Length < MinBodyLength || body.Length > IContentCommentService.BodyMaxLength)
            return null;

        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var parent = await FindAsync(_context, websiteId, parentCommentId, ct);
        if (parent is null) return null;

        var member = await _context.Members.AsNoTracking().FirstOrDefaultAsync(m => m.MemberID == memberId, ct);
        if (member is null) return null;

        if (parent.Status != (byte)ModerationStatus.Approved)
            SetStatus(parent, ModerationStatus.Approved, memberId);

        var name = $"{member.Givenname} {member.Surname}".Trim();
        if (name.Length == 0) name = member.Username;

        var now = DateTime.UtcNow;
        var reply = new ContentComment
        {
            WebsiteID = parent.WebsiteID,
            PostID = parent.PostID,
            PageID = parent.PageID,
            ParentCommentID = parent.ContentCommentID,
            AuthorMemberID = memberId,
            AuthorName = name.Length > IContentCommentService.NameMaxLength ? name[..IContentCommentService.NameMaxLength] : name,
            AuthorEmail = member.Email,
            Body = body,
            Status = (byte)ModerationStatus.Approved,
            CreatedAt = now,
            ModeratedAt = now,
            ModeratedByMemberID = memberId,
        };
        _context.ContentComments.Add(reply);
        await _context.SaveChangesAsync(ct);
        return reply;
    }

    public async Task<bool> DeleteAsync(int? websiteId, int commentId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var comment = await FindAsync(_context, websiteId, commentId, ct);
        if (comment is null) return false;

        // Gather the whole subtree: every comment of the same post/page whose ancestor chain reaches it.
        var siblings = await _context.ContentComments
            .Where(c => c.PostID == comment.PostID && c.PageID == comment.PageID && c.WebsiteID == comment.WebsiteID)
            .ToListAsync(ct);
        var byParent = siblings.Where(c => c.ParentCommentID != null).ToLookup(c => c.ParentCommentID!.Value);

        var doomed = new List<ContentComment>();
        var stack = new Stack<ContentComment>();
        stack.Push(comment);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            doomed.Add(current);
            foreach (var child in byParent[current.ContentCommentID])
                stack.Push(child);
        }

        _context.ContentComments.RemoveRange(doomed);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    // ── Helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// True when the post/page belongs to the site, is publicly visible and has comments switched on.
    /// For a post both the post's own switch and its post type's switch must be on, so an admin can
    /// turn comments off for a whole type (e.g. "News") without editing every post.
    /// </summary>
    private static Task<bool> AcceptsCommentsAsync(AppDbContext _context, int websiteId, CommentTarget target, int targetId, CancellationToken ct)
    {
        if (target == CommentTarget.Post)
        {
            var now = DateTime.UtcNow;
            return _context.Posts.AnyAsync(p => p.PostID == targetId && p.WebsiteID == websiteId && p.IsActive
                && p.Status == PostService.PublishedStatus && (p.PublishedAt == null || p.PublishedAt <= now)
                && p.CommentsEnabled && p.PostType.CommentsEnabled, ct);
        }

        return _context.Pages.AnyAsync(p => p.PageID == targetId && p.WebsiteID == websiteId && p.IsActive
            && p.Status == 1 && p.CommentsEnabled, ct);
    }

    private static IQueryable<ContentComment> ForTarget(IQueryable<ContentComment> q, CommentTarget target, int targetId) =>
        target == CommentTarget.Post
            ? q.Where(c => c.PostID == targetId)
            : q.Where(c => c.PageID == targetId);

    private static Task<ContentComment?> FindAsync(AppDbContext _context, int? websiteId, int commentId, CancellationToken ct) =>
        _context.ContentComments.FirstOrDefaultAsync(
            c => c.ContentCommentID == commentId && (websiteId == null || c.WebsiteID == websiteId), ct);

    private static void SetStatus(ContentComment comment, ModerationStatus status, int? memberId)
    {
        comment.Status = (byte)status;
        comment.ModeratedAt = DateTime.UtcNow;
        comment.ModeratedByMemberID = memberId;
    }

    // Guards against a pathological reply chain blowing the stack; deeper replies are still stored,
    // just not rendered below this depth.
    private const int MaxReplyDepth = 20;

    private static CommentDto Project(ContentComment c, ILookup<int, ContentComment> byParent, int depth) => new()
    {
        CommentID = c.ContentCommentID,
        ParentCommentID = c.ParentCommentID,
        AuthorName = c.AuthorName,
        IsStaff = c.AuthorMemberID != null,
        AuthorAvatarUrl = c.AuthorMember?.Avatar is { IsDeleted: false } avatar ? avatar.ThumbnailCDN ?? avatar.CNDUrl : null,
        Body = c.Body,
        CreatedAt = c.CreatedAt,
        Replies = depth >= MaxReplyDepth
            ? Array.Empty<CommentDto>()
            : byParent[c.ContentCommentID].Select(r => Project(r, byParent, depth + 1)).ToList(),
    };

    private static string? Truncate(string? value, int max) =>
        string.IsNullOrEmpty(value) ? null : value.Length <= max ? value : value[..max];
}
