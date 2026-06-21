using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context) => _context = context;

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.Products.FindAsync([id], ct);

    public async Task<Product?> GetBySlugAsync(int websiteId, string slug, int languageId, CancellationToken ct = default) =>
        await _context.Products.FirstOrDefaultAsync(
            p => p.WebsiteId == websiteId && p.Slug == slug && p.LanguageId == languageId, ct);

    public async Task<IEnumerable<Product>> GetByWebsiteAsync(int websiteId, bool activeOnly = true, CancellationToken ct = default)
    {
        var q = _context.Products.Where(p => p.WebsiteId == websiteId);
        if (activeOnly) q = q.Where(p => p.IsActive);
        return await q.OrderBy(p => p.Name).ToListAsync(ct);
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync(ct);
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        product.UpdatedAt = DateTime.UtcNow;
        _context.Products.Update(product);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var product = await _context.Products.FindAsync([id], ct);
        if (product is null) return;
        _context.Products.Remove(product);
        await _context.SaveChangesAsync(ct);
    }
}
