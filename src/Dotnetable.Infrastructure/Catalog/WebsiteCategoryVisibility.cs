using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Catalog;

/// <summary>Per-website allow-list for the public product-category tree and the products in it.</summary>
public static class WebsiteCategoryVisibility
{
    /// <summary>Null when the website shows the whole catalog. An empty list shows nothing.</summary>
    public static async Task<List<int>?> SelectionAsync(AppDbContext context, int websiteId, CancellationToken ct = default)
    {
        var limited = await context.Websites.AsNoTracking()
            .Where(w => w.WebsiteID == websiteId)
            .Select(w => (bool?)w.LimitVisibleCategories)
            .FirstOrDefaultAsync(ct) == true;
        if (!limited) return null;

        return await context.WebsiteVisibleProductCategories.AsNoTracking()
            .Where(v => v.WebsiteID == websiteId)
            .Select(v => v.ProductCategoryID)
            .ToListAsync(ct);
    }

    /// <summary>Chosen categories plus every ancestor, so a checked branch can still be opened from the root.</summary>
    public static HashSet<int> KeepWithAncestors(
        IEnumerable<(int Id, int? ParentId)> nodes, IReadOnlyCollection<int> selected)
    {
        var parentOf = new Dictionary<int, int?>();
        foreach (var node in nodes)
            parentOf[node.Id] = node.ParentId;

        var keep = new HashSet<int>();
        foreach (var id in selected)
        {
            if (!parentOf.ContainsKey(id)) continue;
            var current = id;
            for (var depth = 0; depth < 32; depth++)
            {
                if (!keep.Add(current)) break;
                if (!parentOf.TryGetValue(current, out var parent) || parent is not int parentId) break;
                current = parentId;
            }
        }
        return keep;
    }
}
