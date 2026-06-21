using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface IWebsiteService
{
    Task<Website?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Website?> GetByDomainAsync(string domain, CancellationToken ct = default);
    Task<IEnumerable<Website>> GetAllAsync(CancellationToken ct = default);
    Task<Website> CreateAsync(Website website, CancellationToken ct = default);
    Task UpdateAsync(Website website, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
