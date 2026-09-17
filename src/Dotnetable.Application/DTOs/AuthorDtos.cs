using System.Text.Json;

namespace Dotnetable.Application.DTOs;

// ── Public read projections (consumed by the website through the API) ──────────

/// <summary>
/// The author box rendered under a post. <see cref="Bio"/> is null when the author has not written
/// one or has chosen not to show it on posts; <see cref="Slug"/> is null when the author has no
/// public résumé page, so the theme must not link to one.
/// </summary>
public sealed class AuthorCardDto
{
    public string Name { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public string? Headline { get; init; }
    public string? Bio { get; init; }
    public string? PhotoUrl { get; init; }
}

/// <summary>A public author page: bio, résumé sections and the chronological timeline.</summary>
public sealed class AuthorPageDto
{
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Headline { get; init; }
    public string? Bio { get; init; }
    /// <summary>Rich-text "about me" (HTML). Not sanitised by the API — render it through a sanitiser.</summary>
    public string? About { get; init; }
    public string? Location { get; init; }
    public string? PublicEmail { get; init; }
    public string? WebsiteUrl { get; init; }
    public string? PhotoUrl { get; init; }
    /// <summary>Downloadable CV file, when the author uploaded one.</summary>
    public string? ResumeFileUrl { get; init; }
    public IReadOnlyList<string> Skills { get; init; } = Array.Empty<string>();
    public IReadOnlyList<AuthorSocialLink> SocialLinks { get; init; } = Array.Empty<AuthorSocialLink>();
    /// <summary>Résumé items grouped by type, in the fixed section order (experience, education, projects…).</summary>
    public IReadOnlyList<ResumeSectionDto> Sections { get; init; } = Array.Empty<ResumeSectionDto>();
    /// <summary>Items flagged for the timeline, newest first (ongoing items on top).</summary>
    public IReadOnlyList<ResumeItemDto> Timeline { get; init; } = Array.Empty<ResumeItemDto>();
    /// <summary>Published posts written by this author.</summary>
    public int PostCount { get; init; }
}

/// <summary>One résumé section (all items of one <c>ResumeItemType</c>).</summary>
public sealed class ResumeSectionDto
{
    /// <summary>The <c>ResumeItemType</c> name in camel case: <c>experience</c>, <c>education</c>, <c>project</c>…</summary>
    public string Type { get; init; } = string.Empty;
    public IReadOnlyList<ResumeItemDto> Items { get; init; } = Array.Empty<ResumeItemDto>();
}

/// <summary>A single résumé / timeline entry, localized to the requested language.</summary>
public sealed class ResumeItemDto
{
    public int ItemID { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Organization { get; init; }
    public string? Location { get; init; }
    public string? Description { get; init; }
    public string? Url { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public bool IsCurrent { get; init; }
}

/// <summary>A social/profile link on an author page.</summary>
public sealed class AuthorSocialLink
{
    /// <summary>Lower-case network key (<see cref="AuthorSocialLinks.Networks"/>), used by themes to pick an icon.</summary>
    public string Network { get; set; } = "website";
    public string Url { get; set; } = string.Empty;
}

/// <summary>Serialisation and validation of <c>AuthorProfile.SocialLinksJson</c>.</summary>
public static class AuthorSocialLinks
{
    /// <summary>Networks the admin can pick from. Anything else is stored as <c>website</c>.</summary>
    public static readonly IReadOnlyList<string> Networks =
    [
        "website", "linkedin", "github", "x", "instagram", "telegram", "youtube", "facebook",
        "mastodon", "stackoverflow", "dribbble", "behance", "medium", "whatsapp",
    ];

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static List<AuthorSocialLink> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try { return JsonSerializer.Deserialize<List<AuthorSocialLink>>(json, Json) ?? new(); }
        catch (JsonException) { return new(); }
    }

    /// <summary>Drops blank rows and anything that is not an absolute http(s) URL (a <c>javascript:</c>
    /// link would otherwise run on every visitor's browser), and normalises unknown networks.
    /// Returns null when nothing is left.</summary>
    public static string? Serialize(IEnumerable<AuthorSocialLink>? links)
    {
        var clean = (links ?? [])
            .Where(l => IsSafeUrl(l.Url))
            .Select(l => new AuthorSocialLink
            {
                Network = Networks.Contains(l.Network?.Trim().ToLowerInvariant() ?? "") ? l.Network!.Trim().ToLowerInvariant() : "website",
                Url = l.Url.Trim(),
            })
            .ToList();
        return clean.Count == 0 ? null : JsonSerializer.Serialize(clean, Json);
    }

    /// <summary>True for an absolute http/https URL.</summary>
    public static bool IsSafeUrl(string? url) =>
        !string.IsNullOrWhiteSpace(url)
        && Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}

// ── Admin projections ─────────────────────────────────────────────────────────

/// <summary>A row of the admin "Authors" list: every admin member of the website and their profile state.</summary>
public sealed class AuthorListRow
{
    public int MemberID { get; init; }
    public string Username { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public bool Active { get; init; }
    public int WebsiteID { get; init; }
    public bool HasProfile { get; init; }
    public string? Slug { get; init; }
    public bool ResumeEnabled { get; init; }
    public bool HasBio { get; init; }
    public int ResumeItemCount { get; init; }
    public int PostCount { get; init; }
}
