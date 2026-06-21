using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface IProductService
{
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Product?> GetBySlugAsync(int websiteId, string slug, int languageId, CancellationToken ct = default);
    Task<IEnumerable<Product>> GetByWebsiteAsync(int websiteId, bool activeOnly = true, CancellationToken ct = default);
    Task<Product> CreateAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
