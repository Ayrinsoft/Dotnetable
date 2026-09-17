namespace Dotnetable.Domain.Enums;

/// <summary>
/// Kind of a <see cref="Entities.ClientReaction"/>. Stored as a TINYINT; never renumber.
/// </summary>
public enum ReactionType : byte
{
    /// <summary>A like on a post/page, or "favorite" on a product (kept in step with the wishlist).
    /// Counted once per client in <c>Post.LikeCount</c> / <c>Page.LikeCount</c> / <c>Product.FavoriteCount</c>.</summary>
    Like = 1,

    /// <summary>A private bookmark; not counted publicly.</summary>
    Bookmark = 2,
}
