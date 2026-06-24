using Dotnetable.Application.DTOs;
using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface IContactMessageService
{
    Task<PagedResult<ContactUsMessage>> GetPagedAsync(int? websiteId, GridQuery query, CancellationToken ct = default);
    Task<ContactUsMessage?> GetByIdAsync(int id, CancellationToken ct = default);
    Task SetArchiveAsync(int id, bool archive, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
