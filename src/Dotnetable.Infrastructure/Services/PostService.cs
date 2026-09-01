using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class PostService : IPostService
{
    /// <summary>Post.Status value that marks a post as publicly published.</summary>
    public const byte PublishedStatus = 1;

    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public PostService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    // ── Admin management ────────────────────────────────────────────

    public async Task<PagedResult<Post>> GetPagedAsync(int? websiteId, PostFilter filter, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.Posts.AsNoTracking().Include(p => p.PostType).AsQueryable();
        if (websiteId is int wid)
            q = q.Where(p => p.WebsiteID == wid);
        if (filter.PostTypeID is int ptid)
            q = q.Where(p => p.PostTypeID == ptid);
        if (filter.Status is byte status)
            q = q.Where(p => p.Status == status);
        if (filter.CategoryID is int cid)
            q = q.Where(p => p.PostCategories.Any(pc => pc.CategoryID == cid));

        if (query.GetSearch(nameof(Post.Title)) is string title)
            q = q.Where(p => p.Title.Contains(title));
        if (query.GetSearch(nameof(Post.Slug)) is string slug)
            q = q.Where(p => p.Slug.Contains(slug));

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(Post.PostID), fallbackDescending: true)
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<Post> { Items = items, TotalCount = total };
    }

    public async Task<Post?> GetByIdAsync(int postId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.Posts
            .Include(p => p.PostTranslations)
            .Include(p => p.PostCategories)
            .Include(p => p.Tags)
            .Include(p => p.FeaturedImageFile)
            .FirstOrDefaultAsync(p => p.PostID == postId, ct);
    }

    public async Task<Post> CreateAsync(Post post, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        post.CreatedAt = post.UpdatedAt = DateTime.UtcNow;
        if (post.Status == PublishedStatus && post.PublishedAt is null)
            post.PublishedAt = DateTime.UtcNow;
        _context.Posts.Add(post);
        await _context.SaveChangesAsync(ct);
        return post;
    }

    public async Task UpdateAsync(Post post, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        post.UpdatedAt = DateTime.UtcNow;
        if (post.Status == PublishedStatus && post.PublishedAt is null)
            post.PublishedAt = DateTime.UtcNow;
        _context.Posts.Update(post);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int postId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var post = await _context.Posts
            .Include(p => p.PostTranslations)
            .Include(p => p.PostCategories)
            .Include(p => p.Tags)
            .Include(p => p.MenuItems)
            .FirstOrDefaultAsync(p => p.PostID == postId, ct);
        if (post is null) return;

        post.Tags.Clear();
        foreach (var mi in post.MenuItems)
            mi.PostID = null;
        _context.PostCategories.RemoveRange(post.PostCategories);
        _context.PostTranslations.RemoveRange(post.PostTranslations);
        _context.Posts.Remove(post);
        await _context.SaveChangesAsync(ct);
    }

    public async Task SetActiveAsync(int postId, bool active, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        await _context.Posts.Where(p => p.PostID == postId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsActive, active), ct);
    }

    // ── Translations ────────────────────────────────────────────────

    public async Task<List<PostTranslation>> GetTranslationsAsync(int postId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.PostTranslations.AsNoTracking()
            .Where(t => t.PostID == postId)
            .ToListAsync(ct);
    }

    public async Task SetTranslationsAsync(int postId, IReadOnlyList<PostTranslation> translations, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.PostTranslations.Where(t => t.PostID == postId).ToListAsync(ct);

        var keepLanguages = translations
            .Where(t => !string.IsNullOrWhiteSpace(t.Title))
            .Select(t => t.LanguageCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _context.PostTranslations.RemoveRange(existing.Where(t => !keepLanguages.Contains(t.LanguageCode)));

        foreach (var t in translations)
        {
            if (string.IsNullOrWhiteSpace(t.Title)) continue;
            var current = existing.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, t.LanguageCode, StringComparison.OrdinalIgnoreCase));
            var slug = string.IsNullOrWhiteSpace(t.Slug) ? t.Title.Trim() : t.Slug.Trim();

            if (current is null)
                _context.PostTranslations.Add(new PostTranslation
                {
                    PostID = postId,
                    LanguageCode = t.LanguageCode,
                    Title = t.Title.Trim(),
                    Slug = slug,
                    Excerpt = t.Excerpt,
                    Content = t.Content,
                });
            else
            {
                current.Title = t.Title.Trim();
                current.Slug = slug;
                current.Excerpt = t.Excerpt;
                current.Content = t.Content;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    // ── Category / tag assignment ───────────────────────────────────

    public async Task<List<int>> GetCategoryIdsAsync(int postId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.PostCategories.AsNoTracking()
            .Where(pc => pc.PostID == postId)
            .Select(pc => pc.CategoryID)
            .ToListAsync(ct);
    }

    public async Task SetCategoriesAsync(int postId, IReadOnlyList<int> categoryIds, int? primaryCategoryId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.PostCategories.Where(pc => pc.PostID == postId).ToListAsync(ct);
        var wanted = categoryIds.Distinct().ToList();

        _context.PostCategories.RemoveRange(existing.Where(pc => !wanted.Contains(pc.CategoryID)));

        foreach (var categoryId in wanted)
        {
            var current = existing.FirstOrDefault(pc => pc.CategoryID == categoryId);
            var isPrimary = primaryCategoryId == categoryId;
            if (current is null)
                _context.PostCategories.Add(new PostCategory { PostID = postId, CategoryID = categoryId, IsPrimary = isPrimary });
            else
                current.IsPrimary = isPrimary;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<int>> GetTagIdsAsync(int postId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.Posts.AsNoTracking()
            .Where(p => p.PostID == postId)
            .SelectMany(p => p.Tags.Select(t => t.TagID))
            .ToListAsync(ct);
    }

    public async Task SetTagsAsync(int postId, IReadOnlyList<int> tagIds, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var post = await _context.Posts.Include(p => p.Tags).FirstOrDefaultAsync(p => p.PostID == postId, ct);
        if (post is null) return;

        var wanted = tagIds.Distinct().ToHashSet();
        post.Tags.Where(t => !wanted.Contains(t.TagID)).ToList()
            .ForEach(t => post.Tags.Remove(t));

        var current = post.Tags.Select(t => t.TagID).ToHashSet();
        var toAdd = wanted.Where(id => !current.Contains(id)).ToList();
        if (toAdd.Count > 0)
        {
            var tags = await _context.Tags.Where(t => toAdd.Contains(t.TagID)).ToListAsync(ct);
            foreach (var tag in tags) post.Tags.Add(tag);
        }

        await _context.SaveChangesAsync(ct);
    }

    // ── Public read ─────────────────────────────────────────────────

    public async Task<PagedResult<PostSummaryDto>> GetPublishedAsync(
        int websiteId, string? postTypeSlug, string? categorySlug, string? tagSlug,
        int pageIndex, int pageSize, string? languageCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var now = DateTime.UtcNow;
        var q = PublishedQuery(_context, websiteId, now);

        if (!string.IsNullOrWhiteSpace(postTypeSlug))
            q = q.Where(p => p.PostType.Slug == postTypeSlug);
        if (!string.IsNullOrWhiteSpace(categorySlug))
            q = q.Where(p => p.PostCategories.Any(pc =>
                pc.Category.Slug == categorySlug || pc.Category.CategoryTranslations.Any(t => t.Slug == categorySlug)));
        if (!string.IsNullOrWhiteSpace(tagSlug))
            q = q.Where(p => p.Tags.Any(t => t.Slug == tagSlug || t.TagTranslations.Any(tt => tt.Slug == tagSlug)));

        var total = await q.CountAsync(ct);

        var take = pageSize < 1 ? 10 : pageSize;
        var skip = (pageIndex < 1 ? 0 : pageIndex - 1) * take;

        var posts = await q
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Skip(skip).Take(take)
            .ToListAsync(ct);

        return new PagedResult<PostSummaryDto>
        {
            Items = posts.Select(p => ProjectSummary(p, languageCode)).ToList(),
            TotalCount = total,
        };
    }

    public async Task<PostDetailDto?> GetBySlugAsync(int websiteId, string slug, string? languageCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var now = DateTime.UtcNow;
        var post = await PublishedQuery(_context, websiteId, now)
            .FirstOrDefaultAsync(p => p.Slug == slug || p.PostTranslations.Any(t => t.Slug == slug), ct);
        if (post is null) return null;

        // Best-effort view counter.
        try
        {
            await _context.Posts.Where(p => p.PostID == post.PostID)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.ViewCount, p => p.ViewCount + 1), ct);
        }
        catch (Exception) { }

        return ProjectDetail(post, languageCode);
    }

    public async Task<List<PostSummaryDto>> GetFeaturedAsync(int websiteId, int take, string? languageCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var now = DateTime.UtcNow;
        var posts = await PublishedQuery(_context, websiteId, now)
            .Where(p => p.IsFeatured)
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Take(take < 1 ? 4 : take)
            .ToListAsync(ct);
        return posts.Select(p => ProjectSummary(p, languageCode)).ToList();
    }

    // Takes the caller's context: an IQueryable is only valid while the context that built it is alive.
    private static IQueryable<Post> PublishedQuery(AppDbContext _context, int websiteId, DateTime now) =>
        _context.Posts.AsNoTracking()
            .Where(p => p.WebsiteID == websiteId && p.IsActive && p.Status == PublishedStatus &&
                (p.PublishedAt == null || p.PublishedAt <= now))
            .Include(p => p.PostType)
            .Include(p => p.AuthorMember)
            .Include(p => p.FeaturedImageFile)
            .Include(p => p.PostTranslations)
            .Include(p => p.PostCategories).ThenInclude(pc => pc.Category).ThenInclude(c => c.CategoryTranslations)
            .Include(p => p.Tags).ThenInclude(t => t.TagTranslations);

    // ── Projection helpers ──────────────────────────────────────────

    private static PostSummaryDto ProjectSummary(Post p, string? lang)
    {
        var (title, slug, excerpt, _) = LocalizedFull(p, lang);
        return new PostSummaryDto
        {
            PostID = p.PostID, Slug = slug, Title = title, Excerpt = excerpt,
            FeaturedImageUrl = FeaturedUrl(p), PostTypeSlug = p.PostType?.Slug ?? string.Empty,
            AuthorName = AuthorName(p), IsFeatured = p.IsFeatured, ViewCount = p.ViewCount,
            PublishedAt = p.PublishedAt, Categories = Categories(p, lang), Tags = Tags(p, lang),
        };
    }

    private static PostDetailDto ProjectDetail(Post p, string? lang)
    {
        var (title, slug, excerpt, content) = LocalizedFull(p, lang);
        return new PostDetailDto
        {
            PostID = p.PostID, Slug = slug, Title = title, Excerpt = excerpt,
            FeaturedImageUrl = FeaturedUrl(p), PostTypeSlug = p.PostType?.Slug ?? string.Empty,
            AuthorName = AuthorName(p), IsFeatured = p.IsFeatured, ViewCount = p.ViewCount,
            PublishedAt = p.PublishedAt, Categories = Categories(p, lang), Tags = Tags(p, lang),
            Content = content, CommentsEnabled = p.CommentsEnabled,
        };
    }

    private static string? FeaturedUrl(Post p) => p.FeaturedImageFile?.ThumbnailCDN ?? p.FeaturedImageFile?.CNDUrl;

    private static string? AuthorName(Post p) =>
        p.AuthorMember is null ? null : $"{p.AuthorMember.Givenname} {p.AuthorMember.Surname}".Trim();

    private static List<CategoryDto> Categories(Post p, string? lang) =>
        p.PostCategories.Where(pc => pc.Category is not null)
            .Select(pc => LocalizeCategory(pc.Category, lang)).ToList();

    private static List<TagDto> Tags(Post p, string? lang) =>
        p.Tags.Select(t => LocalizeTag(t, lang)).ToList();

    private static (string Title, string Slug, string? Excerpt, string? Content) LocalizedFull(Post p, string? lang)
    {
        if (!string.IsNullOrWhiteSpace(lang))
        {
            var t = p.PostTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, lang, StringComparison.OrdinalIgnoreCase));
            if (t is not null && !string.IsNullOrWhiteSpace(t.Title))
                return (t.Title, string.IsNullOrWhiteSpace(t.Slug) ? p.Slug : t.Slug,
                    string.IsNullOrWhiteSpace(t.Excerpt) ? p.Excerpt : t.Excerpt,
                    string.IsNullOrWhiteSpace(t.Content) ? p.Content : t.Content);
        }
        return (p.Title, p.Slug, p.Excerpt, p.Content);
    }

    private static CategoryDto LocalizeCategory(Category c, string? lang)
    {
        var name = c.Name; var slug = c.Slug;
        if (!string.IsNullOrWhiteSpace(lang))
        {
            var t = c.CategoryTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, lang, StringComparison.OrdinalIgnoreCase));
            if (t is not null && !string.IsNullOrWhiteSpace(t.Name)) { name = t.Name; slug = string.IsNullOrWhiteSpace(t.Slug) ? c.Slug : t.Slug; }
        }
        return new CategoryDto { CategoryID = c.CategoryID, ParentCategoryID = c.ParentCategoryID, PostTypeID = c.PostTypeID, Name = name, Slug = slug, SortOrder = c.SortOrder };
    }

    private static TagDto LocalizeTag(Tag t, string? lang)
    {
        var name = t.Name; var slug = t.Slug;
        if (!string.IsNullOrWhiteSpace(lang))
        {
            var tr = t.TagTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, lang, StringComparison.OrdinalIgnoreCase));
            if (tr is not null && !string.IsNullOrWhiteSpace(tr.Name)) { name = tr.Name; slug = string.IsNullOrWhiteSpace(tr.Slug) ? t.Slug : tr.Slug; }
        }
        return new TagDto { TagID = t.TagID, Name = name, Slug = slug };
    }
}
