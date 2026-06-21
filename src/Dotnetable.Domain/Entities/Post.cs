namespace Dotnetable.Domain.Entities;

public class Post
{
    public int Id { get; set; }
    public int WebsiteId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Excerpt { get; set; }
    public PostType Type { get; set; } = PostType.Post;
    public PostStatus Status { get; set; } = PostStatus.Draft;
    public int LanguageId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Website? Website { get; set; }
    public Language? Language { get; set; }
}

public enum PostType { Post, Page, Blog }
public enum PostStatus { Draft, Published, Archived }
