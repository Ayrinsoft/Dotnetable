using Dotnetable.Application;
using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Catalog;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

/// <summary>
/// Copies a website's entered content onto another website. Source rows are left in place,
/// including when a blog is later switched to a store.
/// </summary>
public class WebsiteContentCopyService : IWebsiteContentCopyService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public WebsiteContentCopyService(IDbContextFactory<AppDbContext> contextFactory) =>
        _contextFactory = contextFactory;

    public async Task<WebsiteContentCopyResult> CopyAsync(int sourceWebsiteId, int targetWebsiteId, CancellationToken ct = default)
    {
        if (sourceWebsiteId <= 0 || targetWebsiteId <= 0 || sourceWebsiteId == targetWebsiteId)
            throw new InvalidOperationException("Choose two different websites.");

        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var sites = await context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == sourceWebsiteId || w.WebsiteID == targetWebsiteId)
            .Select(w => w.WebsiteID)
            .ToListAsync(ct);
        if (sites.Count != 2)
            throw new InvalidOperationException("Source or target website was not found.");

        var result = new WebsiteContentCopyResult();
        var platform = await PlatformCatalog.IsPlatformAsync(context, ct);
        var copyCatalog = !platform || targetWebsiteId == AppConstants.MasterWebsiteId;
        if (!copyCatalog)
            result.Note = "Products and product categories were not copied. On a platform only website 1 owns that catalog. Posts and pages were copied, and the source website was not changed.";
        else
            result.Note = "A copy was written on the target website. The source website still has its original rows. Changing a site from blog to store does not delete this data.";

        await CopyContentVersionsAsync(context, sourceWebsiteId, targetWebsiteId, ct);
        var postTypeMap = await CopyPostTypesAsync(context, sourceWebsiteId, targetWebsiteId, result, ct);
        var categoryMap = await CopyContentCategoriesAsync(context, sourceWebsiteId, targetWebsiteId, postTypeMap, result, ct);
        var tagMap = await CopyTagsAsync(context, sourceWebsiteId, targetWebsiteId, result, ct);
        await CopyPostsAsync(context, sourceWebsiteId, targetWebsiteId, postTypeMap, categoryMap, tagMap, result, ct);
        await CopyPagesAsync(context, sourceWebsiteId, targetWebsiteId, result, ct);

        if (copyCatalog)
        {
            var brandMap = await CopyBrandsAsync(context, sourceWebsiteId, targetWebsiteId, result, ct);
            var productCategoryMap = await CopyProductCategoriesAsync(context, sourceWebsiteId, targetWebsiteId, result, ct);
            await CopyProductsAsync(context, sourceWebsiteId, targetWebsiteId, brandMap, productCategoryMap, result, ct);
        }

        return result;
    }

    private static async Task CopyContentVersionsAsync(
        AppDbContext context, int sourceId, int targetId, CancellationToken ct)
    {
        var source = await context.ContentVersions.AsNoTracking().Where(v => v.WebsiteID == sourceId).ToListAsync(ct);
        if (source.Count == 0) return;
        var names = (await context.ContentVersions.AsNoTracking()
            .Where(v => v.WebsiteID == targetId)
            .Select(v => v.Name)
            .ToListAsync(ct)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var row in source)
        {
            if (!names.Add(row.Name)) continue;
            context.ContentVersions.Add(new ContentVersion
            {
                WebsiteID = targetId,
                Name = row.Name,
                SortOrder = row.SortOrder,
                IsLatest = row.IsLatest,
                IsPublished = row.IsPublished,
            });
        }
        await context.SaveChangesAsync(ct);
    }

    private static async Task<Dictionary<int, int>> CopyPostTypesAsync(
        AppDbContext context, int sourceId, int targetId, WebsiteContentCopyResult result, CancellationToken ct)
    {
        var source = await context.PostTypes.AsNoTracking().Where(t => t.WebsiteID == sourceId).ToListAsync(ct);
        var existing = await context.PostTypes.Where(t => t.WebsiteID == targetId).ToListAsync(ct);
        var slugs = existing.Select(t => t.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var map = new Dictionary<int, int>();
        foreach (var row in source)
        {
            var match = existing.FirstOrDefault(t => string.Equals(t.Slug, row.Slug, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                map[row.PostTypeID] = match.PostTypeID;
                continue;
            }

            var copy = new PostType
            {
                WebsiteID = targetId,
                Name = row.Name,
                Slug = Unique(slugs, row.Slug, row.PostTypeID),
                HasCategories = row.HasCategories,
                HasTags = row.HasTags,
                HasAuthor = row.HasAuthor,
                CommentsEnabled = row.CommentsEnabled,
                SupportsStableKey = row.SupportsStableKey,
                SupportsVersioning = row.SupportsVersioning,
            };
            context.PostTypes.Add(copy);
            await context.SaveChangesAsync(ct);
            map[row.PostTypeID] = copy.PostTypeID;
            result.PostTypes++;
        }
        return map;
    }

    private static async Task<Dictionary<int, int>> CopyContentCategoriesAsync(
        AppDbContext context, int sourceId, int targetId, Dictionary<int, int> postTypeMap,
        WebsiteContentCopyResult result, CancellationToken ct)
    {
        var source = await context.Categories.AsNoTracking()
            .Include(c => c.CategoryTranslations)
            .Where(c => c.WebsiteID == sourceId)
            .ToListAsync(ct);
        var slugs = (await context.Categories.AsNoTracking().Where(c => c.WebsiteID == targetId).Select(c => c.Slug).ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var map = new Dictionary<int, int>();
        var pending = source.ToList();
        var guard = 0;
        while (pending.Count > 0 && guard++ < source.Count + 2)
        {
            var ready = pending.Where(c => c.ParentCategoryID is null || map.ContainsKey(c.ParentCategoryID.Value)).ToList();
            if (ready.Count == 0) break;
            foreach (var row in ready)
            {
                var copy = new Category
                {
                    WebsiteID = targetId,
                    PostTypeID = row.PostTypeID is int pt && postTypeMap.TryGetValue(pt, out var mapped) ? mapped : null,
                    ParentCategoryID = row.ParentCategoryID is int parent ? map[parent] : null,
                    Name = row.Name,
                    Slug = Unique(slugs, row.Slug, row.CategoryID),
                    Summary = row.Summary,
                    SortOrder = row.SortOrder,
                    IsActive = row.IsActive,
                };
                foreach (var translation in row.CategoryTranslations)
                {
                    copy.CategoryTranslations.Add(new CategoryTranslation
                    {
                        LanguageCode = translation.LanguageCode,
                        Name = translation.Name,
                        Slug = translation.Slug,
                        Summary = translation.Summary,
                    });
                }
                context.Categories.Add(copy);
                await context.SaveChangesAsync(ct);
                map[row.CategoryID] = copy.CategoryID;
                result.Categories++;
                pending.Remove(row);
            }
        }
        return map;
    }

    private static async Task<Dictionary<int, int>> CopyTagsAsync(
        AppDbContext context, int sourceId, int targetId, WebsiteContentCopyResult result, CancellationToken ct)
    {
        var source = await context.Tags.AsNoTracking().Include(t => t.TagTranslations).Where(t => t.WebsiteID == sourceId).ToListAsync(ct);
        var existing = await context.Tags.Where(t => t.WebsiteID == targetId).ToListAsync(ct);
        var slugs = existing.Select(t => t.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var map = new Dictionary<int, int>();
        foreach (var row in source)
        {
            var match = existing.FirstOrDefault(t => string.Equals(t.Slug, row.Slug, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                map[row.TagID] = match.TagID;
                continue;
            }

            var copy = new Tag
            {
                WebsiteID = targetId,
                Name = row.Name,
                Slug = Unique(slugs, row.Slug, row.TagID),
            };
            foreach (var translation in row.TagTranslations)
            {
                copy.TagTranslations.Add(new TagTranslation
                {
                    LanguageCode = translation.LanguageCode,
                    Name = translation.Name,
                    Slug = translation.Slug,
                });
            }
            context.Tags.Add(copy);
            await context.SaveChangesAsync(ct);
            map[row.TagID] = copy.TagID;
            result.Tags++;
        }
        return map;
    }

    private static async Task CopyPostsAsync(
        AppDbContext context, int sourceId, int targetId,
        Dictionary<int, int> postTypeMap, Dictionary<int, int> categoryMap, Dictionary<int, int> tagMap,
        WebsiteContentCopyResult result, CancellationToken ct)
    {
        var source = await context.Posts.AsNoTracking()
            .Include(p => p.PostTranslations)
            .Include(p => p.PostCategories)
            .Include(p => p.Tags)
            .Include(p => p.PostLocation!).ThenInclude(l => l.PostLocationTranslations)
            .Where(p => p.WebsiteID == sourceId)
            .ToListAsync(ct);
        var slugs = (await context.Posts.AsNoTracking().Where(p => p.WebsiteID == targetId).Select(p => p.Slug).ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;

        foreach (var row in source)
        {
            if (!postTypeMap.TryGetValue(row.PostTypeID, out var postTypeId)) continue;
            var copy = new Post
            {
                WebsiteID = targetId,
                PostTypeID = postTypeId,
                Slug = Unique(slugs, row.Slug, row.PostID),
                Title = row.Title,
                Excerpt = row.Excerpt,
                Content = row.Content,
                MetaTitle = row.MetaTitle,
                MetaDescription = row.MetaDescription,
                MetaKeywords = row.MetaKeywords,
                FeaturedImageFileID = row.FeaturedImageFileID,
                Status = row.Status,
                PublishedAt = row.PublishedAt,
                ScheduledAt = row.ScheduledAt,
                IsFeatured = row.IsFeatured,
                CommentsEnabled = row.CommentsEnabled,
                HasLocation = row.HasLocation && row.PostLocation is not null,
                IsActive = row.IsActive,
                StableKey = row.StableKey,
                AppliesFrom = row.AppliesFrom,
                AppliesTo = row.AppliesTo,
                CreatedAt = now,
                UpdatedAt = now,
            };
            foreach (var translation in row.PostTranslations)
            {
                copy.PostTranslations.Add(new PostTranslation
                {
                    LanguageCode = translation.LanguageCode,
                    Title = translation.Title,
                    Slug = translation.Slug,
                    Excerpt = translation.Excerpt,
                    Content = translation.Content,
                    MetaTitle = translation.MetaTitle,
                    MetaDescription = translation.MetaDescription,
                    MetaKeywords = translation.MetaKeywords,
                });
            }
            foreach (var link in row.PostCategories)
            {
                if (!categoryMap.TryGetValue(link.CategoryID, out var categoryId)) continue;
                copy.PostCategories.Add(new PostCategory { CategoryID = categoryId, IsPrimary = link.IsPrimary });
            }
            if (row.HasLocation && row.PostLocation is not null)
            {
                var location = new PostLocation
                {
                    Latitude = row.PostLocation.Latitude,
                    Longitude = row.PostLocation.Longitude,
                    Kind = row.PostLocation.Kind,
                    Address = row.PostLocation.Address,
                    CityID = row.PostLocation.CityID,
                };
                foreach (var translation in row.PostLocation.PostLocationTranslations)
                {
                    location.PostLocationTranslations.Add(new PostLocationTranslation
                    {
                        LanguageCode = translation.LanguageCode,
                        Address = translation.Address,
                    });
                }
                copy.PostLocation = location;
            }

            context.Posts.Add(copy);
            await context.SaveChangesAsync(ct);

            var tagIds = row.Tags.Select(t => t.TagID).Where(tagMap.ContainsKey).Select(id => tagMap[id]).Distinct().ToList();
            if (tagIds.Count > 0)
            {
                var tags = await context.Tags.Where(t => tagIds.Contains(t.TagID)).ToListAsync(ct);
                foreach (var tag in tags)
                    copy.Tags.Add(tag);
                await context.SaveChangesAsync(ct);
            }

            result.Posts++;
        }
    }

    private static async Task CopyPagesAsync(
        AppDbContext context, int sourceId, int targetId, WebsiteContentCopyResult result, CancellationToken ct)
    {
        var source = await context.Pages.AsNoTracking()
            .Include(p => p.PageTranslations)
            .Where(p => p.WebsiteID == sourceId)
            .ToListAsync(ct);
        var slugs = (await context.Pages.AsNoTracking().Where(p => p.WebsiteID == targetId).Select(p => p.Slug).ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var targetHasHome = await context.Pages.AnyAsync(p => p.WebsiteID == targetId && p.IsHomepage, ct);
        var map = new Dictionary<int, int>();
        var pending = source.ToList();
        var now = DateTime.UtcNow;
        var guard = 0;
        while (pending.Count > 0 && guard++ < source.Count + 2)
        {
            var ready = pending.Where(p => p.ParentPageID is null || map.ContainsKey(p.ParentPageID.Value)).ToList();
            if (ready.Count == 0) break;
            foreach (var row in ready)
            {
                var isHome = row.IsHomepage && !targetHasHome;
                if (isHome) targetHasHome = true;
                var copy = new Page
                {
                    WebsiteID = targetId,
                    ParentPageID = row.ParentPageID is int parent ? map[parent] : null,
                    Slug = Unique(slugs, row.Slug, row.PageID),
                    Title = row.Title,
                    Content = row.Content,
                    MetaTitle = row.MetaTitle,
                    MetaDescription = row.MetaDescription,
                    MetaKeywords = row.MetaKeywords,
                    Template = row.Template,
                    IsHomepage = isHome,
                    Status = row.Status,
                    SortOrder = row.SortOrder,
                    CommentsEnabled = row.CommentsEnabled,
                    IsActive = row.IsActive,
                    CreatedAt = now,
                    UpdatedAt = now,
                };
                foreach (var translation in row.PageTranslations)
                {
                    copy.PageTranslations.Add(new PageTranslation
                    {
                        LanguageCode = translation.LanguageCode,
                        Title = translation.Title,
                        Slug = translation.Slug,
                        Content = translation.Content,
                        MetaTitle = translation.MetaTitle,
                        MetaDescription = translation.MetaDescription,
                        MetaKeywords = translation.MetaKeywords,
                    });
                }
                context.Pages.Add(copy);
                await context.SaveChangesAsync(ct);
                map[row.PageID] = copy.PageID;
                result.Pages++;
                pending.Remove(row);
            }
        }
    }

    private static async Task<Dictionary<int, int>> CopyBrandsAsync(
        AppDbContext context, int sourceId, int targetId, WebsiteContentCopyResult result, CancellationToken ct)
    {
        var source = await context.Brands.AsNoTracking().Include(b => b.BrandTranslations).Where(b => b.WebsiteID == sourceId).ToListAsync(ct);
        var existing = await context.Brands.Where(b => b.WebsiteID == targetId).ToListAsync(ct);
        var slugs = existing.Select(b => b.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var map = new Dictionary<int, int>();
        foreach (var row in source)
        {
            var match = existing.FirstOrDefault(b => string.Equals(b.Slug, row.Slug, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                map[row.BrandID] = match.BrandID;
                continue;
            }

            var copy = new Brand
            {
                WebsiteID = targetId,
                Name = row.Name,
                Slug = Unique(slugs, row.Slug, row.BrandID),
                LogoFileID = row.LogoFileID,
                IsActive = row.IsActive,
            };
            foreach (var translation in row.BrandTranslations)
            {
                copy.BrandTranslations.Add(new BrandTranslation
                {
                    LanguageCode = translation.LanguageCode,
                    Name = translation.Name,
                    Slug = translation.Slug,
                });
            }
            context.Brands.Add(copy);
            await context.SaveChangesAsync(ct);
            map[row.BrandID] = copy.BrandID;
            result.Brands++;
        }
        return map;
    }

    private static async Task<Dictionary<int, int>> CopyProductCategoriesAsync(
        AppDbContext context, int sourceId, int targetId, WebsiteContentCopyResult result, CancellationToken ct)
    {
        var source = await context.ProductCategories.AsNoTracking()
            .Include(c => c.ProductCategoryTranslations)
            .Where(c => c.WebsiteID == sourceId)
            .ToListAsync(ct);
        var slugs = (await context.ProductCategories.AsNoTracking().Where(c => c.WebsiteID == targetId).Select(c => c.Slug).ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var map = new Dictionary<int, int>();
        var pending = source.ToList();
        var guard = 0;
        while (pending.Count > 0 && guard++ < source.Count + 2)
        {
            var ready = pending.Where(c => c.ParentCategoryID is null || map.ContainsKey(c.ParentCategoryID.Value)).ToList();
            if (ready.Count == 0) break;
            foreach (var row in ready)
            {
                var copy = new ProductCategory
                {
                    WebsiteID = targetId,
                    ParentCategoryID = row.ParentCategoryID is int parent ? map[parent] : null,
                    Name = row.Name,
                    Slug = Unique(slugs, row.Slug, row.ProductCategoryID),
                    ImageFileID = row.ImageFileID,
                    SortOrder = row.SortOrder,
                    IsActive = row.IsActive,
                };
                foreach (var translation in row.ProductCategoryTranslations)
                {
                    copy.ProductCategoryTranslations.Add(new ProductCategoryTranslation
                    {
                        LanguageCode = translation.LanguageCode,
                        Name = translation.Name,
                        Slug = translation.Slug,
                    });
                }
                context.ProductCategories.Add(copy);
                await context.SaveChangesAsync(ct);
                map[row.ProductCategoryID] = copy.ProductCategoryID;
                result.ProductCategories++;
                pending.Remove(row);
            }
        }
        return map;
    }

    private static async Task CopyProductsAsync(
        AppDbContext context, int sourceId, int targetId,
        Dictionary<int, int> brandMap, Dictionary<int, int> categoryMap,
        WebsiteContentCopyResult result, CancellationToken ct)
    {
        var source = await context.Products.AsNoTracking()
            .Include(p => p.ProductTranslations)
            .Include(p => p.ProductVariants)
            .Include(p => p.ProductCategoryMaps)
            .Where(p => p.WebsiteID == sourceId)
            .ToListAsync(ct);
        var slugs = (await context.Products.AsNoTracking().Where(p => p.WebsiteID == targetId).Select(p => p.Slug).ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var skus = (await context.ProductVariants.AsNoTracking().Where(v => v.WebsiteID == targetId).Select(v => v.Sku).ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;

        foreach (var row in source)
        {
            int? mappedBrand = null;
            if (row.BrandID is int brandId)
            {
                if (!brandMap.TryGetValue(brandId, out var found)) continue;
                mappedBrand = found;
            }

            var copy = new Product
            {
                WebsiteID = targetId,
                BrandID = mappedBrand,
                Slug = Unique(slugs, row.Slug, row.ProductID),
                Title = row.Title,
                ShortDescription = row.ShortDescription,
                Content = row.Content,
                ExpertReview = row.ExpertReview,
                FeaturedImageFileID = row.FeaturedImageFileID,
                ProductType = row.ProductType,
                RequiresShipping = row.RequiresShipping,
                DigitalDownloadUrl = row.DigitalDownloadUrl,
                DigitalServiceUrl = row.DigitalServiceUrl,
                DigitalDeliveryNote = row.DigitalDeliveryNote,
                IsCatalogOnly = row.IsCatalogOnly,
                HasVariants = row.HasVariants,
                Status = row.Status,
                SortOrder = row.SortOrder,
                IsActive = row.IsActive,
                CreatedAt = now,
                UpdatedAt = now,
            };
            foreach (var translation in row.ProductTranslations)
            {
                copy.ProductTranslations.Add(new ProductTranslation
                {
                    LanguageCode = translation.LanguageCode,
                    Title = translation.Title,
                    Slug = translation.Slug,
                    ShortDescription = translation.ShortDescription,
                    Content = translation.Content,
                    ExpertReview = translation.ExpertReview,
                });
            }
            foreach (var variant in row.ProductVariants)
            {
                copy.ProductVariants.Add(new ProductVariant
                {
                    WebsiteID = targetId,
                    Sku = Unique(skus, variant.Sku, variant.ProductVariantID),
                    Title = variant.Title,
                    IsDefault = variant.IsDefault,
                    ImageFileID = variant.ImageFileID,
                    ReferencePrice = variant.ReferencePrice,
                    CompareAtPrice = variant.CompareAtPrice,
                    ReferencePriceUsd = variant.ReferencePriceUsd,
                    CompareAtPriceUsd = variant.CompareAtPriceUsd,
                    OverridePrice = variant.OverridePrice,
                    Weight = variant.Weight,
                    Barcode = variant.Barcode,
                    IsActive = variant.IsActive,
                    CreatedAt = now,
                });
            }
            foreach (var map in row.ProductCategoryMaps)
            {
                if (!categoryMap.TryGetValue(map.ProductCategoryID, out var categoryId)) continue;
                copy.ProductCategoryMaps.Add(new ProductCategoryMap
                {
                    ProductCategoryID = categoryId,
                    IsPrimary = map.IsPrimary,
                });
            }

            context.Products.Add(copy);
            await context.SaveChangesAsync(ct);
            result.Products++;
        }
    }

    private static string Unique(HashSet<string> used, string? slug, int sourceId)
    {
        var value = string.IsNullOrWhiteSpace(slug) ? "item" : slug.Trim();
        if (value.Length > 180) value = value[..180].Trim('-');
        if (used.Add(value)) return value;
        var alt = $"{value}-{sourceId}";
        if (alt.Length > 200) alt = alt[..200];
        if (!used.Add(alt))
        {
            alt = $"{value}-copy-{sourceId}";
            if (alt.Length > 200) alt = alt[..200];
            used.Add(alt);
        }
        return alt;
    }
}
