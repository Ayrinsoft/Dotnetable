using System;
using System.Collections.Generic;

namespace Dotnetable.Domain.Entities;

/// <summary>
/// Public "about the author" data of an admin <see cref="Member"/> who writes content: the short bio
/// shown under their posts and the online résumé page (<c>/author/{Slug}</c>). One row per member,
/// created the first time the member saves their author profile.
/// </summary>
public partial class AuthorProfile
{
    public int AuthorProfileID { get; set; }

    public int MemberID { get; set; }

    public int WebsiteID { get; set; }

    /// <summary>URL segment of the public author page, unique per website.</summary>
    public string Slug { get; set; } = null!;

    /// <summary>Name shown to visitors; falls back to the member's given name + surname when blank.</summary>
    public string? DisplayName { get; set; }

    /// <summary>One-line professional title, e.g. "Senior .NET developer".</summary>
    public string? Headline { get; set; }

    /// <summary>Short plain-text bio rendered in the author box under posts.</summary>
    public string? Bio { get; set; }

    /// <summary>When false the author box under posts shows only the name — no bio.</summary>
    public bool ShowBioOnPosts { get; set; }

    /// <summary>When false the public author page (résumé + timeline) returns 404.</summary>
    public bool ResumeEnabled { get; set; }

    /// <summary>Long rich-text "about me" (HTML) at the top of the résumé page. Sanitised on render.</summary>
    public string? About { get; set; }

    public string? Location { get; set; }

    public string? PublicEmail { get; set; }

    public string? WebsiteUrl { get; set; }

    /// <summary>JSON array of <c>{ "network": "...", "url": "..." }</c> objects.</summary>
    public string? SocialLinksJson { get; set; }

    /// <summary>Comma-separated skill names.</summary>
    public string? Skills { get; set; }

    /// <summary>Author photo; falls back to <see cref="Member.Avatar"/> when null.</summary>
    public int? PhotoFileID { get; set; }

    /// <summary>Optional downloadable CV (e.g. a PDF) linked from the résumé page.</summary>
    public int? ResumeFileID { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Member Member { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;

    public virtual FileRecord? PhotoFile { get; set; }

    public virtual FileRecord? ResumeFile { get; set; }

    public virtual ICollection<AuthorProfileTranslation> AuthorProfileTranslations { get; set; } = new List<AuthorProfileTranslation>();

    public virtual ICollection<AuthorResumeItem> AuthorResumeItems { get; set; } = new List<AuthorResumeItem>();
}
