using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Application.Text;
using Dotnetable.Domain.Entities;
using Dotnetable.Domain.Enums;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class PostService : IPostService
{
    /// <summary>Post.Status value that marks a post as publicly published.</summary>
    public const byte PublishedStatus = 1;

    private const int SlugMaxLength = 300;

    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public PostService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    /// <summary>All slugs in use by this website's posts (main + translation rows), excluding
    /// <paramref name="excludePostId"/> — the lookup route matches either, so uniqueness must span
    /// both.</summary>
    private static async Task<HashSet<string>> GetUsedSlugsAsync(
        AppDbContext context, int websiteId, int excludePostId, CancellationToken ct)
    {
        var main = await context.Posts.AsNoTracking()
            .Where(p => p.WebsiteID == websiteId && p.PostID != excludePostId)
            .Select(p => p.Slug).ToListAsync(ct);
        var translated = await context.PostTranslations.AsNoTracking()
            .Where(t => t.Post.WebsiteID == websiteId && t.PostID != excludePostId)
            .Select(t => t.Slug).ToListAsync(ct);
        return new HashSet<string>(main.Concat(translated), StringComparer.OrdinalIgnoreCase);
    }

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
            .Include(p => p.PostLocation).ThenInclude(l => l!.PostLocationTranslations)
            .Include(p => p.PostLocation).ThenInclude(l => l!.City)
            .FirstOrDefaultAsync(p => p.PostID == postId, ct);
    }

    public async Task<Post> CreateAsync(Post post, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        post.CreatedAt = post.UpdatedAt = DateTime.UtcNow;
        if (post.Status == PublishedStatus && post.PublishedAt is null)
            post.PublishedAt = DateTime.UtcNow;

        var used = await GetUsedSlugsAsync(_context, post.WebsiteID, excludePostId: 0, ct);
        post.Slug = SlugGenerator.MakeUnique(
            SlugGenerator.Normalize(string.IsNullOrWhiteSpace(post.Slug) ? post.Title : post.Slug, SlugMaxLength),
            used);

        _context.Posts.Add(post);
        await _context.SaveChangesAsync(ct);
        return post;
    }

    public async Task UpdateAsync(Post post, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        // The edit form holds the graph from GetById (translations, tags, categories, location,
        // city, featured file). Posts.Update on that graph inserts PostTags rows that already
        // exist and rewrites unrelated rows. Those relations are saved by their own methods.
        var existing = await _context.Posts.FirstOrDefaultAsync(p => p.PostID == post.PostID, ct);
        if (existing is null) return;

        existing.WebsiteID = post.WebsiteID;
        existing.PostTypeID = post.PostTypeID;
        existing.AuthorMemberID = post.AuthorMemberID;
        existing.Title = post.Title;
        existing.Excerpt = post.Excerpt;
        existing.Content = post.Content;
        existing.MetaTitle = post.MetaTitle;
        existing.MetaDescription = post.MetaDescription;
        existing.MetaKeywords = post.MetaKeywords;
        existing.FeaturedImageFileID = post.FeaturedImageFileID;
        existing.Status = post.Status;
        existing.PublishedAt = post.PublishedAt;
        existing.ScheduledAt = post.ScheduledAt;
        existing.IsFeatured = post.IsFeatured;
        existing.CommentsEnabled = post.CommentsEnabled;
        existing.HasLocation = post.HasLocation;
        existing.IsActive = post.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        if (existing.Status == PublishedStatus && existing.PublishedAt is null)
            existing.PublishedAt = DateTime.UtcNow;

        var used = await GetUsedSlugsAsync(_context, existing.WebsiteID, existing.PostID, ct);
        existing.Slug = SlugGenerator.MakeUnique(
            SlugGenerator.Normalize(string.IsNullOrWhiteSpace(post.Slug) ? existing.Title : post.Slug, SlugMaxLength),
            used);

        await _context.SaveChangesAsync(ct);

        post.Slug = existing.Slug;
        post.UpdatedAt = existing.UpdatedAt;
        post.PublishedAt = existing.PublishedAt;
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
        var locationTranslations = await _context.PostLocationTranslations.Where(t => t.PostID == postId).ToListAsync(ct);
        _context.PostLocationTranslations.RemoveRange(locationTranslations);
        var location = await _context.PostLocations.FirstOrDefaultAsync(l => l.PostID == postId, ct);
        if (location is not null)
            _context.PostLocations.Remove(location);
        foreach (var mi in post.MenuItems)
            mi.PostID = null;
        // Comment FKs are NO ACTION; the whole reply tree goes in the same SaveChanges.
        _context.ContentComments.RemoveRange(
            await _context.ContentComments.Where(c => c.PostID == postId).ToListAsync(ct));
        _context.PostCategories.RemoveRange(post.PostCategories);
        _context.ClientReactions.RemoveRange(await ReactionStore.ForTargetAsync(_context, Dotnetable.Domain.Enums.ReactionTargetType.Post, postId, ct));
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

        var post = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.PostID == postId, ct);
        if (post is null) return;

        var existing = await _context.PostTranslations.Where(t => t.PostID == postId).ToListAsync(ct);

        var keepLanguages = translations
            .Where(t => !string.IsNullOrWhiteSpace(t.Title))
            .Select(t => t.LanguageCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _context.PostTranslations.RemoveRange(existing.Where(t => !keepLanguages.Contains(t.LanguageCode)));

        var used = await GetUsedSlugsAsync(_context, post.WebsiteID, postId, ct);

        foreach (var t in translations)
        {
            if (string.IsNullOrWhiteSpace(t.Title)) continue;
            var current = existing.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, t.LanguageCode, StringComparison.OrdinalIgnoreCase));
            var slug = SlugGenerator.MakeUnique(
                SlugGenerator.Normalize(string.IsNullOrWhiteSpace(t.Slug) ? t.Title : t.Slug, SlugMaxLength),
                used);
            used.Add(slug);

            if (current is null)
                _context.PostTranslations.Add(new PostTranslation
                {
                    PostID = postId,
                    LanguageCode = t.LanguageCode,
                    Title = t.Title.Trim(),
                    Slug = slug,
                    Excerpt = t.Excerpt,
                    Content = t.Content,
                    MetaTitle = t.MetaTitle,
                    MetaDescription = t.MetaDescription,
                    MetaKeywords = t.MetaKeywords,
                });
            else
            {
                current.Title = t.Title.Trim();
                current.Slug = slug;
                current.Excerpt = t.Excerpt;
                current.Content = t.Content;
                current.MetaTitle = t.MetaTitle;
                current.MetaDescription = t.MetaDescription;
                current.MetaKeywords = t.MetaKeywords;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task SetLocationAsync(int postId, bool hasLocation, PostLocationInput? location, IReadOnlyList<PostLocationTranslation> translations, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostID == postId, ct);
        if (post is null) return;

        var existing = await _context.PostLocations
            .Include(l => l.PostLocationTranslations)
            .FirstOrDefaultAsync(l => l.PostID == postId, ct);

        if (!hasLocation || location is null)
        {
            if (existing is not null)
            {
                _context.PostLocationTranslations.RemoveRange(existing.PostLocationTranslations);
                _context.PostLocations.Remove(existing);
            }
            post.HasLocation = false;
            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            return;
        }

        if (location.Latitude is < -90 or > 90 || location.Longitude is < -180 or > 180)
            throw new InvalidOperationException("Latitude must be between -90 and 90, and longitude between -180 and 180.");

        if (!Enum.IsDefined(location.Kind))
            throw new InvalidOperationException("The place type is not recognized.");

        if (location.CityID is int cityId && !await _context.Cities.AnyAsync(c => c.CityID == cityId, ct))
            throw new InvalidOperationException("The selected city was not found.");

        if (existing is null)
        {
            existing = new PostLocation { PostID = postId };
            _context.PostLocations.Add(existing);
        }

        existing.Latitude = location.Latitude;
        existing.Longitude = location.Longitude;
        existing.Kind = (byte)location.Kind;
        existing.Address = string.IsNullOrWhiteSpace(location.Address) ? null : location.Address.Trim();
        existing.CityID = location.CityID;

        var keep = translations
            .Where(t => !string.IsNullOrWhiteSpace(t.LanguageCode) && !string.IsNullOrWhiteSpace(t.Address))
            .Select(t => t.LanguageCode.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _context.PostLocationTranslations.RemoveRange(
            existing.PostLocationTranslations.Where(t => !keep.Contains(t.LanguageCode)));

        foreach (var t in translations)
        {
            if (string.IsNullOrWhiteSpace(t.LanguageCode) || string.IsNullOrWhiteSpace(t.Address)) continue;
            var code = t.LanguageCode.Trim();
            var current = existing.PostLocationTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, code, StringComparison.OrdinalIgnoreCase));
            if (current is null)
            {
                _context.PostLocationTranslations.Add(new PostLocationTranslation
                {
                    PostID = postId,
                    LanguageCode = code,
                    Address = t.Address.Trim(),
                });
            }
            else
            {
                current.Address = t.Address.Trim();
            }
        }

        post.HasLocation = true;
        post.UpdatedAt = DateTime.UtcNow;
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
        int pageIndex, int pageSize, string? languageCode = null, string? authorSlug = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var now = DateTime.UtcNow;
        var q = PublishedQuery(_context, websiteId, now);

        if (!string.IsNullOrWhiteSpace(postTypeSlug))
            q = q.Where(p => p.PostType.Slug == postTypeSlug);
        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            // A parent such as «مقالات» often has no posts of its own; the posts live on its children.
            // When nothing is published directly in the matched categories, the listing is the latest
            // posts from the whole subtree.
            var scope = await CategoryFilterIdsAsync(_context, websiteId, categorySlug, now, ct);
            q = scope.Count == 0
                ? q.Where(p => false)
                : q.Where(p => p.PostCategories.Any(pc => scope.Contains(pc.CategoryID)));
        }
        if (!string.IsNullOrWhiteSpace(tagSlug))
            q = q.Where(p => p.Tags.Any(t => t.Slug == tagSlug || t.TagTranslations.Any(tt => tt.Slug == tagSlug)));
        if (!string.IsNullOrWhiteSpace(authorSlug))
            q = q.Where(p => p.AuthorMember != null && p.AuthorMember.AuthorProfile != null && p.AuthorMember.AuthorProfile.Slug == authorSlug);

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

    /// <summary>
    /// Category ids a public listing should use. The slug's own categories when they have published
    /// posts; otherwise those categories plus every descendant.
    /// </summary>
    private static async Task<List<int>> CategoryFilterIdsAsync(
        AppDbContext context, int websiteId, string categorySlug, DateTime now, CancellationToken ct)
    {
        var rows = await context.Categories.AsNoTracking()
            .Where(c => c.WebsiteID == websiteId && c.IsActive)
            .Select(c => new
            {
                c.CategoryID,
                c.ParentCategoryID,
                c.Slug,
                Translated = c.CategoryTranslations.Select(t => t.Slug).ToList(),
            })
            .ToListAsync(ct);

        var roots = rows
            .Where(c => SlugEquals(c.Slug, categorySlug) || c.Translated.Any(t => SlugEquals(t, categorySlug)))
            .Select(c => c.CategoryID)
            .Distinct()
            .ToList();
        if (roots.Count == 0)
            return roots;

        var hasOwnPosts = await context.Posts.AnyAsync(p =>
            p.WebsiteID == websiteId && p.IsActive && p.Status == PublishedStatus &&
            (p.PublishedAt == null || p.PublishedAt <= now) &&
            p.PostCategories.Any(pc => roots.Contains(pc.CategoryID)), ct);
        if (hasOwnPosts)
            return roots;

        var byParent = rows.ToLookup(c => c.ParentCategoryID);
        var scope = new HashSet<int>(roots);
        var pending = new Stack<int>(roots);
        while (pending.Count > 0)
        {
            foreach (var child in byParent[pending.Pop()])
            {
                if (scope.Add(child.CategoryID))
                    pending.Push(child.CategoryID);
            }
        }
        return scope.ToList();
    }

    private static bool SlugEquals(string? left, string? right) =>
        !string.IsNullOrWhiteSpace(left) &&
        string.Equals(left.Trim(), right?.Trim(), StringComparison.OrdinalIgnoreCase);

    public async Task<PostDetailDto?> GetBySlugAsync(int websiteId, string slug, string? languageCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var now = DateTime.UtcNow;
        var post = await PublishedQuery(_context, websiteId, now)
            .FirstOrDefaultAsync(p => p.Slug == slug || p.PostTranslations.Any(t => t.Slug == slug), ct);
        if (post is null) return null;

        return ProjectDetail(post, languageCode);
    }

    public async Task<bool> RecordViewAsync(int websiteId, int postId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var now = DateTime.UtcNow;
        var updated = await _context.Posts
            .Where(p => p.PostID == postId && p.WebsiteID == websiteId && p.IsActive && p.Status == PublishedStatus &&
                        (p.PublishedAt == null || p.PublishedAt <= now))
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.ViewCount, p => p.ViewCount + 1), ct);
        return updated > 0;
    }

    public async Task<int> SumViewCountAsync(int? websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var query = _context.Posts.AsNoTracking().AsQueryable();
        if (websiteId is int id && id > 0)
            query = query.Where(p => p.WebsiteID == id);
        return await query.SumAsync(p => (int?)p.ViewCount, ct) ?? 0;
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
            .Include(p => p.AuthorMember).ThenInclude(m => m!.Avatar)
            .Include(p => p.AuthorMember).ThenInclude(m => m!.AuthorProfile).ThenInclude(a => a!.PhotoFile)
            .Include(p => p.AuthorMember).ThenInclude(m => m!.AuthorProfile).ThenInclude(a => a!.AuthorProfileTranslations)
            .Include(p => p.FeaturedImageFile)
            .Include(p => p.PostTranslations)
            .Include(p => p.PostLocation).ThenInclude(l => l!.PostLocationTranslations)
            .Include(p => p.PostLocation).ThenInclude(l => l!.City).ThenInclude(c => c!.CityTranslations)
            .Include(p => p.PostCategories).ThenInclude(pc => pc.Category).ThenInclude(c => c.CategoryTranslations)
            .Include(p => p.Tags).ThenInclude(t => t.TagTranslations);

    // ── Projection helpers ──────────────────────────────────────────

    private static PostSummaryDto ProjectSummary(Post p, string? lang)
    {
        var (title, slug, excerpt, _) = LocalizedFull(p, lang);
        var author = AuthorProjection.Card(p.AuthorMember, lang);
        return new PostSummaryDto
        {
            PostID = p.PostID, Slug = slug, Title = title, Excerpt = excerpt,
            FeaturedImageUrl = FeaturedUrl(p), FeaturedImageFullUrl = p.FeaturedImageFile?.CNDUrl, PostTypeSlug = p.PostType?.Slug ?? string.Empty,
            AuthorName = author?.Name, Author = author, IsFeatured = p.IsFeatured, ViewCount = p.ViewCount, LikeCount = p.LikeCount,
            HasLocation = p.HasLocation && p.PostLocation is not null, Location = ProjectLocation(p, lang),
            PublishedAt = p.PublishedAt, Categories = Categories(p, lang), Tags = Tags(p, lang),
        };
    }

    private static PostDetailDto ProjectDetail(Post p, string? lang)
    {
        var (title, slug, excerpt, content) = LocalizedFull(p, lang);
        var (metaTitle, metaDescription, metaKeywords) = LocalizedMeta(p, lang);
        var author = AuthorProjection.Card(p.AuthorMember, lang);
        return new PostDetailDto
        {
            PostID = p.PostID, Slug = slug, Title = title, Excerpt = excerpt,
            FeaturedImageUrl = FeaturedUrl(p), FeaturedImageFullUrl = p.FeaturedImageFile?.CNDUrl, PostTypeSlug = p.PostType?.Slug ?? string.Empty,
            AuthorName = author?.Name, Author = author, IsFeatured = p.IsFeatured, ViewCount = p.ViewCount, LikeCount = p.LikeCount,
            HasLocation = p.HasLocation && p.PostLocation is not null, Location = ProjectLocation(p, lang),
            PublishedAt = p.PublishedAt, Categories = Categories(p, lang), Tags = Tags(p, lang),
            Content = content,
            // The post type can switch comments off for every post of that type.
            CommentsEnabled = p.CommentsEnabled && (p.PostType?.CommentsEnabled ?? true),
            MetaTitle = metaTitle, MetaDescription = metaDescription, MetaKeywords = metaKeywords,
        };
    }

    private static string? FeaturedUrl(Post p) => p.FeaturedImageFile?.ThumbnailCDN ?? p.FeaturedImageFile?.CNDUrl;

    private static PostLocationDto? ProjectLocation(Post p, string? lang)
    {
        if (!p.HasLocation || p.PostLocation is not { } loc) return null;

        var address = loc.Address;
        if (!string.IsNullOrWhiteSpace(lang))
        {
            var translated = loc.PostLocationTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, lang, StringComparison.OrdinalIgnoreCase));
            if (translated is not null && !string.IsNullOrWhiteSpace(translated.Address))
                address = translated.Address;
        }

        string? cityName = null;
        if (loc.City is not null)
        {
            cityName = loc.City.Title;
            if (!string.IsNullOrWhiteSpace(lang))
            {
                var cityTranslation = loc.City.CityTranslations.FirstOrDefault(x =>
                    string.Equals(x.LanguageCode, lang, StringComparison.OrdinalIgnoreCase));
                if (cityTranslation is not null && !string.IsNullOrWhiteSpace(cityTranslation.Title))
                    cityName = cityTranslation.Title;
            }
        }

        var kind = (PostLocationKind)loc.Kind;
        if (!Enum.IsDefined(kind)) kind = PostLocationKind.Unspecified;

        return new PostLocationDto
        {
            Latitude = loc.Latitude,
            Longitude = loc.Longitude,
            Kind = kind,
            Address = address,
            CityID = loc.CityID,
            CityName = cityName,
        };
    }

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

    private static (string? MetaTitle, string? MetaDescription, string? MetaKeywords) LocalizedMeta(Post p, string? lang)
    {
        if (!string.IsNullOrWhiteSpace(lang))
        {
            var t = p.PostTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, lang, StringComparison.OrdinalIgnoreCase));
            if (t is not null)
                return (
                    string.IsNullOrWhiteSpace(t.MetaTitle) ? p.MetaTitle : t.MetaTitle,
                    string.IsNullOrWhiteSpace(t.MetaDescription) ? p.MetaDescription : t.MetaDescription,
                    string.IsNullOrWhiteSpace(t.MetaKeywords) ? p.MetaKeywords : t.MetaKeywords);
        }
        return (p.MetaTitle, p.MetaDescription, p.MetaKeywords);
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
