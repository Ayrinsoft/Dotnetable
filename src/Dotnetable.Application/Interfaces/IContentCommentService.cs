using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

/// <summary>
/// Visitor comments on blog posts and CMS pages. Visitor comments are held as
/// <see cref="ModerationStatus.Pending"/> until a moderator approves them; only approved comments are
/// ever returned publicly. Admin-side methods take a nullable <c>websiteId</c>: null means a master
/// admin acting across sites, a value scopes the call so one site's staff cannot touch another's.
/// </summary>
public interface IContentCommentService
{
    public const int BodyMaxLength = 4000;
    public const int NameMaxLength = 150;

    Task<CommentSubmitResult> SubmitAsync(CommentSubmission submission, CancellationToken ct = default);

    /// <summary>Approved top-level comments (newest first, paged) with all their approved replies nested.
    /// Returns an empty page when the target is not published or has comments turned off.</summary>
    Task<PagedResult<CommentDto>> GetApprovedAsync(int websiteId, CommentTarget target, int targetId, GridQuery query, CancellationToken ct = default);

    /// <summary>Moderation list with <c>Post</c>, <c>Page</c>, <c>ParentComment</c>, <c>WebsiteClient</c> and
    /// <c>AuthorMember</c> loaded; pending comments sort oldest first, everything else newest first.</summary>
    Task<PagedResult<ContentComment>> GetForModerationAsync(int? websiteId, CommentModerationFilter filter, GridQuery query, CancellationToken ct = default);

    Task<int> CountPendingAsync(int? websiteId, CancellationToken ct = default);

    Task<bool> ModerateAsync(int? websiteId, int commentId, bool approve, int? memberId, CancellationToken ct = default);

    /// <summary>Publishes a staff reply under <paramref name="parentCommentId"/>. A reply means the parent
    /// was read and accepted, so a still-pending parent is approved along with it.</summary>
    Task<ContentComment?> ReplyAsync(int? websiteId, int parentCommentId, int memberId, string body, CancellationToken ct = default);

    /// <summary>Deletes the comment and every reply beneath it.</summary>
    Task<bool> DeleteAsync(int? websiteId, int commentId, CancellationToken ct = default);
}
