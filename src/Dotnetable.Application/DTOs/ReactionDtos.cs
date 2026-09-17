using Dotnetable.Domain.Enums;

namespace Dotnetable.Application.DTOs;

/// <summary>
/// Like/favorite and bookmark state of one product, post or page. <see cref="Liked"/> and
/// <see cref="Bookmarked"/> are always false for an anonymous caller.
/// </summary>
public sealed class ReactionStateDto
{
    /// <summary><c>product</c>, <c>post</c> or <c>page</c>.</summary>
    public string TargetType { get; init; } = string.Empty;
    public int TargetId { get; init; }
    /// <summary>Clients who liked the post/page, or marked the product as a favorite — each counted once.</summary>
    public int LikeCount { get; init; }
    public bool Liked { get; init; }
    public bool Bookmarked { get; init; }
}

/// <summary>A row of the signed-in customer's bookmark (or favorites) list, localized.</summary>
public sealed class ClientReactionItemDto
{
    public string TargetType { get; init; } = string.Empty;
    public int TargetId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string? Excerpt { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>Parsing of the lower-case target names used in public URLs.</summary>
public static class ReactionTargets
{
    public static bool TryParse(string? value, out ReactionTargetType type)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "product": case "products": type = ReactionTargetType.Product; return true;
            case "post": case "posts": type = ReactionTargetType.Post; return true;
            case "page": case "pages": type = ReactionTargetType.Page; return true;
            default: type = default; return false;
        }
    }

    public static string Name(ReactionTargetType type) => type.ToString().ToLowerInvariant();
}
