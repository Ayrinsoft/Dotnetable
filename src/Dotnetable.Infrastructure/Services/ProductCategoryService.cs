using Dotnetable.Application.DTOs;
using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Catalog;
using Dotnetable.Infrastructure.Data;
using Dotnetable.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ProductCategoryService : IProductCategoryService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public ProductCategoryService(IDbContextFactory<AppDbContext> contextFactory) => _contextFactory = contextFactory;

    // ── Admin management ────────────────────────────────────────────

    public async Task<List<ProductCategory>> GetAllAsync(int? websiteId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.ProductCategories.AsNoTracking();
        if (websiteId is int wid)
        {
            wid = await PlatformCatalog.CatalogWebsiteIdAsync(_context, wid, ct);
            q = q.Where(c => c.WebsiteID == wid);
        }
        return await q.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync(ct);
    }

    public async Task<PagedResult<ProductCategory>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var q = _context.ProductCategories.AsNoTracking();
        if (websiteId is int wid)
        {
            wid = await PlatformCatalog.CatalogWebsiteIdAsync(_context, wid, ct);
            q = q.Where(c => c.WebsiteID == wid);
        }

        if (query.GetSearch(nameof(ProductCategory.Name)) is string name)
            q = q.Where(c => c.Name.Contains(name));
        if (query.GetSearch(nameof(ProductCategory.Slug)) is string slug)
            q = q.Where(c => c.Slug.Contains(slug));
        if (query.GetSearch(nameof(ProductCategory.IsActive)) is string active && bool.TryParse(active, out var isActive))
            q = q.Where(c => c.IsActive == isActive);

        var total = await q.CountAsync(ct);
        var items = await q
            .ApplyOrderBy(query.OrderBy, nameof(ProductCategory.SortOrder))
            .Skip(query.Skip).Take(query.Take)
            .ToListAsync(ct);

        return new PagedResult<ProductCategory> { Items = items, TotalCount = total };
    }

    public async Task<ProductCategory?> GetByIdAsync(int productCategoryId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.ProductCategories.FindAsync([productCategoryId], ct);
    }

    public async Task<ProductCategory> CreateAsync(ProductCategory category, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        NormalizeOptionalFks(category);
        await PlatformCatalog.EnsureOwnerAsync(_context, category.WebsiteID, ct);
        await EnsureParentAsync(_context, category, ct);
        _context.ProductCategories.Add(category);
        await _context.SaveChangesAsync(ct);
        // Blazor Server keeps AppDbContext for the whole circuit; detach so a later
        // edit of the same key with a fresh copy does not hit an identity conflict.
        _context.Entry(category).State = EntityState.Detached;
        return category;
    }

    public async Task UpdateAsync(ProductCategory category, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        NormalizeOptionalFks(category);
        await PlatformCatalog.EnsureOwnerAsync(_context, category.WebsiteID, ct);
        await EnsureParentAsync(_context, category, ct);
        _context.ProductCategories.Update(category);
        await _context.SaveChangesAsync(ct);
        _context.Entry(category).State = EntityState.Detached;
    }

    public async Task DeleteAsync(int productCategoryId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var category = await _context.ProductCategories
            .Include(c => c.ProductCategoryTranslations)
            .Include(c => c.ProductCategoryMaps)
            .Include(c => c.ProductCategoryAttributes)
            .Include(c => c.InverseParentCategory)
            .FirstOrDefaultAsync(c => c.ProductCategoryID == productCategoryId, ct);
        if (category is null) return;
        await PlatformCatalog.EnsureOwnerAsync(_context, category.WebsiteID, ct);

        // Re-parent children so the self FK never dangles, drop product links and translations.
        foreach (var child in category.InverseParentCategory)
            child.ParentCategoryID = category.ParentCategoryID;

        _context.ProductCategoryAttributes.RemoveRange(category.ProductCategoryAttributes);
        _context.ProductCategoryMaps.RemoveRange(category.ProductCategoryMaps);
        _context.ProductCategoryTranslations.RemoveRange(category.ProductCategoryTranslations);
        _context.ProductCategories.Remove(category);
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>MudSelect / clearable binds sometimes emit 0 instead of null for optional int FKs.</summary>
    private static void NormalizeOptionalFks(ProductCategory category)
    {
        if (category.ParentCategoryID is 0) category.ParentCategoryID = null;
    }


    // ── Translations ────────────────────────────────────────────────

    public async Task<List<ProductCategoryTranslation>> GetTranslationsAsync(int productCategoryId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.ProductCategoryTranslations.AsNoTracking()
            .Where(t => t.ProductCategoryID == productCategoryId)
            .ToListAsync(ct);
    }

    public async Task SetTranslationsAsync(int productCategoryId, IReadOnlyDictionary<string, (string Name, string Slug)> byLanguage, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.ProductCategoryTranslations
            .Where(t => t.ProductCategoryID == productCategoryId)
            .ToListAsync(ct);

        foreach (var (languageCode, value) in byLanguage)
        {
            var current = existing.FirstOrDefault(t =>
                string.Equals(t.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(value.Name))
            {
                if (current is not null) _context.ProductCategoryTranslations.Remove(current);
                continue;
            }

            var slug = string.IsNullOrWhiteSpace(value.Slug) ? value.Name.Trim() : value.Slug.Trim();
            if (current is null)
                _context.ProductCategoryTranslations.Add(new ProductCategoryTranslation
                {
                    ProductCategoryID = productCategoryId,
                    LanguageCode = languageCode,
                    Name = value.Name.Trim(),
                    Slug = slug,
                });
            else
            {
                current.Name = value.Name.Trim();
                current.Slug = slug;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    // ── Category ↔ attribute definitions ────────────────────────────

    public async Task<List<int>> GetAttributeDefinitionIdsAsync(int productCategoryId, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        return await _context.ProductCategoryAttributes.AsNoTracking()
            .Where(a => a.ProductCategoryID == productCategoryId)
            .OrderBy(a => a.SortOrder)
            .Select(a => a.AttributeDefinitionID)
            .ToListAsync(ct);
    }

    public async Task SetAttributeDefinitionIdsAsync(
        int productCategoryId,
        IReadOnlyList<int> attributeDefinitionIds,
        CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await _context.ProductCategoryAttributes
            .Where(a => a.ProductCategoryID == productCategoryId)
            .ToListAsync(ct);

        // Preserve order from the caller; drop duplicates and zeros.
        var orderedIds = attributeDefinitionIds
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        // Only allow non-variant, active-or-existing definitions on the same website as the category.
        var categoryWebsiteId = await _context.ProductCategories.AsNoTracking()
            .Where(c => c.ProductCategoryID == productCategoryId)
            .Select(c => (int?)c.WebsiteID)
            .FirstOrDefaultAsync(ct);
        if (categoryWebsiteId is null) return;

        if (orderedIds.Count > 0)
        {
            var allowed = await _context.AttributeDefinitions.AsNoTracking()
                .Where(d => orderedIds.Contains(d.AttributeDefinitionID)
                            && d.WebsiteID == categoryWebsiteId.Value
                            && !d.IsVariantAttribute)
                .Select(d => d.AttributeDefinitionID)
                .ToListAsync(ct);
            var allowedSet = allowed.ToHashSet();
            orderedIds = orderedIds.Where(id => allowedSet.Contains(id)).ToList();
        }

        var wanted = orderedIds.ToHashSet();
        _context.ProductCategoryAttributes.RemoveRange(existing.Where(e => !wanted.Contains(e.AttributeDefinitionID)));

        for (var i = 0; i < orderedIds.Count; i++)
        {
            var defId = orderedIds[i];
            var row = existing.FirstOrDefault(e => e.AttributeDefinitionID == defId);
            if (row is null)
            {
                _context.ProductCategoryAttributes.Add(new ProductCategoryAttribute
                {
                    ProductCategoryID = productCategoryId,
                    AttributeDefinitionID = defId,
                    SortOrder = i,
                });
            }
            else
            {
                row.SortOrder = i;
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<AttributeDefinition>> GetAttributesForCategoryAsync(
        int productCategoryId,
        bool includeAncestors = true,
        CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        var categoryIds = includeAncestors
            ? await ResolveCategoryAncestorChainAsync(_context, productCategoryId, ct)
            : new List<int> { productCategoryId };
        if (categoryIds.Count == 0) return new List<AttributeDefinition>();

        // Walk from root → leaf so more specific category mappings win on conflicts (re-add moves to end).
        categoryIds.Reverse();

        var orderedDefIds = new List<int>();
        var seen = new HashSet<int>();
        foreach (var catId in categoryIds)
        {
            var ids = await _context.ProductCategoryAttributes.AsNoTracking()
                .Where(a => a.ProductCategoryID == catId)
                .OrderBy(a => a.SortOrder)
                .Select(a => a.AttributeDefinitionID)
                .ToListAsync(ct);
            foreach (var id in ids)
            {
                if (seen.Add(id))
                    orderedDefIds.Add(id);
            }
        }

        if (orderedDefIds.Count == 0) return new List<AttributeDefinition>();

        var defs = await _context.AttributeDefinitions.AsNoTracking()
            .Include(d => d.AttributeOptions)
            .Include(d => d.AttributeDefinitionTranslations)
            .Where(d => orderedDefIds.Contains(d.AttributeDefinitionID) && d.Active && !d.IsVariantAttribute)
            .ToListAsync(ct);

        var byId = defs.ToDictionary(d => d.AttributeDefinitionID);
        return orderedDefIds
            .Where(id => byId.ContainsKey(id))
            .Select(id => byId[id])
            .ToList();
    }

    public async Task<List<CategoryAttributeFilterDto>> GetFilterableAttributesBySlugAsync(
        int websiteId,
        string categorySlug,
        string? languageCode = null,
        CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);

        if (string.IsNullOrWhiteSpace(categorySlug)) return new List<CategoryAttributeFilterDto>();
        websiteId = await PlatformCatalog.CatalogWebsiteIdAsync(_context, websiteId, ct);

        var category = await _context.ProductCategories.AsNoTracking()
            .Include(c => c.ProductCategoryTranslations)
            .FirstOrDefaultAsync(c => c.WebsiteID == websiteId && c.IsActive &&
                (c.Slug == categorySlug || c.ProductCategoryTranslations.Any(t => t.Slug == categorySlug)), ct);
        if (category is null) return new List<CategoryAttributeFilterDto>();

        var defs = await GetAttributesForCategoryAsync(category.ProductCategoryID, includeAncestors: true, ct);
        return defs
            .Where(d => d.IsFilterable)
            .Select(d =>
            {
                var (name, unit) = LocalizedAttribute(d, languageCode);
                return new CategoryAttributeFilterDto
                {
                    AttributeDefinitionID = d.AttributeDefinitionID,
                    Code = d.Code,
                    Name = name,
                    Unit = unit,
                    InputType = d.InputType,
                    SortOrder = d.SortOrder,
                    Options = d.AttributeOptions
                        .OrderBy(o => o.SortOrder).ThenBy(o => o.Value)
                        .Select(o => new CategoryAttributeFilterOptionDto
                        {
                            AttributeOptionID = o.AttributeOptionID,
                            Value = o.Value,
                            ColorHex = o.ColorHex,
                            SortOrder = o.SortOrder,
                        })
                        .ToList(),
                };
            })
            .ToList();
    }

    /// <summary>Category + ancestors from leaf to root (leaf first).</summary>
    private async Task<List<int>> ResolveCategoryAncestorChainAsync(AppDbContext _context, int productCategoryId, CancellationToken ct)
    {
        var result = new List<int>();
        var currentId = (int?)productCategoryId;
        var guard = 0;
        while (currentId is int id && id > 0 && guard++ < 64)
        {
            result.Add(id);
            currentId = await _context.ProductCategories.AsNoTracking()
                .Where(c => c.ProductCategoryID == id)
                .Select(c => c.ParentCategoryID)
                .FirstOrDefaultAsync(ct);
        }
        return result;
    }

    private static (string Name, string? Unit) LocalizedAttribute(AttributeDefinition d, string? languageCode)
    {
        if (!string.IsNullOrWhiteSpace(languageCode))
        {
            var t = d.AttributeDefinitionTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));
            if (t is not null && !string.IsNullOrWhiteSpace(t.Name))
                return (t.Name, string.IsNullOrWhiteSpace(t.Unit) ? d.Unit : t.Unit);
        }
        return (d.Name, d.Unit);
    }

    // ── Public read ─────────────────────────────────────────────────

    public async Task<List<ProductCategoryDto>> GetTreeAsync(int websiteId, string? languageCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        websiteId = await PlatformCatalog.CatalogWebsiteIdAsync(_context, websiteId, ct);

        var categories = await _context.ProductCategories.AsNoTracking()
            .Where(c => c.WebsiteID == websiteId && c.IsActive)
            .Include(c => c.ProductCategoryTranslations)
            .Include(c => c.ImageFile)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .ToListAsync(ct);

        var byParent = categories.ToLookup(c => c.ParentCategoryID);

        return BuildChildren(null, byParent, languageCode);
    }

    public async Task<ProductCategoryDto?> GetBySlugAsync(int websiteId, string slug, string? languageCode = null, CancellationToken ct = default)
    {
        await using var _context = await _contextFactory.CreateDbContextAsync(ct);
        websiteId = await PlatformCatalog.CatalogWebsiteIdAsync(_context, websiteId, ct);

        var category = await _context.ProductCategories.AsNoTracking()
            .Include(c => c.ProductCategoryTranslations)
            .Include(c => c.ImageFile)
            .FirstOrDefaultAsync(c => c.WebsiteID == websiteId && c.IsActive &&
                (c.Slug == slug || c.ProductCategoryTranslations.Any(t => t.Slug == slug)), ct);
        return category is null ? null : Project(category, Array.Empty<ProductCategoryDto>(), languageCode);
    }

    // ── Projection helpers ──────────────────────────────────────────

    private static List<ProductCategoryDto> BuildChildren(
        int? parentId,
        ILookup<int?, ProductCategory> byParent,
        string? languageCode)
    {
        return byParent[parentId]
            .Select(c => Project(c, BuildChildren(c.ProductCategoryID, byParent, languageCode), languageCode))
            .ToList();
    }

    private static ProductCategoryDto Project(ProductCategory c, IReadOnlyList<ProductCategoryDto> children, string? languageCode)
    {
        var (name, slug) = Localized(c, languageCode);
        return new ProductCategoryDto
        {
            ProductCategoryID = c.ProductCategoryID,
            ParentCategoryID = c.ParentCategoryID,
            Name = name,
            Slug = slug,
            ImageUrl = c.ImageFile?.ThumbnailCDN ?? c.ImageFile?.CNDUrl,
            SortOrder = c.SortOrder,
            Children = children,
        };
    }

    private static (string Name, string Slug) Localized(ProductCategory c, string? languageCode)
    {
        if (!string.IsNullOrWhiteSpace(languageCode))
        {
            var t = c.ProductCategoryTranslations.FirstOrDefault(x =>
                string.Equals(x.LanguageCode, languageCode, StringComparison.OrdinalIgnoreCase));
            if (t is not null && !string.IsNullOrWhiteSpace(t.Name))
                return (t.Name, string.IsNullOrWhiteSpace(t.Slug) ? c.Slug : t.Slug);
        }
        return (c.Name, c.Slug);
    }

    public async Task<IReadOnlyList<ProductCategoryPickDto>> GetChildrenAsync(
        int websiteId, int? parentCategoryId, string? search, int take = 40, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        websiteId = await PlatformCatalog.CatalogWebsiteIdAsync(context, websiteId, ct);
        if (parentCategoryId is 0) parentCategoryId = null;
        var limit = take < 1 ? 40 : Math.Min(take, 100);

        var q = context.ProductCategories.AsNoTracking()
            .Where(c => c.WebsiteID == websiteId && c.IsActive && c.ParentCategoryID == parentCategoryId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var needle = search.Trim();
            q = q.Where(c => c.Name.Contains(needle));
        }

        return await q.OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Take(limit)
            .Select(c => new ProductCategoryPickDto
            {
                ProductCategoryID = c.ProductCategoryID,
                ParentCategoryID = c.ParentCategoryID,
                Name = c.Name,
                Path = c.Name,
                IsActive = c.IsActive,
                HasChildren = context.ProductCategories.Any(ch => ch.ParentCategoryID == c.ProductCategoryID && ch.IsActive),
            })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductCategoryPickDto>> SearchAsync(
        int websiteId, string term, int take = 25, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(term)) return Array.Empty<ProductCategoryPickDto>();
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        websiteId = await PlatformCatalog.CatalogWebsiteIdAsync(context, websiteId, ct);
        var needle = term.Trim();
        var limit = take < 1 ? 25 : Math.Min(take, 50);

        var matches = await context.ProductCategories.AsNoTracking()
            .Where(c => c.WebsiteID == websiteId && c.IsActive && (c.Name.Contains(needle) || c.Slug.Contains(needle)))
            .OrderBy(c => c.Name)
            .Take(limit)
            .Select(c => new Node(c.ProductCategoryID, c.ParentCategoryID, c.Name, c.IsActive))
            .ToListAsync(ct);
        if (matches.Count == 0) return Array.Empty<ProductCategoryPickDto>();

        var known = matches.ToDictionary(n => n.Id);
        var pending = matches.Where(n => n.ParentId is int pid && !known.ContainsKey(pid)).Select(n => n.ParentId!.Value).ToHashSet();
        var guard = 0;
        while (pending.Count > 0 && guard++ < 8)
        {
            var batch = await context.ProductCategories.AsNoTracking()
                .Where(c => pending.Contains(c.ProductCategoryID))
                .Select(c => new Node(c.ProductCategoryID, c.ParentCategoryID, c.Name, c.IsActive))
                .ToListAsync(ct);
            pending.Clear();
            foreach (var node in batch)
            {
                known[node.Id] = node;
                if (node.ParentId is int pid && !known.ContainsKey(pid))
                    pending.Add(pid);
            }
        }

        var result = new List<ProductCategoryPickDto>();
        foreach (var match in matches)
        {
            if (!AncestorsActive(match, known)) continue;
            result.Add(new ProductCategoryPickDto
            {
                ProductCategoryID = match.Id,
                ParentCategoryID = match.ParentId,
                Name = match.Name,
                Path = PathOf(match, known),
                IsActive = true,
                HasChildren = false,
            });
        }
        return result;
    }

    public async Task<IReadOnlyList<ProductCategoryPickDto>> GetAncestorChainAsync(int productCategoryId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var chain = new List<ProductCategoryPickDto>();
        var current = productCategoryId;
        for (var i = 0; i < 8; i++)
        {
            var node = await context.ProductCategories.AsNoTracking()
                .Where(c => c.ProductCategoryID == current)
                .Select(c => new ProductCategoryPickDto
                {
                    ProductCategoryID = c.ProductCategoryID,
                    ParentCategoryID = c.ParentCategoryID,
                    Name = c.Name,
                    IsActive = c.IsActive,
                })
                .FirstOrDefaultAsync(ct);
            if (node is null) break;
            chain.Insert(0, node);
            if (node.ParentCategoryID is not int parent) break;
            current = parent;
        }

        var path = "";
        foreach (var node in chain)
        {
            path = path.Length == 0 ? node.Name : path + " > " + node.Name;
            node.Path = path;
        }
        return chain;
    }

    public async Task<int> CountAsync(int websiteId, bool activeOnly = true, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        websiteId = await PlatformCatalog.CatalogWebsiteIdAsync(context, websiteId, ct);
        var q = context.ProductCategories.AsNoTracking().Where(c => c.WebsiteID == websiteId);
        if (activeOnly) q = q.Where(c => c.IsActive);
        return await q.CountAsync(ct);
    }

    public async Task SetActiveAsync(int productCategoryId, bool isActive, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var category = await context.ProductCategories.FirstOrDefaultAsync(c => c.ProductCategoryID == productCategoryId, ct);
        if (category is null) return;
        await PlatformCatalog.EnsureOwnerAsync(context, category.WebsiteID, ct);
        category.IsActive = isActive;
        await context.SaveChangesAsync(ct);
    }

    public async Task DetachFromParentAsync(int productCategoryId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var category = await context.ProductCategories.FirstOrDefaultAsync(c => c.ProductCategoryID == productCategoryId, ct);
        if (category is null) return;
        await PlatformCatalog.EnsureOwnerAsync(context, category.WebsiteID, ct);
        category.ParentCategoryID = null;
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteBranchAsync(int productCategoryId, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var root = await context.ProductCategories.AsNoTracking()
            .Where(c => c.ProductCategoryID == productCategoryId)
            .Select(c => new { c.ProductCategoryID, c.WebsiteID })
            .FirstOrDefaultAsync(ct);
        if (root is null) return;
        await PlatformCatalog.EnsureOwnerAsync(context, root.WebsiteID, ct);

        var rows = await context.ProductCategories.AsNoTracking()
            .Where(c => c.WebsiteID == root.WebsiteID)
            .Select(c => new { c.ProductCategoryID, c.ParentCategoryID })
            .ToListAsync(ct);
        var children = rows.ToLookup(r => r.ParentCategoryID);
        var ids = new List<int>();
        var queue = new Queue<int>();
        queue.Enqueue(root.ProductCategoryID);
        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            ids.Add(id);
            foreach (var child in children[id])
                queue.Enqueue(child.ProductCategoryID);
        }

        if (await context.ProductCategoryMaps.AnyAsync(m => ids.Contains(m.ProductCategoryID), ct))
            throw new InvalidOperationException("This branch is used by products. Deactivate it so it stays hidden, or move the products first.");

        await context.ProductCategoryAttributes.Where(a => ids.Contains(a.ProductCategoryID)).ExecuteDeleteAsync(ct);
        await context.ProductCategoryTranslations.Where(t => ids.Contains(t.ProductCategoryID)).ExecuteDeleteAsync(ct);
        await context.ProductCategoryRelations.Where(r => ids.Contains(r.RelatedProductCategoryID)).ExecuteDeleteAsync(ct);
        await context.MarketplaceChannelCategories.Where(r => ids.Contains(r.ProductCategoryID)).ExecuteDeleteAsync(ct);
        await context.MenuItems.Where(m => m.ProductCategoryID != null && ids.Contains(m.ProductCategoryID.Value))
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.ProductCategoryID, (int?)null), ct);
        await context.PriceLists.Where(p => p.ProductCategoryID != null && ids.Contains(p.ProductCategoryID.Value))
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.ProductCategoryID, (int?)null), ct);
        await context.CatalogRequests.Where(r => r.ParentCategoryID != null && ids.Contains(r.ParentCategoryID.Value))
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.ParentCategoryID, (int?)null), ct);

        var remaining = ids.ToHashSet();
        while (remaining.Count > 0)
        {
            var leaves = remaining.Where(id => !rows.Any(r => r.ParentCategoryID == id && remaining.Contains(r.ProductCategoryID))).ToList();
            if (leaves.Count == 0) leaves = remaining.ToList();
            await context.ProductCategories.Where(c => leaves.Contains(c.ProductCategoryID)).ExecuteDeleteAsync(ct);
            foreach (var id in leaves) remaining.Remove(id);
        }
    }

    private static async Task EnsureParentAsync(AppDbContext context, ProductCategory category, CancellationToken ct)
    {
        if (category.ParentCategoryID is not int parentId) return;
        if (parentId == category.ProductCategoryID)
            throw new InvalidOperationException("A category cannot be its own parent.");

        var parentWebsite = await context.ProductCategories.AsNoTracking()
            .Where(c => c.ProductCategoryID == parentId)
            .Select(c => (int?)c.WebsiteID)
            .FirstOrDefaultAsync(ct);
        if (parentWebsite is null || parentWebsite != category.WebsiteID)
            throw new InvalidOperationException("Parent category was not found on this website.");

        if (category.ProductCategoryID <= 0) return;
        var current = parentId;
        for (var i = 0; i < 32; i++)
        {
            if (current == category.ProductCategoryID)
                throw new InvalidOperationException("A category cannot be moved under one of its own children.");
            var parent = await context.ProductCategories.AsNoTracking()
                .Where(c => c.ProductCategoryID == current)
                .Select(c => c.ParentCategoryID)
                .FirstOrDefaultAsync(ct);
            if (parent is not int next) return;
            current = next;
        }
    }

    private static bool AncestorsActive(Node node, Dictionary<int, Node> known)
    {
        var current = node;
        var guard = 0;
        while (current.ParentId is int pid && known.TryGetValue(pid, out var parent) && guard++ < 16)
        {
            if (!parent.Active) return false;
            current = parent;
        }
        return true;
    }

    private static string PathOf(Node node, Dictionary<int, Node> known)
    {
        var parts = new List<string> { node.Name };
        var current = node;
        var guard = 0;
        while (current.ParentId is int pid && known.TryGetValue(pid, out var parent) && guard++ < 16)
        {
            parts.Insert(0, parent.Name);
            current = parent;
        }
        return string.Join(" > ", parts);
    }

    private sealed record Node(int Id, int? ParentId, string Name, bool Active);
}
