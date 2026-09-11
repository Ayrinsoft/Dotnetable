namespace Dotnetable.Application.DTOs;

// ── Public read projections (consumed by the website through the API) ──────────

/// <summary>A blog/content post in a list (card) context — localized to the requested language.</summary>
public class PostSummaryDto
{
    public int PostID { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Excerpt { get; init; }
    public string? FeaturedImageUrl { get; init; }
    public string PostTypeSlug { get; init; } = string.Empty;
    public string? AuthorName { get; init; }
    public bool IsFeatured { get; init; }
    public int ViewCount { get; init; }
    public DateTime? PublishedAt { get; init; }
    public IReadOnlyList<CategoryDto> Categories { get; init; } = Array.Empty<CategoryDto>();
    public IReadOnlyList<TagDto> Tags { get; init; } = Array.Empty<TagDto>();
}

/// <summary>A single post with its full (localized) body.</summary>
public sealed class PostDetailDto : PostSummaryDto
{
    public string? Content { get; init; }
    public bool CommentsEnabled { get; init; }
    /// <summary>Admin override for the browser-tab/search-result title; falls back to <see cref="PostSummaryDto.Title"/>.</summary>
    public string? MetaTitle { get; init; }
    /// <summary>Admin override for the search-result/social-preview description; falls back to <see cref="PostSummaryDto.Excerpt"/>.</summary>
    public string? MetaDescription { get; init; }
    public string? MetaKeywords { get; init; }
}

/// <summary>A CMS page projected for public rendering, localized when a translation exists.</summary>
public sealed class PageDto
{
    public int PageID { get; init; }
    public int? ParentPageID { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Content { get; init; }
    public string? Template { get; init; }
    public bool IsHomepage { get; init; }
    public int SortOrder { get; init; }
    /// <summary>Admin override for the browser-tab/search-result title; falls back to <see cref="Title"/>.</summary>
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string? MetaKeywords { get; init; }
    public IReadOnlyList<PageDto> Children { get; init; } = Array.Empty<PageDto>();
}

/// <summary>A category node (may carry nested children) localized to the requested language.</summary>
public sealed class CategoryDto
{
    public int CategoryID { get; init; }
    public int? ParentCategoryID { get; init; }
    public int? PostTypeID { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public IReadOnlyList<CategoryDto> Children { get; init; } = Array.Empty<CategoryDto>();
}

/// <summary>A tag localized to the requested language.</summary>
public sealed class TagDto
{
    public int TagID { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}

/// <summary>Resolved redirect target for a requested source path.</summary>
public sealed class RedirectResultDto
{
    public string TargetPath { get; init; } = string.Empty;
    public int StatusCode { get; init; }
}

// ── Admin filters ─────────────────────────────────────────────────────────────

/// <summary>Optional filters applied to the admin post listing.</summary>
public sealed class PostFilter
{
    public int? PostTypeID { get; set; }
    public int? CategoryID { get; set; }
    public byte? Status { get; set; }
}
