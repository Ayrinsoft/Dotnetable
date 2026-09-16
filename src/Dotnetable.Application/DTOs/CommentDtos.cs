namespace Dotnetable.Application.DTOs;

/// <summary>What a visitor comment is attached to.</summary>
public enum CommentTarget : byte
{
    Post = 1,
    Page = 2,
}

/// <summary>An approved comment as the public site renders it, with its approved replies nested.
/// Deliberately carries no email, IP or customer id.</summary>
public sealed class CommentDto
{
    public int CommentID { get; init; }
    public int? ParentCommentID { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    /// <summary>True when the comment was written by site staff from the admin panel.</summary>
    public bool IsStaff { get; init; }
    public string Body { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public IReadOnlyList<CommentDto> Replies { get; init; } = Array.Empty<CommentDto>();
}

/// <summary>Body of <c>POST api/posts/{id}/comments</c> and <c>POST api/pages/{id}/comments</c>.</summary>
public sealed class CommentSubmitRequest
{
    public string Body { get; set; } = string.Empty;

    /// <summary>Approved comment on the same post/page this one replies to.</summary>
    public int? ParentCommentId { get; set; }

    /// <summary>Required for guests; ignored for a signed-in customer (taken from the profile).</summary>
    public string? AuthorName { get; set; }

    /// <summary>Optional for guests; never shown publicly. Ignored for a signed-in customer.</summary>
    public string? AuthorEmail { get; set; }

    /// <summary>Turnstile or math-captcha token — required for guests when the site has captcha on.</summary>
    public string? CaptchaToken { get; set; }

    public string? CaptchaAnswer { get; set; }

    /// <summary>Honeypot: must stay empty.</summary>
    public string? Website { get; set; }
}

/// <summary>Everything the service needs to store a visitor comment; built by the API from the
/// request plus the resolved website, caller and connection.</summary>
public sealed record CommentSubmission(
    int WebsiteId,
    CommentTarget Target,
    int TargetId,
    int? ParentCommentId,
    int? WebsiteClientId,
    string? AuthorName,
    string? AuthorEmail,
    string Body,
    string? IpAddress,
    string? UserAgent);

/// <summary>Outcome of a comment submission. <see cref="NotFound"/> means the post/page is missing,
/// unpublished, or has comments turned off.</summary>
public sealed record CommentSubmitResult(bool Success, int? CommentId, string? Error, bool NotFound = false)
{
    public static CommentSubmitResult Ok(int id) => new(true, id, null);
    public static CommentSubmitResult Invalid(string error) => new(false, null, error);
    public static CommentSubmitResult Missing(string error) => new(false, null, error, NotFound: true);
}

/// <summary>Admin moderation list filter.</summary>
public sealed class CommentModerationFilter
{
    /// <summary>A <c>ModerationStatus</c> value; null lists every status.</summary>
    public byte? Status { get; set; }
    public CommentTarget? Target { get; set; }
    public int? PostId { get; set; }
    public int? PageId { get; set; }
    /// <summary>Matches body, author name or email.</summary>
    public string? Search { get; set; }
}
